# System Architecture — GFriend AI Agent

## Overview

The GFriend AI Agent is a **locally-hosted, multi-layer system** that converts manual test cases (Excel input) into GFriend automation scripts (`.txt` output) using a Retrieval-Augmented Generation (RAG) pipeline grounded in GFriend's keyword libraries, documentation, and past scripts.

---

## Architecture Diagrams

### Visual Diagrams (SVG Format)

The following professional diagrams provide comprehensive visualization of the GFriend AI Agent architecture:

1. **[System Architecture Diagram](diagrams/01_System_Architecture.svg)** — Complete component overview showing all layers from input to output, including the knowledge base layer
2. **[Flow Chart](diagrams/02_Flow_Chart.svg)** — Process flow from test case input through generation to developer review
3. **[Execution Sequence Diagram](diagrams/03_Execution_Sequence_Diagram.svg)** — Detailed component interaction timeline with all processing steps

### High-Level Architecture Overview

```text
┌─────────────────────────────────────────────────────────────────────┐
│                          INPUT LAYER                                │
│  ┌───────────────────────┐    ┌──────────────────────────────────┐  │
│  │ TestRail Excel Export │    │  GFriend Custom TC Template      │  │
│  │ (.xlsx / .csv)        │    │  (.xlsx via generate_template)   │  │
│  └───────────┬───────────┘    └───────────────┬──────────────────┘  │
│              └──────────────┬─────────────────┘                     │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
┌───────────────────────────────▼─────────────────────────────────────┐
│                       PROCESSING LAYER                              │
│                                                                     │
│   ┌─────────────────────────────────────────────────────────────┐   │
│   │  1. Excel Parser (pandas / openpyxl)                        │   │
│   │     - Detects format: TestRail vs Custom Template           │   │
│   │     - Parses all rows into normalized TC schema (JSON)      │   │
│   └─────────────────────┬───────────────────────────────────────┘   │
│                         │                                           │
│   ┌─────────────────────▼───────────────────────────────────────┐   │
│   │  2. TC Normalizer                                           │   │
│   │     - Groups rows by TC_ID into complete TC objects         │   │
│   │     - Validates required fields                             │   │
│   │     - Produces list of NormalizedTC objects                 │   │
│   └─────────────────────┬───────────────────────────────────────┘   │
│                         │                                           │
│   ┌─────────────────────▼───────────────────────────────────────┐   │
│   │  3. Domain Classifier                                       │   │
│   │     - Uses Domain column (if provided) OR                   │   │
│   │     - Semantic analysis of TC title + steps to infer domain │   │
│   │     - Maps domain → list of required GFK.* libraries        │   │
│   └─────────────────────┬───────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────────┐
│                        RAG LAYER                                    │
│                                                                     │
│   ┌─────────────────────────────────────────────────────────────┐   │
│   │  4. Semantic Retriever (LangChain + ChromaDB)               │   │
│   │     - Embeds TC description + step actions                  │   │
│   │     - Queries ChromaDB for top-K similar:                   │   │
│   │       • GFK.* keyword definitions                           │   │
│   │       • Past GFriend scripts (sample_outputs/)              │   │
│   │       • KT documentation excerpts                           │   │
│   │       • DAT framework architecture (for printer tests)      │   │
│   │       • Variable file patterns                              │   │
│   │     - Returns ranked context chunks                         │   │
│   └─────────────────────┬───────────────────────────────────────┘   │
│                         │                                           │
│   ┌─────────────────────▼───────────────────────────────────────┐   │
│   │  5. Prompt Builder                                          │   │
│   │     - Constructs structured prompt:                         │   │
│   │       [System instructions: GFriend conventions]            │   │
│   │       [Retrieved keyword definitions]                       │   │
│   │       [Retrieved similar scripts]                           │   │
│   │       [Full TC: title, preconditions, steps, expected]      │   │
│   │       [Generation instruction]                              │   │
│   └─────────────────────┬───────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────────┐
│                        MODEL LAYER                                  │
│                                                                     │
│   ┌─────────────────────────────────────────────────────────────┐   │
│   │  6. LLM Inference (LM Studio — localhost:1234)              │   │
│   │     - Model: Qwen2.5-Coder-7B-Instruct (Q4_K_M)             │   │
│   │     - OpenAI-compatible API                                 │   │
│   │     - Temperature: 0.1 (deterministic code generation)      │   │
│   │     - Max tokens: 2048                                      │   │
│   └─────────────────────┬───────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────────┐
│                      POST-PROCESSING LAYER                          │
│                                                                     │
│   ┌─────────────────────────────────────────────────────────────┐   │
│   │  7. Script Validator                                        │   │
│   │     - Checks: all `using` statements present                │   │
│   │     - Checks: test case block { } structure valid           │   │
│   │     - Checks: no hallucinated GFK.* keywords                │   │
│   │     - Checks: variable syntax correct (${Var})              │   │
│   │     - Flags any validation failures for developer attention │   │
│   └─────────────────────┬───────────────────────────────────────┘   │
│                         │                                           │
│   ┌─────────────────────▼───────────────────────────────────────┐   │
│   │  8. Output Writer                                           │   │
│   │     - Writes: TC_ID_Title.txt (GFriend script)              │   │
│   │     - Writes: generation_report.json (metadata + warnings)  │   │
│   │     - Logs: confidence score per generated keyword          │   │
│   └─────────────────────┬───────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────────┐
│                        OUTPUT LAYER                                 │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  sample_outputs/                                             │   │
│  │  ├── TC-001_Web_Login_Test.txt                               │   │
│  │  ├── TC-002_Mobile_Print.txt                                 │   │
│  │  ├── TC-003_REST_API_Login.txt                               │   │
│  │  └── generation_report.json                                  │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Component Details

### 1. Excel Parser

**File:** `agent/excel_parser.py`

**Responsibilities:**

- Auto-detect input format (TestRail vs Custom Template) by checking column header signature
- Parse both single-row-per-TC (TestRail Format A) and multi-row-per-TC (both formats)
- Return list of raw TC dictionaries

**Format detection logic:**

```python
TESTRAIL_SIGNATURE = {"ID", "Title", "Steps", "Expected Result"}
CUSTOM_SIGNATURE = {"TC_ID", "TC_Title", "Step_Action", "Step_Expected_Result"}

