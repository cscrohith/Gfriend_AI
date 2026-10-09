# GFriend AI Agent — Implementation Task Breakdown

This document consolidates the 21 implementation tasks discussed for the GFriend AI Agent project, from prompt building through end-to-end integration.

## Project Overview

### Goal
Build a local RAG-based AI assistant that converts manual TestRail/Excel test cases into draft GFriend automation scripts, grounded in verified GFriend knowledge.

### High-level workflow
```text
Manual Test Case (TestRail / Excel)
    ↓
Excel Parser → Test Case Normalizer → Domain Classifier
    ↓
ChromaDB RAG Retrieval
    ↓
Prompt Builder → Local Qwen2.5-Coder-7B-Instruct via LM Studio
    ↓
GFriend Script Generator
    ↓
Variable Generator / GFVar Mapping
    ↓
Script Validator
    ↓
Output Writer + generation_report.json
    ↓
Developer Review → GFriend / STF real execution
```

### Proposed local technology stack
- **LLM:** Qwen2.5-Coder-7B-Instruct
- **LLM hosting:** LM Studio, local OpenAI-compatible endpoint `http://localhost:1234/v1`
- **Vector database:** ChromaDB
- **Embeddings:** nomic-embed-text-v1.5
- **RAG framework:** LangChain
- **API:** FastAPI + Uvicorn
- **Excel/data processing:** pandas + openpyxl
- **Configuration:** python-dotenv + Pydantic
- **Execution model:** Local Windows environment; no cloud API dependency

### Core design principles
- RAG supplies verified GFK keyword definitions, validated/sample scripts, KT/GFriend documentation, DAT information, and variable knowledge.
- Do not ask the LLM to invent GFriend keywords or library usage.
- Unknown or low-confidence steps should be flagged for review, e.g. `// UNIMPLEMENTED` or `// WARNING`.
- Validation reports problems and does not silently modify the generated script.
- Generated scripts are **drafts requiring developer review**, even if automated validation passes.
- Avoid hardcoded credentials and environment-specific values.
- Preserve generation, retrieval, variable, validation, warning, and error details in `generation_report.json`.

### Project success target
Evaluate approximately **10 real Test Cases**. Target at least **70% of generated scripts requiring only minor edits rather than complete rewrites**. Measure this using actual results; do not assume the target has been achieved.

---

# Task 1 — Prompt Builder
**Jira:** `GF-AI-101`

## Objective
Build a reusable prompt builder that combines the normalized Test Case, retrieved RAG context, verified GFK rules, similar script examples, variable rules, and expected output format.

## Inputs
- Normalized Test Case
- Classified domain/library hint
- Retrieved GFK keyword definitions
- Similar validated scripts
- KT/DAT documentation
- Variable rules and existing variable context

## Implementation
- Create a reusable prompt template.
- Include the original Test Case intent and normalized steps.
- Include relevant retrieved knowledge and examples.
- Tell the model not to invent keywords, parameters, libraries, or variable mappings.
- Tell the model to flag unmapped steps for review.
- Define the required GFriend script output structure.
- Keep prompt construction separate from API/CLI code.

## Example
```python
prompt = prompt_builder.build(
    test_case=normalized_tc,
    domain=domain,
    rag_context=rag_context
)
```

## Acceptance criteria
- Prompt includes the normalized Test Case and relevant RAG context.
- Domain-specific rules are included.
- Similar scripts are included when available.
- Unknown mappings are flagged rather than fabricated.
- Output format is explicit and consistent.
- Prompt construction is unit-tested.

**Deliverable:** Prompt builder module, templates, tests, and prompt documentation.

---

# Task 2 — LM Studio Integration
**Jira:** `GF-AI-102`

## Objective
Connect the application to the local Qwen model through LM Studio's OpenAI-compatible API.

## Implementation
- Configure endpoint, model, temperature, token limit, and timeout.
- Send the prompt and receive the model response.
- Handle connection errors, timeouts, empty responses, and invalid response structures.
- Keep model access separate from generation orchestration.
- Do not require a cloud LLM service.

## Example configuration
```text
LM_STUDIO_URL=http://localhost:1234/v1
LM_STUDIO_MODEL=qwen2.5-coder-7b-instruct
```

## Acceptance criteria
- A prompt can be sent to the configured local model.
- Response text is returned in a predictable structure.
- Connection/timeout/empty-response failures are controlled and logged.
- Configuration is externalized rather than hardcoded.

**Deliverable:** LM Studio client/service, configuration, tests, and setup instructions.

