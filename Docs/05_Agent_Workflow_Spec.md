# Agent Workflow Specification

## Purpose

This document defines the end-to-end processing pipeline of the GFriend AI Agent — from receiving an Excel input file to producing validated GFriend automation scripts. Each step includes inputs, outputs, logic, error handling, and the responsible code module.

---

## End-to-End Flow

```
Step 1: Receive Input
Step 2: Parse Excel
Step 3: Validate Input
Step 4: Normalize TCs
Step 5: Classify Domain (per TC)
Step 6: Retrieve Context from Knowledge Base
Step 7: Build Generation Prompt
Step 8: Generate Script via LLM
Step 9: Validate Generated Script
Step 10: Write Output Files
Step 11: Generate Report
```

---

## Step 1 — Receive Input

**Trigger:** CLI command, FastAPI endpoint, or VS Code extension call

**CLI:**
```bash
python agent/generate_scripts.py \
    --input testcases.xlsx \
    --output ./scripts/ \
    [--tc-ids TC-001,TC-002]  # optional: process specific TCs only
    [--domain Web]            # optional: override domain for all TCs
```

**FastAPI:**
```
POST /generate-from-excel
Content-Type: multipart/form-data
Body: file=@testcases.xlsx, tc_ids=TC-001,TC-002 (optional)
```

**Inputs:**
- `testcases.xlsx` or `.csv` (TestRail export or custom template)
- Optional filter: comma-separated TC IDs to process

**Outputs:** Passes file path / DataFrame to Step 2

---

## Step 2 — Parse Excel

**Module:** `agent/excel_parser.py`  
**Function:** `parse_excel(filepath: str) -> list[dict]`

**Logic:**
1. Load file with pandas: `pd.read_excel(filepath)` or `pd.read_csv(filepath)`
2. Normalize column names: strip whitespace, standardize case
3. Detect format using column signature:
   - TestRail: contains columns `ID`, `Title`, `Steps`, `Expected Result`
   - Custom Template: contains `TC_ID`, `TC_Title`, `Step_Action`, `Step_Expected_Result`
4. If TestRail format:
   - Detect sub-format A (single row per TC with newline-delimited steps) vs sub-format B (one row per step)
   - For sub-format A: split `Steps` and `Expected Result` by numbered lines
5. Return list of raw row dicts

**Error handling:**
- Unrecognized format → raise `InputFormatError` with guidance message
- Empty file → raise `EmptyInputError`
- Missing required column → raise `MissingColumnError(column_name)`

---

## Step 3 — Validate Input

**Module:** `agent/validator.py` (input validation section)  
**Function:** `validate_input(raw_rows: list[dict]) -> ValidationResult`

**Checks:**
| Check | Logic | On Failure |
|-------|-------|-----------|
| TC_ID present | `TC_ID` not blank | Skip row, log WARNING |
| TC has at least one step | TC_ID has ≥1 step row | Skip TC, log ERROR |
| TC_Title present | Not blank | Use TC_ID as title, log WARNING |
| Step_Action not blank | Each step has action text | Skip step, log WARNING |
| Domain value valid | In accepted domain list | Default to `Multi_Domain`, log WARNING |

**Output:**
- `valid_tcs`: list of TCs that passed validation
- `skipped_tcs`: list with reasons
- `warnings`: list of non-blocking issues logged

---

## Step 4 — Normalize TCs

**Module:** `agent/tc_normalizer.py`  
**Function:** `normalize(raw_rows: list[dict]) -> list[NormalizedTC]`

**Logic:**
1. Group raw rows by `TC_ID` (handles both multi-row per TC and single-row formats)
2. For each group, construct a `NormalizedTC` object:
   - Extract preamble fields: `tc_id`, `tc_title`, `domain`, `dut_type`, `dut_address`, `tc_type`, `priority`, `preconditions`, `variables_needed`, `notes`
   - Construct ordered `list[TestStep]` with `step_number`, `action`, `expected_result`, `library_hint`
3. Sort steps by `step_number`
4. If `variables_needed` column has values, parse comma-separated list

**Output:** `list[NormalizedTC]` — one per TC_ID

---

## Step 5 — Domain Classification

**Module:** `agent/domain_classifier.py`  
**Function:** `classify(tc: NormalizedTC) -> list[str]`

**Logic (priority order):**

