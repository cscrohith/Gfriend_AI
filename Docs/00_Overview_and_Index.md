# GFriend AI Agent — Documentation Index

| | |
|---|---|
| **Program** | GFriend Print Domain (HP Inc.) |
| **Status** | Phase 2 — Documentation Complete, Development In Progress |

---

## Purpose

This documentation set defines everything required to build the **GFriend AI Agent**: a system that accepts manual test cases (from TestRail exports or a custom Excel template) and generates equivalent GFriend automation scripts ready for developer review and execution.

The agent supports **all GFriend library domains** — Web, Android, iOS, Windows, JediOmni (printer panel), OXPD, Hallasan, REST, SSH, Telnet, Vision, SpreadSheet, MSOffice, and all other `GFK.*` libraries.

---

## What This Agent Does

```text
INPUT:  Manual Test Case (Excel — TestRail export or custom template)
          |
          v
     GFriend AI Agent
          |
          v
OUTPUT: GFriend Automation Script (.txt) ready for developer review
```

**One Excel file → one or more `.txt` GFriend automation scripts**, one per test case row.

---

## Document Index

| # | Document | Purpose | Audience |
|---|----------|---------|----------|
| 01 | [Excel TC Template Specification](01_Excel_TC_Template_Spec.md) | Defines both input formats: TestRail export schema and custom template design | QA Team, Test Leads |
| 02 | [System Architecture](02_System_Architecture.md) | End-to-end system design — all layers, components, data flow | Dev Team, Architects |
| 03 | [GFriend Scripting Reference for AI](03_GFriend_Scripting_Reference_for_AI.md) | GFriend syntax, conventions, and rules the agent must produce correct code against | Dev Team, AI Engineers |
| 04 | [Knowledge Base Ingestion Plan](04_Knowledge_Base_Ingestion_Plan.md) | What to ingest into ChromaDB, how to structure it, ingestion scripts (includes DAT framework documentation) | Dev Team |
| 05 | [Agent Workflow Specification](05_Agent_Workflow_Spec.md) | Step-by-step pipeline: Excel in → validated script out | Dev Team |
| 06 | [Keyword Mapping Rules](06_Keyword_Mapping_Rules.md) | Natural-language test step patterns → correct `GFK.*` keyword mappings | Dev Team, AI Engineers |
| 07 | [Implementation Plan — Phases](07_Implementation_Plan_Phases.md) | Phase-by-phase build roadmap with tasks, owners, exit criteria | Dev Team, Project Lead |
| 08 | [Sample TC to Script Examples](08_Sample_TC_to_Script_Examples.md) | Real worked examples: manual TC input → expected GFriend script output | Dev Team, QA Team |
| 09 | [Script Review Checklist](09_Script_Review_Checklist.md) | How developers validate and sign off on AI-generated scripts | Dev Team, QA Team |

### Supporting Tools

| File | Purpose |
|------|---------|
| [`../tools/generate_tc_template.py`](../tools/generate_tc_template.py) | Python script to generate the custom TC input Excel template (`.xlsx`) |

---

## Quick Start — Using the Agent (Once Built)

1. Open TestRail → Export test cases as CSV/Excel **or** fill in the custom template (see [Doc 01](01_Excel_TC_Template_Spec.md))
2. Run the agent:

   ```bash
   python agent/generate_scripts.py --input path/to/testcases.xlsx --output path/to/scripts/
   ```
3. Agent produces one `.txt` script per test case in the output folder
4. Developer reviews each script using the [Script Review Checklist](09_Script_Review_Checklist.md)
5. After review, commit to GitHub as per the SCL Automation Evidence Plan

---

## Phase Status

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | Local environment — LM Studio, ChromaDB, Python stack | **Complete** |
| 2 | Knowledge base ingestion + RAG retrieval pipeline | **In Progress** |
| 3 | Generation pipeline + Excel input processor + FastAPI endpoint | Planned |
| 4 | VS Code extension — right-click TC → generate script | Planned |

---

## Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Domain scope | All GFriend libraries from day one | Agent is general-purpose; domain is inferred from TC content |
| Input formats | Both TestRail export AND custom template | Supports teams using TestRail and those without it |
| Output format | `.txt` GFriend script per TC | Matches existing GFriend file conventions |
| Model | Qwen2.5-Coder-7B (local) | Zero cost, zero data exposure, deterministic outputs |
| Vector DB | ChromaDB | Persisted locally, Python-native, production-grade |
| Delivery format | Markdown files in repo | Version-controlled, reviewable, no extra tooling |