---

# Task 3 — GFriend Script Generator
**Jira:** `GF-AI-103`

## Objective
Generate a draft GFriend script from a normalized Test Case and retrieved knowledge.

## Implementation
- Accept normalized Test Case, domain, RAG context, and prompt builder output.
- Call the LM Studio integration.
- Extract the script from the model response.
- Preserve GFriend structure, `using` statements, keywords, variables, and control flow.
- Flag unmapped steps.
- Do not invent unverified GFK keywords.

## Acceptance criteria
- Generator returns script content and generation metadata.
- Output contains the script rather than explanatory prose.
- Unknown steps are explicitly marked for review.
- Generation errors are propagated in a controlled way.
- Output remains a draft.

**Deliverable:** Generator module and unit tests using representative Test Cases.

---

# Task 4 — Generator Error Handling
**Jira:** `GF-AI-104`

## Objective
Provide controlled error handling for failures in generation.

## Scenarios
- LM Studio unavailable
- Model timeout
- Empty or malformed LLM response
- RAG context unavailable or insufficient
- Unmapped/low-confidence steps
- Unexpected generator exception

## Implementation
- Define structured error codes/types.
- Distinguish temporary dependency failures from invalid input and logical issues.
- Log enough context to troubleshoot without exposing secrets.
- Return a controlled failure result rather than an unhandled exception.
- Do not retry logical errors such as invalid input or validation findings indiscriminately.

## Acceptance criteria
- Each known failure has a clear error code/message.
- Pipeline callers receive predictable error structures.
- Secrets are not logged.
- Failure stage is identifiable.

**Deliverable:** Error types, handling logic, tests, and error-code documentation.

---

# Task 5 — Variable Generator
**Jira:** `GF-AI-105`

## Objective
Identify variables required by the generated script and compare them with known variable definitions.

## Implementation
- Extract `${Variable}` references.
- Identify environment-specific values and credential requirements.
- Compare referenced variables with known/shared variables.
- Report required, existing, missing, duplicate, and suspicious hardcoded values.
- Never insert credentials or invent values.

## Example output
```json
{
  "required": ["Username", "Password", "DUT_Address"],
  "existing": ["Username", "DUT_Address"],
  "missing": ["Password"]
}
```

## Acceptance criteria
- Variable references are extracted correctly.
- Missing variables are reported.
- Existing definitions are reused where verified.
- Hardcoded secrets/environment values are flagged.
- Results are structured for GFVar mapping and reporting.

**Deliverable:** Variable analysis module and tests.

---

# Task 6 — GFVar Mapping
**Jira:** `GF-AI-106`

## Objective
Map variables referenced by generated scripts to existing GFVar/shared variable definitions.

## Implementation
- Reuse known variable names and definitions where evidence supports the mapping.
- Detect missing mappings and duplicate/conflicting definitions.
- Preserve variable names and naming conventions.
- Do not invent aliases or values.
- Return mapped and unmapped variable details.

## Acceptance criteria
- Existing matching variables are reused.
- Missing variables are clearly identified.
- Duplicate/conflicting definitions are reported.
- No secret values are fabricated or exposed.

**Deliverable:** GFVar mapping service, mapping result model, and tests.

---

# Task 7 — Output Writer
**Jira:** `GF-AI-107`

## Objective
Write generated artifacts to a consistent output structure.

## Expected artifacts
- Generated `.cs` script
- `.gfvar` file when applicable
- `generation_report.json`

## Implementation
- Use safe, predictable output filenames.
- Create output directories when needed.
- Define overwrite behavior.
- Handle partial write failures and report them.
- Preserve draft/review status.
- Avoid unsafe paths and path traversal.

## Example output
```text
output/
└── TC-1234_Login/
    ├── TC-1234_Login.cs
    ├── TC-1234_Login.gfvar
    └── generation_report.json
```

## Acceptance criteria
- Files are written to the configured output directory.
- Names are sanitized.
- Missing directories are created.
- Partial failures are reported.
- `.gfvar` is written only when applicable.

**Deliverable:** Output writer, tests, and output-format documentation.

---

# Task 8 — Generation Report
**Jira:** `GF-AI-108`

## Objective
Create `generation_report.json` describing the generation run end-to-end.

## Suggested fields
- Test Case ID/title/domain
- Pipeline and generation status
- Model and duration
- RAG retrieval counts and available scores
- Variable/mapping status
- Validation status, errors, and warnings
- Unmapped steps
- Output file information
- Manual-review requirement