1. **Explicit domain field**: If `tc.domain` is set and recognized → map directly to library list
2. **Step-level library hints**: Collect all non-null `step.library_hint` values → add those libraries
3. **Keyword matching**: Run TC title + all step actions through a domain keyword dictionary:
   ```python
   DOMAIN_KEYWORDS = {
       "Web": ["browser", "website", "URL", "HTTP", "login page", "web portal"],
       "Android": ["Android", "mobile", "app", "phone", "tablet", "Play Store"],
       "JediOmni": ["printer panel", "OCP", "touchscreen", "home screen", "sign in to device"],
       "Hallasan": ["print job", "driver", "print document", "send to printer"],
       "REST": ["API", "endpoint", "GET", "POST", "status code", "JSON response"],
       "SSH": ["SSH", "shell command", "terminal", "Linux", "bash command"],
       ...
   }
   ```
4. **Semantic fallback**: If no keywords match, embed TC description and query ChromaDB `kt_docs` collection filtered by domain metadata

**Output:** `list[str]` — e.g., `["Web"]`, `["Android", "JediOmni"]`, `["REST"]`

---

## Step 6 — Retrieve Context from Knowledge Base

**Module:** `agent/retriever.py`  
**Function:** `retrieve_context(tc: NormalizedTC, libraries: list[str]) -> RetrievedContext`

**Query 1 — Keyword retrieval (per library):**
```python
for lib in libraries:
    results = chromadb_client.get_collection("gfk_keywords").query(
        query_texts=[f"{tc.tc_title}. {' '.join(step.action for step in tc.steps)}"],
        n_results=TOP_K_KEYWORDS,
        where={"library": {"$in": [f"GFK.{lib}"]}}
    )
```

**Query 2 — Similar scripts:**
```python
results = chromadb_client.get_collection("gf_scripts").query(
    query_texts=[f"{tc.tc_title}. {tc.preconditions}. {all_steps_text}"],
    n_results=5,
    where={"libraries": {"$contains": libraries[0]}}
)
```

**Query 3 — KT docs (framework rules):**
```python
# Always include core scripting rules — use a fixed set of IDs
kt_sections = ["using_statement", "variables_static", "flow_control_if", "return_statements"]
results = chromadb_client.get_collection("kt_docs").get(ids=kt_sections)
```

**Query 4 — DAT framework context (for printer/device TCs):**
```python
# If any printer/device libraries are involved, retrieve relevant DAT architecture context
printer_libs = ["JediOmni", "OXPD", "Hallasan", "Dune", "Sirius", "BadgeBox"]
if any(lib in libraries for lib in printer_libs):
    dat_context = chromadb_client.get_collection("dat_framework").query(
        query_texts=[f"{tc.tc_title}. Device automation. Control panel interaction."],
        n_results=3,
        where={"doc_type": "dat_framework"}
    )
```

**Query 5 — Per-step keyword lookup:**
```python
for step in tc.steps:
    step_keywords = chromadb_client.get_collection("gfk_keywords").query(
        query_texts=[step.action],
        n_results=3,
        where={"library": {"$in": [f"GFK.{lib}" for lib in libraries]}}
    )
    step.retrieved_keywords = step_keywords
```

**Output:** `RetrievedContext` containing:
- `keyword_definitions`: list of keyword doc strings
- `similar_scripts`: list of past script chunks
- `kt_rules`: framework rule excerpts
- `dat_context`: DAT framework context (if printer/device domain)
- `per_step_keywords`: dict mapping step_number → list of keyword suggestions

---

## Step 7 — Build Generation Prompt

**Module:** `agent/prompt_builder.py`  
**Function:** `build_prompt(tc: NormalizedTC, context: RetrievedContext) -> str`

**Prompt construction:**
```
[SYSTEM INSTRUCTIONS]
- GFriend framework rules (condensed from kt_rules)
- "Only use keywords from the provided definitions below"
- "Output ONLY the GFriend script — no explanation, no markdown fences"

[KEYWORD DEFINITIONS]
(deduplicated, most relevant first)

[SIMILAR SCRIPTS FOR REFERENCE]
(top 2-3 similar past scripts, truncated to fit token budget)

[TEST CASE]
ID: {tc.tc_id}
Title: {tc.tc_title}
Domain: {domain}
Device: {tc.dut_type} at {tc.dut_address or 'TBD'}
Preconditions:
{tc.preconditions}

Test Steps:
{numbered steps with expected results, aligned}

Known variables needed: {tc.variables_needed}

[GENERATION INSTRUCTION]
Generate a complete GFriend automation script. Include:
1. Variable declarations for all values that might change per environment
2. using statements for all required libraries
3. One test case block matching the TC title
4. Keywords mapped to each test step in order
5. Appropriate flow control (If/While/Repeat) where the TC requires it
6. A Capture Screen Shot before any critical verification step
```