def detect_format(df_columns):
    cols = set(df_columns)
    if TESTRAIL_SIGNATURE.issubset(cols):
        return "testrail"
    elif CUSTOM_SIGNATURE.issubset(cols):
        return "custom"
    else:
        raise ValueError("Unrecognized Excel format")
```

---

### 2. TC Normalizer

**File:** `agent/tc_normalizer.py`

**Output schema (NormalizedTC):**

```python
@dataclass
class TestStep:
    step_number: int
    action: str
    expected_result: str
    library_hint: str | None  # from GFriend_Library_Hint column

@dataclass
class NormalizedTC:
    tc_id: str
    tc_title: str
    domain: str
    dut_type: str
    dut_address: str | None
    tc_type: str
    priority: str
    preconditions: str
    steps: list[TestStep]
    variables_needed: list[str]
    notes: str
```

---

### 3. Domain Classifier

**File:** `agent/domain_classifier.py`

**Logic (priority order):**

1. Use `Domain` column value directly if provided and valid
2. Use `GFriend_Library_Hint` on individual steps for mixed-domain TCs
3. If domain is `Multi_Domain` or blank: analyze TC title + step text against a domain keyword dictionary
4. Fall back to semantic similarity against domain description embeddings in ChromaDB

**Domain → Library mapping:**

```python
DOMAIN_TO_LIBRARIES = {
    "Web":           ["Web"],
    "Android":       ["Android"],
    "iOS":           ["IOS"],
    "Printer_Panel": ["JediOmni"],
    "Printer_OXPD":  ["OXPD"],
    "Printer_Hallasan": ["Hallasan"],
    "Mobile_Print":  ["Android", "JediOmni"],
    "REST_API":      ["REST"],
    "SSH":           ["SSH"],
    "CardReader":    ["JediOmni", "BadgeBox"],
    "Windows_Desktop": ["Windows"],
    "Vision":        ["Vision"],
    ...
}
```

---

### 4. Semantic Retriever

**File:** `agent/retriever.py`

**ChromaDB collections:**

| Collection | Contents | Chunk size |
|-----------|---------|-----------|
| `gfk_keywords` | All GFK.* keyword definitions with parameters | Per keyword |
| `gf_scripts` | Past GFriend automation scripts (chunked by test case) | Per test case |
| `kt_docs` | GFriend architecture docs, variable file patterns | 500 tokens |
| `gf_variables` | Common variable files (.gfvar) and their context | Per file |
| `dat_framework` | Device Automation Toolkit (DAT) architecture and device control patterns | Per section |

**Query strategy:**

- Primary query: `"{tc_title}. {preconditions}. {all_steps_concatenated}"`
- Per-step query for step-level keyword retrieval: `"{step_action}"` filtered by domain
- **DAT-specific retrieval**: When domain includes printer/device libraries (`JediOmni`, `OXPD`, `Hallasan`, `Dune`, `Sirius`), also retrieve relevant DAT framework context to understand device control patterns
- Top-K: 10 for full TC context, 5 per step, 3 for DAT framework (if applicable)
- Filter: `where={"domain": {"$in": libraries_for_this_tc}}`

---

### 5. Prompt Builder

**File:** `agent/prompt_builder.py`

**Prompt structure:**

```text
SYSTEM:
You are a GFriend automation script generator. GFriend is a proprietary C#/.NET test
automation framework. Generate ONLY valid GFriend script syntax. Never invent keyword
names — only use keywords from the provided keyword definitions.