## Example
```json
{
  "tc_id": "TC-1234",
  "title": "Login",
  "domain": "Web",
  "generation": {
    "status": "SUCCESS",
    "model": "qwen2.5-coder-7b-instruct",
    "duration_seconds": 14.2
  },
  "variables": {
    "required": 4,
    "mapped": 3,
    "missing": 1
  },
  "validation": {
    "status": "WARNING",
    "errors": 0,
    "warnings": 2
  },
  "manual_review_required": true
}
```

## Acceptance criteria
- Report is valid JSON.
- Report includes generation, retrieval, variables, validation, and output metadata.
- Warnings/errors are captured.
- Secrets are not included.
- Report is created for successful runs and appropriately handled failures.

**Deliverable:** Report builder/schema, tests, and example report.

---

# Task 9 — Excel Template Generator
**Jira:** `GF-AI-109`

## Objective
Provide a standard Excel template compatible with the parser and normalizer.

## Suggested columns
- Test Case ID
- Title
- Domain
- Preconditions
- Step number
- Action
- Expected result
- GFriend library hint
- Test data
- Comments

## Implementation
- Generate an `.xlsx` template with headers and instructions.
- Include a sample row or sample sheet.
- Apply basic input validation where useful.
- Keep template column names aligned with the parser's expected schema.

## Acceptance criteria
- Template can be generated.
- Headers match parser expectations.
- Instructions and sample data are included.
- Parser can read a completed template.

**Deliverable:** Template generator, sample template, tests, and usage instructions.

---

# Task 10 — Script Validator Framework
**Jira:** `GF-AI-110`

## Objective
Create a central framework that executes validators and aggregates findings.

## Implementation
- Define a common validator interface.
- Define a common result model with validator, severity, code, message, and optional line number.
- Run independent validators.
- Aggregate errors and warnings.
- Calculate overall `PASS`, `WARNING`, or `FAILED`.
- Support enabling/disabling validators through configuration if required.
- Fail safely and log validator execution failures.

## Suggested status logic
- Any error → `FAILED`
- No errors but one or more warnings → `WARNING`
- No errors or warnings → `PASS`

## Acceptance criteria
- Validators use a common interface/result structure.
- All configured validators run.
- Multiple findings are aggregated.
- Overall status is calculated consistently.
- Framework tests cover success and failure.

**Deliverable:** Validator framework, result models, and tests.

---

# Task 11 — GFK Keyword Validation
**Jira:** `GF-AI-111`

## Objective
Verify that generated keywords and their library/parameter usage are supported by the GFK knowledge base.

## Checks
- Keyword exists in verified GFK definitions.
- Correct library and `using` context.
- Alias consistency.
- Parameter count/structure matches the known definition.
- Keyword is appropriate for the identified context where that information exists.

## Rules
- Do not invent a missing keyword.
- Do not silently replace invalid keywords.
- Report the source/evidence where available.

## Acceptance criteria
- Known valid keywords pass.
- Unknown keywords are reported as errors.
- Incorrect library/parameter usage is reported.
- Findings include line numbers where available.

**Deliverable:** GFK keyword validator and tests.

---

# Task 12 — Syntax Validation
**Jira:** `GF-AI-112`

## Objective
Detect malformed or incomplete generated scripts.

## Checks
- Required Test Case structure
- Balanced braces where applicable
- `using` statement syntax
- Required statement structure/termination according to GFriend conventions
- Empty or corrupted output
- Unexpected LLM explanatory text outside the script
- Basic structural integrity

## Acceptance criteria
- Valid fixtures pass.
- Known malformed fixtures fail.
- Findings have useful messages and line numbers where available.
- Syntax validation does not duplicate keyword, variable, or safety checks unnecessarily.

**Deliverable:** Syntax validator and test fixtures.

---

# Task 13 — Variable Validation
**Jira:** `GF-AI-113`

## Objective
Validate variable syntax and mapping consistency.

## Checks
- `${Variable}` syntax
- Undefined variables
- GFVar mapping
- Duplicate/conflicting variables
- Naming conventions
- Unused variables where detectable
- Hardcoded environment values
- Hardcoded credentials
- Consistency between script references and mapping output

## Acceptance criteria
- Valid mapped variables pass.
- Undefined variables are reported.
- Invalid syntax and mapping conflicts are reported.
- Sensitive values are not included unnecessarily in findings.

**Deliverable:** Variable validator and tests.

---

# Task 14 — Safety / AlwaysPass Validation
**Jira:** `GF-AI-114`