**Token budget management:**
- Max prompt tokens: 3500 (leaving 2048 for generation)
- If over budget: truncate similar scripts first, then keyword definitions (keep at least 5 relevant)
- Never truncate the test case itself

---

## Step 8 — Generate Script via LLM

**Module:** `agent/generator.py`  
**Function:** `generate(prompt: str) -> str`

```python
from openai import OpenAI

client = OpenAI(base_url="http://localhost:1234/v1", api_key="local")

response = client.chat.completions.create(
    model="qwen2.5-coder-7b-instruct",
    messages=[{"role": "user", "content": prompt}],
    temperature=0.1,
    max_tokens=2048,
    top_p=0.95
)
return response.choices[0].message.content
```

**Error handling:**
- Connection refused → `LMStudioNotRunningError` with message to start LM Studio
- Timeout (> 120s) → retry once, then `GenerationTimeoutError`
- Empty response → `GenerationFailedError`

---

## Step 9 — Validate Generated Script

**Module:** `agent/validator.py`  
**Function:** `validate_script(script: str, tc: NormalizedTC, libraries: list[str]) -> ValidationResult`

**Checks:**
| Check | Method | Severity |
|-------|--------|---------|
| `using` statement for each library | Regex search | WARNING |
| Test case block `{...}` balanced | Brace counter | ERROR |
| Variables follow `${...}` syntax | Regex | WARNING |
| No `AlwaysPass` present | String search | ERROR — remove it |
| Keywords in generated script exist in knowledge base | ChromaDB ID lookup | WARNING with annotation |
| Script non-empty and > 5 lines | Length check | ERROR |
| Test case name matches TC title | String match | INFO |

**Annotation strategy:**
- WARNING issues: add inline `// WARNING: {reason}` comment above the flagged line
- ERROR issues (except empty/`AlwaysPass`): add `// ERROR: {reason}` at top of script, script still saved
- `AlwaysPass`: removed from output, error logged

**Output:** `ValidationResult(script: str, issues: list[Issue], is_valid: bool)`

---

## Step 9.5 — Guardrails: Handling Unmapped Test Steps

**Critical Design Decision:** What happens when a test step action has **no equivalent keyword** in any GFriend library?

### Scenarios Where This Occurs

1. **Test step describes a manual verification** not automatable (e.g., "Verify print quality is good")
2. **Test step uses domain-specific terminology** not present in keyword library (e.g., "Activate the duplex finisher")
3. **GFriend library incomplete** — missing keyword for a valid automatable action
4. **Typo or ambiguous step description** in the manual TC

### Guardrail Strategy (4-Tier Fallback)

**Tier 1: Semantic Retrieval Confidence Threshold**
- If top retrieved keyword has similarity score < 0.65 (configurable), flag as low confidence
- LLM receives additional instruction: "Use the closest keyword match but add WARNING comment"

**Tier 2: Comment + Manual Action Placeholder**
- Agent generates:
  ```javascript
  // WARNING: No direct keyword match for step "Verify duplex finisher activates"
  // TODO: Developer — manually implement or identify correct keyword
  // Suggested alternatives: JediOmni.Wait For Text(...)
  Sleep(2)  // Placeholder — replace with actual verification
  ```

**Tier 3: Explicit `// UNIMPLEMENTED` Marker**
- For test steps that are clearly manual-only (e.g., "Inspect paper tray quality"):
  ```javascript
  // STEP 5: Inspect paper tray quality — MANUAL VERIFICATION REQUIRED
  // UNIMPLEMENTED: This step cannot be automated with current GFriend libraries
  ```

**Tier 4: Human-in-the-Loop Notification**
- If > 30% of steps in a TC are unmapped → flag TC as `NEEDS_MANUAL_REVIEW` in `generation_report.json`
- Agent logs unmapped steps to `unmapped_steps.log` for knowledge base enhancement

### Configuration

```python
# agent/config.py
UNMAPPED_STEP_CONFIG = {
    "similarity_threshold": 0.65,  # Below this = low confidence
    "max_unmapped_percentage": 30,  # % of unmapped steps before flagging TC
    "placeholder_action": "Sleep(2)",  # Placeholder keyword for unmapped steps
    "enable_manual_markers": True  # Add UNIMPLEMENTED comments
}
```

### Example: End-to-End Handling