GFriend Rules:
- Scripts start with variable declarations (${Var}=value) and using statements
- using <Library> [As <Alias>] [With <DeviceID>] [At <RemoteID>]
- Test case: CaseName\n{\n    Library.Keyword(params)\n}
- Flow control: Repeat:N{}, While:Condition{}, If:Condition{} Fail:{} Error:{}
- ${KEYWORD_RESULT} holds last keyword result (Pass/Fail/Error)
- Never set AlwaysPass flag

RETRIEVED KEYWORDS:
{top_k_keyword_definitions}

RETRIEVED SIMILAR SCRIPTS:
{similar_past_scripts}

[IF printer/device domain]
DEVICE AUTOMATION CONTEXT (DAT Framework):
{dat_framework_context}
Note: GFriend printer libraries (JediOmni, OXPD, Hallasan, etc.) are built on top of the Device 
Automation Toolkit (DAT). DAT handles device discovery, control panel automation, and device 
management. Your generated GFriend scripts use high-level keywords that internally leverage DAT.
[END IF]

TEST CASE TO AUTOMATE:
ID: {tc_id}
Title: {tc_title}
Domain: {domain}
Device: {dut_type} at {dut_address}
Preconditions: {preconditions}
Steps:
{numbered_steps_with_expected_results}

Generate a complete GFriend automation script for this test case. Output ONLY the script, no explanation.
```

---

### 6. LLM Inference

**Config:**

```env
LM_STUDIO_BASE_URL=http://localhost:1234/v1
LM_STUDIO_MODEL=qwen2.5-coder-7b-instruct
TEMPERATURE=0.1
MAX_TOKENS=2048
TOP_P=0.95
```

**File:** `agent/generator.py`

Uses `openai` Python SDK pointed at LM Studio's local OpenAI-compatible endpoint.

---

### 7. Script Validator

**File:** `agent/validator.py`

**Validation checks:**

| Check | Method | On Failure |
|-------|--------|-----------|
| `using` statement for each inferred library | Regex match | Add `// WARNING: missing using statement for {lib}` |
| Test case block `{ }` balanced | Brace counter | Flag as SYNTAX_ERROR |
| Keywords exist in knowledge base | ChromaDB lookup by keyword name | Add `// WARNING: unverified keyword {name}` |
| Variable syntax `${...}` correct | Regex | Add `// WARNING: malformed variable` |
| No `AlwaysPass` in generated code | String search | Replace + log error |
| Script non-empty | Length check | Flag as GENERATION_FAILED |

---

### 8. API Layer (FastAPI)

**File:** `api/main.py`

**Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/generate-script` | Single TC JSON → GFriend script text |
| `POST` | `/generate-from-excel` | Excel file upload → ZIP of .txt scripts |
| `GET` | `/health` | Liveness check (model + vector DB) |
| `GET` | `/keywords/search?q=...` | Search keyword knowledge base |
| `POST` | `/ingest/keywords` | Ingest new keyword definitions |
| `POST` | `/ingest/scripts` | Ingest new sample scripts |

**Run:**

```bash
uvicorn api.main:app --reload --port 8000
```

---

### Knowledge Base (ChromaDB)

**File:** `knowledge_base/` (documents) + `vector_store/` (persisted embeddings)

```text
gfriend-ai-agent/
├── knowledge_base/
│   ├── keywords/          GFK.* keyword .md definitions (one file per library)
│   ├── scripts/           Past .txt GFriend scripts (organized by domain)
│   ├── kt_docs/           GFriend architecture markdown docs
│   └── variable_files/    .gfvar files and .yaml variable patterns
├── vector_store/          ChromaDB persisted embeddings
└── scripts/
    └── ingest.py          Re-runnable ingestion script
```

---

## Data Flow Summary

```text
Excel file
  → ExcelParser           (raw rows)
  → TCNormalizer           (NormalizedTC objects)
  → DomainClassifier       (+ library list per TC)
  → SemanticRetriever      (+ context chunks from ChromaDB)
  → PromptBuilder          (structured prompt string)
  → LLM (LM Studio)        (raw script text)
  → ScriptValidator        (validated + annotated script)
  → OutputWriter           (TC-001_Title.txt + report.json)
```

---

## Infrastructure Requirements

| Component | Requirement | Notes |
|-----------|-------------|-------|
| Python | 3.11+ | Via pyenv recommended |
| RAM | 8GB minimum, 16GB recommended | 5.5GB for model + Python stack |
| GPU | Apple Silicon (Metal) preferred | Fallback: CPU inference (slower) |
| Disk | 10GB free | Model 4.68GB + vector store |
| LM Studio | Latest stable | Pre-installed (Phase 1 complete) |
| ChromaDB | Latest | Pre-installed (Phase 1 complete) |
| Network | Local only | Zero external calls |