## Objective
Detect risky or inappropriate script patterns before developer review.

## Checks
- Hardcoded credentials/secrets/tokens
- Environment-specific values that should be parameterized
- `AlwaysPass`
- Pass/fail manipulation or result overrides
- Unrequested destructive actions
- Unsafe commands or unrequested operations where detectable

## Rules
- Compare against the original Test Case where context is available.
- Do not automatically reject destructive actions explicitly required by the Test Case.
- Do not expose secret values in reports.
- Report findings; do not modify the script.

## Acceptance criteria
- Safety fixtures trigger expected findings.
- Legitimate actions required by the Test Case are not automatically rejected.
- `AlwaysPass` and pass/fail overrides are detected.
- Secrets are masked/not echoed.
- Validator does not alter script content.

**Deliverable:** Safety validator, test fixtures, and rule documentation.

---

# Task 15 — Validation Test Suite
**Jira:** `GF-AI-115`

## Objective
Build automated tests for the validator framework and each validator, including integration tests.

## Test areas
- Framework result aggregation
- GFK keyword existence/library/parameters
- Syntax and structure
- Variables and GFVar mapping
- Safety and `AlwaysPass`
- Unmapped steps
- Multiple simultaneous findings
- Valid scripts and invalid scripts

## Suggested test layout
```text
tests/
└── validator/
    ├── test_validator_framework.py
    ├── test_keyword_validator.py
    ├── test_syntax_validator.py
    ├── test_variable_validator.py
    ├── test_safety_validator.py
    ├── test_validator_integration.py
    └── fixtures/
        ├── valid/
        └── invalid/
```

## Acceptance criteria
- Positive and negative scenarios are covered.
- `PASS`, `WARNING`, and `FAILED` behavior is verified.
- Multiple findings are aggregated.
- Unmapped steps and safety cases are covered.
- Tests run with one command, e.g. `pytest tests/validator/`.
- Test results are documented.
- Regression tests protect existing behavior.

**Deliverable:** Automated validator test suite, fixtures, and execution report.

---

# Task 16 — CLI Generator
**Jira:** `GF-AI-116`

## Objective
Provide a command-line interface for running the generation pipeline without directly calling internal modules.

## Suggested commands
```bash
python -m gfriend_ai --help
python -m gfriend_ai generate --input TestCases.xlsx
python -m gfriend_ai generate --input TestCases.xlsx --tc-id TC-1234
python -m gfriend_ai generate --input TestCases.xlsx --output ./generated
python -m gfriend_ai validate --input generated/TC-1234.cs
```

## Suggested options
- `--input`
- `--tc-id`
- `--output`
- `--domain`
- `--verbose`
- `--no-report`
- `--dry-run`

## Implementation
- Keep CLI code thin; call reusable services/pipeline.
- Validate input path, extension, and requested Test Case ID.
- Display pipeline progress, warnings, errors, output paths, and review status.
- Define stable exit codes for success, invalid input, configuration/dependency errors, generation errors, and output failures.
- Do not duplicate generation logic in CLI code.

## Acceptance criteria
- Help works.
- Generate command invokes the pipeline.
- TC selection and output directory work.
- Verbose and dry-run behavior work as specified.
- Errors are clear and exit codes are tested.
- Report path and validation status are shown.

**Deliverable:** CLI entry point, tests, and `docs/CLI_USAGE.md`.

---

# Task 17 — Pipeline Orchestration
**Jira:** `GF-AI-117`

## Objective
Connect all components into one reusable end-to-end pipeline used by both CLI and API.

## Pipeline stages
1. Parse input
2. Normalize Test Case
3. Classify domain
4. Retrieve RAG context
5. Build prompt
6. Generate script through LM Studio
7. Analyze variables
8. Map GFVar variables
9. Validate script
10. Write output
11. Create generation report

## Implementation
- Define input/output contracts for stages.
- Use a common `PipelineResult` model.
- Record current stage and stage status.
- Handle failures in a controlled way.
- Preserve useful draft output if generation succeeded but validation failed.
- Keep secrets out of logs/reports.
- Reuse components rather than duplicating logic.

## Suggested result fields
```python
class PipelineResult:
    tc_id: str
    status: str
    raw_test_case: dict
    normalized_test_case: dict
    domain: dict
    rag_context: dict
    generated_script: str | None
    variables: dict
    validation: dict
    output: dict
    warnings: list
    errors: list
    manual_review_required: bool
```