**Input TC Step:**
> "Verify the finisher output tray is full"

**Retrieval Result:**
- Top keyword: `JediOmni.Get Text(finisher_status, ${Buffer})` (similarity: 0.58 — below threshold)

**Generated Script Output:**
```javascript
// WARNING: Low confidence keyword match for step "Verify finisher output tray is full"
// Retrieved keyword similarity: 0.58 (threshold: 0.65)
// TODO: Developer — verify this is the correct keyword or replace with manual verification
JediOmni.Get Text(finisher_status, ${Buffer})
// TODO: Add assertion logic to verify ${Buffer} contains expected value
```

**Report Entry:**
```json
{
  "tc_id": "TC-042",
  "status": "WARNING",
  "issues": [
    {
      "step": 5,
      "action": "Verify the finisher output tray is full",
      "severity": "WARNING",
      "message": "No high-confidence keyword match found (best: 0.58)",
      "suggested_keyword": "JediOmni.Get Text",
      "requires_manual_review": true
    }
  ]
}
```

### Knowledge Base Enhancement Loop

- All unmapped steps logged to `unmapped_steps.log`
- Weekly review: Dev team identifies:
  - Missing keywords → document in GFK.* library
  - Manual-only steps → add to exclusion list
  - Terminology gaps → enhance keyword descriptions with synonyms

---

## Step 10 — Write Output Files

**Module:** `agent/output_writer.py`  
**Function:** `write_output(tc: NormalizedTC, result: ValidationResult, output_dir: str)`

**Output files per TC:**
```
output_dir/
├── TC-001_Web_Login_Test.txt               GFriend script
├── TC-002_Mobile_Print.txt
├── TC-003_REST_API_Login.txt
└── ...
```

**Filename format:** `{TC_ID}_{Sanitized_Title}.txt`
- Sanitize: replace spaces with `_`, remove special chars, max 80 chars

**File header (written at top of each .txt):**
```javascript
// Generated by GFriend AI Agent
// TC ID: {tc_id}
// TC Title: {tc_title}
// Generated: {timestamp}
// Status: DRAFT - REQUIRES DEVELOPER REVIEW
// Validation: {PASSED / WARNING(N) / ERROR(N)}
//
// Review checklist: AI Agent/docs/09_Script_Review_Checklist.md
```

---

## Step 11 — Generate Report

**Module:** `agent/output_writer.py`  
**Function:** `write_report(results: list[ProcessingResult], output_dir: str)`

**Output:** `generation_report.json`

```json
{
  "generated_at": "2026-06-15T10:30:00",
  "total_tcs": 25,
  "successful": 22,
  "warnings": 3,
  "errors": 0,
  "skipped": 0,
  "results": [
    {
      "tc_id": "TC-001",
      "tc_title": "Web Login Test",
      "output_file": "TC-001_Web_Login_Test.txt",
      "domain": "Web",
      "libraries_used": ["Web"],
      "status": "PASSED",
      "issues": [],
      "generation_time_ms": 3200
    },
    {
      "tc_id": "TC-005",
      "tc_title": "CardReader Badge Auth",
      "output_file": "TC-005_CardReader_Badge_Auth.txt",
      "domain": "CardReader",
      "libraries_used": ["JediOmni", "BadgeBox"],
      "status": "WARNING",
      "issues": [
        {"severity": "WARNING", "message": "Keyword BadgeBox.ScanBadge not found in knowledge base — please verify"}
      ],
      "generation_time_ms": 4100
    }
  ]
}
```

---

## Processing Times (Estimated)

| Step | Time per TC |
|------|------------|
| Parse + Normalize | < 100ms |
| Domain classification | < 200ms |
| ChromaDB retrieval | 200–500ms |
| Prompt building | < 100ms |
| LLM generation | 2–8 seconds |
| Validation + write | < 200ms |
| **Total per TC** | **~3–10 seconds** |

For 89 TCs: approximately **5–15 minutes** total.

---

## Error Recovery

| Error | Recovery Action |
|-------|----------------|
| LM Studio not running | Print clear message: "Start LM Studio and load Qwen2.5-Coder-7B-Instruct, then retry" |
| ChromaDB empty | Print: "Run `python scripts/ingest.py` first to populate knowledge base" |
| Invalid Excel format | Print column names found + expected, suggest running `generate_tc_template.py` |
| Generation timeout | Log TC as FAILED, continue with next TC, summarize in report |
| Partial run (some TCs fail) | Continue processing remaining TCs, all results in report |
