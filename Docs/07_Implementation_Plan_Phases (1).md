# Implementation Plan — Phase-by-Phase Build

## Overview

Phase 1 (local environment setup) is complete. This document covers Phases 2–4 with detailed tasks, owners, acceptance criteria, and exit conditions for each phase.

---

## Phase 1 — Environment Setup (COMPLETE)

**Outcome:** Local AI stack running and verified.

| Component | Status |
|-----------|--------|
| Python 3.11.9 via pyenv | Done |
| Node.js 20.20.2 | Done |
| ChromaDB installed + verified | Done |
| LangChain + LangChain-OpenAI | Done |
| FastAPI + Uvicorn | Done |
| LM Studio + Qwen2.5-Coder-7B-Instruct | Done |
| Embedding model (nomic-embed-text-v1.5) | Done |
| `.env` configuration | Done |

---

## Phase 2 — Knowledge Base + RAG Pipeline

**Goal:** Agent can retrieve relevant GFriend keywords and scripts given a TC description.

**Duration:** ~3 weeks (2 hrs/day)

**Exit criteria:**
- ChromaDB populated with all 26 GFK.* library keyword definitions
- ChromaDB populated with ≥ 20 validated GFriend scripts
- Semantic search returns relevant keywords for a given step description (manual validation)
- FastAPI `/keywords/search` endpoint returns correct results

---

### Phase 2 Tasks

#### 2.1 — Generate Keyword Documentation Files (Week 1)

**Task:** Create `knowledge_base/keywords/GFK.{Library}.md` for all 26 libraries

**How:**
1. Use `ManualGenerator/GenerateManual.py` to generate HTML from existing C# source
2. Convert HTML to markdown format matching the schema in [Doc 04](04_Knowledge_Base_Ingestion_Plan.md)
3. Alternatively: write a Python script that parses C# `GFLibraries/GFK.*/` source directly using regex on method signatures, XML doc comments, and `[KeywordAttribute]` attributes

**File:** `scripts/extract_keywords_from_csharp.py`
```bash
python scripts/extract_keywords_from_csharp.py \
    --source ../../GFLibraries/ \
    --output knowledge_base/keywords/
```

**Priority order for keyword extraction (highest to lowest):**
1. GFK.JediOmni (printer panel — most-used in SCL)
2. GFK.Web (web testing — broadly applicable)
3. GFK.Android (mobile — broadly applicable)
4. GFK.OXPD (SCL-specific)
5. GFK.Hallasan (print driver)
6. GFK.REST (API testing)
7. GFK.SSH (server testing)
8. GFK.Windows, GFK.IOS, GFK.Mac, GFK.Vision, GFK.SpreadSheet (remainder)

**Acceptance:** All 26 `GFK.*.md` files exist with ≥ 5 keyword definitions each

---

#### 2.2 — Collect Sample Scripts (Week 1)

**Task:** Gather existing validated GFriend scripts to seed `knowledge_base/scripts/`

**Sources:**
- `GFriend_AI_Agent/Samples/` directory — all sample `.txt` scripts
- SCL program scripts already written and committed to GitHub
- Any script from team members (minimum: cover each domain)

**Organize into:**
```
knowledge_base/scripts/
├── web/
├── android/
├── printer_panel/
├── printer_oxpd/
├── rest_api/
├── ssh/
└── multi_domain/
```

**Acceptance:** ≥ 10 scripts total, at least 3 domains covered

---

#### 2.3 — Prepare KT Documentation (Week 1)

**Task:** Copy and convert GFriend README files into `knowledge_base/kt_docs/`

**Files to convert (copy from repo):**
```
GFCore/README.md         → kt_docs/gfcore_scripting_guide.md
Samples/FlowControl/README.md → kt_docs/gfcore_flow_control.md
Samples/Variables/README.md   → kt_docs/gfcore_variables.md
Samples/DataDrivenTest/README.md → kt_docs/gfcore_data_driven.md
GFCore/Custom/README.md  → kt_docs/gfcore_custom_libraries.md
```

Also create: `kt_docs/gfriend_conventions.md` — a distilled "10 rules" document the agent always receives

**Acceptance:** ≥ 5 KT doc files created

---

#### 2.4 — Write Ingestion Script (Week 1)

**File:** `scripts/ingest.py` (see full implementation in [Doc 04](04_Knowledge_Base_Ingestion_Plan.md))

**Test ingestion:**
```bash
python scripts/ingest.py
# Verify:
# Keywords ingested: X
# Scripts ingested: X
# KT docs ingested: X
# Variable files ingested: X
```