## Acceptance criteria
- One call executes the full pipeline.
- Stage order and contracts are defined.
- Failure stage is identifiable.
- Validation results are included.
- Output/report generation is integrated.
- CLI can call the pipeline.
- Pipeline integration tests pass.

**Deliverable:** Orchestrator, shared models, exception definitions, tests, and `docs/PIPELINE.md`.

---

# Task 18 — FastAPI Generate Endpoint
**Jira:** `GF-AI-118`

## Objective
Expose script generation through FastAPI.

## Endpoint
`POST /api/v1/generate`

## Request
Accept an Excel file upload and a Test Case ID, with optional domain/output/dry-run options according to the API contract.

Example:
```bash
curl -X POST "http://localhost:8000/api/v1/generate" \
  -F "file=@TestCases.xlsx" \
  -F "tc_id=TC-1234"
```

## Response should include
- Overall/generation status
- Test Case ID/title
- Domain and confidence/source where available
- Generation/model/duration metadata
- RAG retrieval counts
- Script content
- Variable/GFVar mapping information
- Validation status and findings
- Warnings/errors
- Output/report information
- Manual-review status

## Implementation
- Call GF-AI-117 pipeline; do not duplicate its stages.
- Use Pydantic request/response models.
- Validate extension, file size, Test Case ID, and request parameters.
- Use temporary storage safely and clean up input files.
- Return structured errors for invalid input, missing Test Case, unavailable RAG/LLM, generation failure, and output failure.
- Do not expose secrets or unnecessary filesystem paths.

## Suggested HTTP behavior
- `200`: request processed; generation may still require review
- `400`: invalid request/file
- `404`: Test Case not found
- `503`: required dependency unavailable
- `500`: unexpected internal failure

Choose and document one consistent convention for validation failures.

## Acceptance criteria
- Endpoint accepts supported Excel input.
- Specific TC selection works.
- Existing pipeline is invoked.
- Script and validation results are returned.
- Errors are structured.
- Uploads are handled safely.
- Swagger docs and API tests exist.

**Deliverable:** FastAPI route, Pydantic models, error handling, tests, and `docs/API.md` update.

---

# Task 19 — Keyword Search Endpoint
**Jira:** `GF-AI-119`

## Objective
Expose verified GFK keyword lookup and semantic search through FastAPI.

## Endpoint
`GET /api/v1/keywords/search`

## Suggested parameters
- `q`: query string
- `domain`: optional domain filter
- `library`: optional library filter
- `top_k`: number of results, default 5 and maximum 20

## Examples
```bash
curl "http://localhost:8000/api/v1/keywords/search?q=Web.Click"
curl "http://localhost:8000/api/v1/keywords/search?q=click%20button&domain=Web&top_k=5"
```

## Results
Return verified KB fields where available:
- Keyword
- Library
- Description
- Parameters
- Examples
- Source
- Retrieval score/distance with correct terminology

## Rules
- Use ChromaDB/RAG knowledge.
- Do not ask the LLM to invent keyword definitions.
- Empty query should be rejected.
- No results should return an empty list.
- KB unavailability should be a controlled `503`, not an empty search result.

## Acceptance criteria
- Exact and semantic search work.
- Domain/library filters work.
- `top_k` is bounded.
- Result schema is documented.
- Search works without LM Studio if KB is available.
- API tests cover normal, empty, no-result, and dependency-failure cases.

**Deliverable:** Keyword route/service/models, tests, and API documentation.

---

# Task 20 — Validate Endpoint
**Jira:** `GF-AI-120`

## Objective
Expose the existing script validator through FastAPI. This endpoint analyzes a submitted GFriend `.cs` script and returns findings without modifying the script.

## Endpoint
`POST /api/v1/validate`

## Example
```bash
curl -X POST "http://localhost:8000/api/v1/validate" \
  -F "file=@TC-1234_Login.cs"
```

## Validation areas
- GFK keyword validation
- Syntax validation
- Variable/GFVar validation
- Safety/`AlwaysPass` validation
- Additional configured validators

## Response
Each finding should include, where available:
- Validator
- Severity (`INFO`, `WARNING`, `ERROR`)
- Line number
- Stable finding code
- Safe message

Overall status:
- Any error → `FAILED`
- No errors but warnings → `WARNING`
- No findings → `PASS`

## Implementation
- Reuse GF-AI-110 through GF-AI-114; do not duplicate validation rules in the route.
- Validate file extension and size.
- Aggregate all findings.
- Do not return secret values in messages.
- Do not modify the submitted script.
- Return a controlled error if required KB access is unavailable.

## Acceptance criteria
- Endpoint accepts `.cs` scripts.
- All configured validators run.
- Findings are structured and aggregated.
- Overall status is correct.
- Sensitive data is protected.
- Swagger and API tests are available.

**Deliverable:** Validate route, validation service/model, tests, and API documentation.

---

# Task 21 — End-to-End Integration
**Jira:** `GF-AI-121`

## Objective
Verify the complete system with real Test Cases and through both CLI and FastAPI.

## End-to-end flow
```text
Excel Test Case
    ↓
Parser → Normalizer → Classifier
    ↓
ChromaDB RAG
    ↓
Prompt Builder → Qwen/LM Studio
    ↓
Script Generator
    ↓
Variable Generator → GFVar Mapping
    ↓
Validator
    ↓
Output Writer + generation_report.json
```

## Validation scope
- Full pipeline with representative real Test Cases
- CLI generation
- FastAPI generation
- Keyword Search API
- Validate API
- RAG grounding
- Variable mapping
- Output artifacts and report
- Failure paths for parser, RAG, LM Studio, validation, and output
- Safe handling of credentials and logs

## Ten-Test-Case evaluation
Use approximately 10 representative real Test Cases. Record actual outcomes rather than assuming success.

For each Test Case, capture:
- TC ID and domain
- Generation result
- Validation result
- Warnings/errors
- Unmapped steps
- Manual-review status
- Developer review classification

## Developer review classification
- **Minor edit:** small selector, variable, timeout, or syntax adjustments
- **Major rewrite:** incorrect workflow, wrong library, multiple incorrect keywords, or large sections rewritten
- **Generation failure:** parser, RAG, LLM, or other failure prevents a usable script

## Success metric
Target: at least **70% of generated scripts require only minor edits rather than complete rewrites** across the 10-Test-Case evaluation. Calculate the rate from actual results and document the denominator used.

## Acceptance criteria
- Full pipeline executes on real Test Cases.
- CLI and API invoke the same pipeline.
- Keyword and Validate APIs work.
- RAG grounding is checked.
- Unknown keywords and unmapped steps are surfaced.
- Variable/GFVar mapping and validation are checked.
- Outputs and reports are verified.
- Failure scenarios are controlled.
- Ten-Test-Case results and minor-edit rate are documented.

## Suggested test layout
```text
tests/
└── e2e/
    ├── test_generation_e2e.py
    ├── test_cli_e2e.py
    ├── test_generate_api_e2e.py
    ├── test_keyword_api_e2e.py
    ├── test_validate_api_e2e.py
    └── fixtures/
```

**Deliverable:** End-to-end tests, 10-TC evaluation report, issue/blocker list, and documented results.

---

# Consolidated Jira List

| Jira | Task |
|---|---|
| GF-AI-101 | Prompt Builder |
| GF-AI-102 | LM Studio Integration |
| GF-AI-103 | GFriend Script Generator |
| GF-AI-104 | Generator Error Handling |
| GF-AI-105 | Variable Generator |
| GF-AI-106 | GFVar Mapping |
| GF-AI-107 | Output Writer |
| GF-AI-108 | Generation Report |
| GF-AI-109 | Excel Template Generator |
| GF-AI-110 | Script Validator Framework |
| GF-AI-111 | GFK Keyword Validation |
| GF-AI-112 | Syntax Validation |
| GF-AI-113 | Variable Validation |
| GF-AI-114 | Safety / AlwaysPass Validation |
| GF-AI-115 | Validation Test Suite |
| GF-AI-116 | CLI Generator |
| GF-AI-117 | Pipeline Orchestration |
| GF-AI-118 | FastAPI Generate Endpoint |
| GF-AI-119 | Keyword Search Endpoint |
| GF-AI-120 | Validate Endpoint |
| GF-AI-121 | End-to-End Integration |

---

# Overall Definition of Done

The implementation group is complete when:
- All 21 task deliverables are implemented and tested against their acceptance criteria.
- The local RAG/LLM pipeline produces draft GFriend scripts from Excel Test Cases.
- Generation is grounded in verified GFK knowledge.
- Variables and GFVar mappings are reported.
- Validators detect keyword, syntax, variable, and safety issues.
- CLI and FastAPI reuse the same orchestration layer.
- Keyword Search and Validate endpoints work.
- Outputs and `generation_report.json` are produced.
- Secrets are not exposed in reports, API responses, or logs.
- End-to-end tests and the 10-Test-Case evaluation are documented.
- Generated scripts remain drafts until developer review and real GFriend/STF execution approval.