**Acceptance:** Ingestion completes without errors, counts > 0 for all collections

---

#### 2.5 — Build and Test Retrieval Pipeline (Week 2)

**File:** `agent/retriever.py`

**Test script:**
```python
# Test: given "login to printer panel with admin credentials", retrieve relevant keywords
from agent.retriever import retrieve_context

results = retrieve_context(
    tc_description="Login to HP printer panel using admin ID and password",
    libraries=["JediOmni"]
)
print(results.keyword_definitions[:3])  # Should show JediOmni.Sign In, JediOmni.Touch Text, etc.
```

**Acceptance:**
- Top-3 results for web TC queries contain `Web.*` keywords
- Top-3 results for printer queries contain `JediOmni.*` keywords
- Retrieval completes in < 500ms

---

#### 2.6 — FastAPI Keyword Search Endpoint (Week 2)

**File:** `api/main.py`

**Endpoint:** `GET /keywords/search?q=login+to+printer&library=JediOmni&limit=5`

**Test:**
```bash
curl "http://localhost:8000/keywords/search?q=login+to+printer&library=JediOmni"
```

**Acceptance:** Returns JSON list of matching keyword definitions

---

#### 2.7 — Phase 2 Integration Test (Week 3)

**Task:** End-to-end retrieval test using a real manual TC

**Test TC:** "Verify user can login to HP printer panel with valid admin credentials"

**Expected retrieval:** `JediOmni.Sign In`, `JediOmni.Wait For Text`, `JediOmni.Navigate To Home`, `JediOmni.Capture Screen Shot`

**Acceptance criteria:**
- [ ] ChromaDB has > 100 keyword chunks
- [ ] ChromaDB has > 20 script chunks
- [ ] Retrieval returns domain-appropriate results
- [ ] FastAPI server starts and `/health` returns OK
- [ ] `/keywords/search` returns relevant results

---

## Phase 3 — Generation Pipeline + Excel Input

**Goal:** Agent accepts Excel file with TCs, generates GFriend scripts, saves output files.

**Duration:** ~4 weeks (2 hrs/day)

**Exit criteria:**
- `generate_scripts.py` CLI accepts Excel file and produces `.txt` scripts
- FastAPI `/generate-from-excel` endpoint works end-to-end
- ≥ 70% of generated scripts pass developer review with minor edits only

---

### Phase 3 Tasks

#### 3.1 — Excel Parser (Week 1)

**File:** `agent/excel_parser.py`

Implement auto-detection of TestRail vs Custom Template format (see [Doc 05](05_Agent_Workflow_Spec.md) Step 2 for logic).

**Test:**
```bash
python -c "
from agent.excel_parser import parse_excel
rows = parse_excel('test_data/sample_testrail_export.xlsx')
print(f'Parsed {len(rows)} rows')
"
```

**Acceptance:** Parses both TestRail export and Custom Template without errors

---

#### 3.2 — TC Normalizer (Week 1)

**File:** `agent/tc_normalizer.py`

Implement `NormalizedTC` dataclass and grouping logic (see [Doc 05](05_Agent_Workflow_Spec.md) Step 4).

**Acceptance:** Given 10 raw rows for 3 TCs, produces 3 `NormalizedTC` objects with correct step order

---

#### 3.3 — Domain Classifier (Week 1)

**File:** `agent/domain_classifier.py`

Implement keyword-matching + semantic fallback classification (see [Doc 05](05_Agent_Workflow_Spec.md) Step 5 and [Doc 06](06_Keyword_Mapping_Rules.md)).

**Acceptance:** Correct domain classification for 10 test TCs spanning all domains

---

#### 3.4 — Prompt Builder (Week 2)

**File:** `agent/prompt_builder.py`

Implement structured prompt construction with token budget management (see [Doc 05](05_Agent_Workflow_Spec.md) Step 7).

**Test:** Print a generated prompt for a sample TC, verify it contains all required sections and is < 3500 tokens.

---

#### 3.5 — Generator (Week 2)

**File:** `agent/generator.py`

Implement LLM call and error handling (see [Doc 05](05_Agent_Workflow_Spec.md) Step 8).

**First test:** Single TC → print raw LLM output to console.

**Acceptance:** Produces non-empty GFriend-looking output for a simple web TC

---

#### 3.6 — Script Validator (Week 2)

**File:** `agent/validator.py`

Implement all validation checks and annotation logic (see [Doc 05](05_Agent_Workflow_Spec.md) Step 9).

---

#### 3.7 — Output Writer + Report (Week 2)

**File:** `agent/output_writer.py`

Implement file writer, report generator (see [Doc 05](05_Agent_Workflow_Spec.md) Steps 10–11).

---

#### 3.8 — CLI Entry Point (Week 3)

**File:** `agent/generate_scripts.py`

```bash
python agent/generate_scripts.py --input testcases.xlsx --output ./scripts/
```

Wires all components together: parse → normalize → classify → retrieve → build → generate → validate → write → report.

---

#### 3.9 — FastAPI Endpoint (Week 3)

**File:** `api/main.py`

Implement `POST /generate-from-excel` endpoint using the same pipeline as the CLI.

---

#### 3.10 — Excel Template Generator Tool (Week 3)

**File:** `tools/generate_tc_template.py`

Python script that generates `GFriend_TC_Template.xlsx` with correct headers, dropdowns, formatting, and example rows. See [Doc 01](01_Excel_TC_Template_Spec.md) for column spec.

---

#### 3.11 — Phase 3 Validation (Week 4)

**Task:** Generate scripts for 10 real manual TCs. Have developers review using [Doc 09](09_Script_Review_Checklist.md).

**Target:** ≥ 7/10 scripts (70%) require only minor keyword parameter edits, not rewrites.

**Acceptance criteria:**
- [ ] CLI processes full Excel file successfully
- [ ] Output `.txt` files are syntactically valid GFriend scripts
- [ ] `generation_report.json` produced
- [ ] 70% script usability rate achieved
- [ ] FastAPI `/generate-from-excel` endpoint tested end-to-end

---

## Phase 4 — VS Code Extension

**Goal:** Developer can right-click a test case row in VS Code → "Generate GFriend Script" → script opens in editor.

**Duration:** ~3 weeks (2 hrs/day)

**Exit criteria:**
- VS Code extension installable from local `.vsix` package
- Workflow: open Excel → right-click TC row → script generated and opened in new tab
- Extension shows generation progress and validation results

---

### Phase 4 Tasks

#### 4.1 — VS Code Extension Scaffold (Week 1)

```bash
npm install -g yo generator-code
yo code  # Select: New Extension (TypeScript)
```

**Extension name:** `gfriend-ai-agent`

---

#### 4.2 — Excel File Context Menu Command (Week 1)

**Command:** `GFriend AI Agent: Generate Script for This Test Case`

**Triggered by:** Right-click in editor when a `.xlsx` file is open + cursor is on a TC row

**Action:** Calls `http://localhost:8000/generate-from-excel` with current file path and detected TC ID

---

#### 4.3 — Script Preview in New Tab (Week 2)

When generation completes, opens a new `.txt` editor tab with the generated script pre-loaded.

---

#### 4.4 — Validation Warnings in Problems Panel (Week 2)

Validation warnings from the agent (`// WARNING: ...` lines) surfaced in VS Code Problems panel as information/warning diagnostics.

---

#### 4.5 — Package and Distribute (Week 3)

```bash
vsce package  # Creates gfriend-ai-agent-1.0.0.vsix
# Install: code --install-extension gfriend-ai-agent-1.0.0.vsix
```

---

## Risk Register

| Risk | Phase | Probability | Impact | Mitigation |
|------|-------|------------|--------|-----------|
| Keyword docs incomplete or inconsistent | 2 | High | High | Extract from C# source programmatically, validate against real scripts |
| LLM hallucinates non-existent keywords | 3 | Medium | Medium | RAG grounding + validator catches unknown keywords |
| 70% accuracy target not met in Phase 3 | 3 | Medium | High | Start with simpler TCs (Web/Android), defer complex CardReader to Phase 3.5 |
| LM Studio not reliable on Windows machines | 3 | Low | Medium | Test on Windows; fallback: Claude API with same prompt structure |
| Phase 4 VS Code API changes | 4 | Low | Low | Use stable extension API; test against current VS Code version |

---

## Milestone Summary

| Milestone | Phase | Target Date | Deliverable |
|-----------|-------|------------|-------------|
| M1: Knowledge base populated | 2 | Week 3 from start | ChromaDB with 100+ keyword chunks, 20+ scripts |
| M2: Retrieval validated | 2 | Week 3 | Retrieval accuracy demo |
| M3: First script generated | 3 | Week 6 | Single TC → script via CLI |
| M4: Excel pipeline working | 3 | Week 9 | Full Excel → scripts batch run |
| M5: 70% accuracy validated | 3 | Week 10 | Developer review of 10 TCs |
| M6: VS Code extension | 4 | Week 13 | .vsix installable extension |
