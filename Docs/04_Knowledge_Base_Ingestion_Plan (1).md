# Knowledge Base Ingestion Plan

## Purpose

This document defines what content to ingest into the ChromaDB vector store, how to structure it, and how to run ingestion — both initially and on updates.

---

## ChromaDB Collections

The vector store contains five collections:

| Collection | Contents | Primary Use |
|-----------|---------|------------|
| `gfk_keywords` | GFK.* keyword definitions + parameters | Retrieved at script generation time |
| `gf_scripts` | Past validated GFriend automation scripts | Provides code patterns and examples |
| `kt_docs` | Architecture docs, framework guides, conventions | Retrieved for context and rules |
| `gf_variables` | Common `.gfvar` variable files and patterns | Retrieved for variable declarations |
| `dat_framework` | Device Automation Toolkit (DAT) architecture, device-specific control patterns | Retrieved when generating printer/device automation scripts to understand underlying device control layer |

---

## Collection 1: `gfk_keywords`

### Source Files

```text
knowledge_base/keywords/
├── GFK.Web.md
├── GFK.Android.md
├── GFK.Android2.md
├── GFK.IOS.md
├── GFK.JediOmni.md
├── GFK.OXPD.md
├── GFK.Hallasan.md
├── GFK.Windows.md
├── GFK.REST.md
├── GFK.SSH.md
├── GFK.Telnet.md
├── GFK.Vision.md
├── GFK.SpreadSheet.md
├── GFK.MSOffice.md
├── GFK.Mac.md
├── GFK.Json.md
├── GFK.Regex.md
├── GFK.XML.md
├── GFK.Info.md
├── GFK.LogInfo.md
├── GFK.Lp.md
├── GFK.Sirius.md
├── GFK.Fleet.md
├── GFK.BadgeBox.md
├── GFK.Preparation.md
└── GFK.Dune.md
```

### Keyword Definition Format (one file per library)

Each keyword should be documented in this format for ingestion:

```markdown
## Web.Open With Chrome

**Library:** GFK.Web  
**Domain:** Web  
**Syntax:** `Web.Open With Chrome(url)`  
**Parameters:**
- `url` (string) — Full URL to navigate to (e.g., https://app.example.com)

**Returns:** Pass on success, Fail if browser fails to open  
**Description:** Opens Google Chrome browser and navigates to the specified URL.  
**Example:**
```javascript
Web.Open With Chrome(https://portal.hp.com)
```
**Use when:** Test requires opening a web browser. First keyword in any web test.
```

### How to Generate These Files

Run the existing `ManualGenerator/GenerateManual.py` to extract keyword docs from the GFriend C# source, then convert to this markdown format.

Alternatively, extract directly from GFLibraries source:
```bash
python scripts/extract_keywords_from_csharp.py \
    --source ../../GFLibraries/ \
    --output knowledge_base/keywords/
```

### Metadata Schema (per keyword chunk)

```python
{
    "id": "gfk_web_open_with_chrome",
    "document": "Web.Open With Chrome\nLibrary: GFK.Web\nDomain: Web\nSyntax: Web.Open With Chrome(url)\n...",
    "metadata": {
        "library": "GFK.Web",
        "keyword_name": "Web.Open With Chrome",
        "domain": "Web",
        "has_output_var": False,
        "parameter_count": 1
    }
}
```

---

## Collection 2: `gf_scripts`

### Source Files

```text
knowledge_base/scripts/
├── web/
│   ├── login_test.txt
│   ├── form_submission_test.txt
│   └── ...
├── android/
│   ├── mobile_print_test.txt
│   └── ...
├── printer_panel/
│   ├── oxpd_functional_test.txt
│   ├── cardreader_test.txt
│   └── ...
├── rest_api/
│   ├── rest_login_test.txt
│   └── ...
└── multi_domain/
    ├── mobile_print_endtoend.txt
    └── ...
```

### What to Include

- All existing validated GFriend scripts from the SCL program
- Sample scripts from `Samples/` directory in the GFriend repo
- Any scripts that have been reviewed and committed to GitHub

### Script Chunking Strategy

Each script is ingested as **one document per test case block** (not whole file), preserving the using statements as context:

```python
def chunk_script(script_text: str) -> list[dict]:
    """
    Returns one chunk per test case, with using/variable preamble prepended.
    """
    preamble, test_cases = parse_gfriend_script(script_text)
    return [
        {
            "document": f"{preamble}\n\n{tc_name}\n{{\n{tc_body}\n}}",
            "metadata": {
                "domain": infer_domain(preamble),
                "libraries": extract_libraries(preamble),
                "test_case_name": tc_name,
                "source_file": filename
            }
        }
        for tc_name, tc_body in test_cases
    ]
```

### Metadata Schema

```python
{
    "id": "script_web_login_001",
    "document": "using Web\n\nLogin Test\n{\n    Web.Open With Chrome(...)\n}",
    "metadata": {
        "domain": "Web",
        "libraries": ["Web"],
        "test_case_name": "Login Test",
        "source_file": "login_test.txt",
        "validated": True
    }
}
```

---

## Collection 3: `kt_docs`

### Source Files

```text
knowledge_base/kt_docs/
├── gfcore_scripting_guide.md     (from GFCore/README.md)
├── gfcore_flow_control.md        (from Samples/FlowControl/README.md)
├── gfcore_variables.md           (from Samples/Variables/README.md)
├── gfcore_data_driven.md         (from Samples/DataDrivenTest/README.md)
├── gfcore_custom_libraries.md    (from GFCore/Custom/README.md)
├── gfcore_return_statements.md   (from Samples/ReturnStatements/README.md)
├── gfremote_execution.md         (from GFRemote/README.md)
├── gfkeywords_interface.md       (from GFKeywords/README.md)
└── gfriend_conventions.md        (distilled from all READMEs — framework rules)
```

### Chunking Strategy

Split by `##` section headings. Each chunk = one section (approximately 300–700 tokens).

### Metadata Schema

```python
{
    "id": "kt_gfcore_variables_static",
    "document": "## Static Variables\nGFriend treats text with format ${variable_name}...",
    "metadata": {
        "source": "gfcore_scripting_guide.md",
        "section": "Static Variables",
        "doc_type": "kt_doc"
    }
}
```

---

## Collection 4: `dat_framework`

### Source Files

```text
knowledge_base/dat_framework/
├── 01_Executive_Summary.md           (from ../manual/DAT/)
├── 02_Solution_MindMap.md
├── 03_End_to_End_Flow.md
├── 04_DAT_Framework_Deep_Dive.md
├── 05_Framework_Relationship_View.md
├── 06_Manager_Onboarding_View.md
├── 07_Change_Impact_View.md
└── 08_Assumptions_Questions.md
```

### Purpose

The **Device Automation Toolkit (DAT)** is the foundational C# library that GFriend uses for printer device automation. DAT provides:

- Device factory pattern for auto-discovering printer firmware types (Jedi/Oz/Phoenix/Sirius/Dune)
- Control panel automation APIs (button presses, element inspection, JavaScript execution)
- Device management (power, SNMP, network configuration, diagnostics)
- Protocol abstraction (HTTP/HTTPS, SNMP, WebSockets, WCF)

**Why this matters for the AI Agent:**
When generating GFriend scripts that use `JediOmni`, `OXPD`, `Hallasan`, or other printer-related libraries, the agent should understand:

- The underlying DAT architecture these libraries are built upon
- Device-specific communication patterns
- Common device control workflows (device discovery, panel interaction, print job management)
- Troubleshooting patterns when device communication fails

### Chunking Strategy

Split by `##` section headings. Each chunk = one section with context about DAT's role in the automation stack.

### Metadata Schema

```python
{
    "id": "dat_executive_summary_business_purpose",
    "document": "## Business Purpose\nThe Device Automation Toolkit (DAT) is a foundational C# library...",
    "metadata": {
        "source": "01_Executive_Summary.md",
        "section": "Business Purpose",
        "doc_type": "dat_framework",
        "relevance": "printer_automation"
    }
}
```

---

## Collection 5: `gf_variables`

### Source Files

```text
knowledge_base/variable_files/
├── device_config.gfvar       Common device address/credential variables
├── print_driver.gfvar        Printer driver names and settings
├── web_endpoints.gfvar       Base URLs and app endpoints
├── user_credentials.gfvar    Test user accounts (no real passwords)
├── android_devices.gfvar     Android device IDs
└── common_timeouts.gfvar     Standard wait timeout values
```

### Format

Each `.gfvar` file is ingested whole (they are small). Metadata includes the context in which these variables are typically used.

---

## Ingestion Script

**File:** `scripts/ingest.py`

```python
#!/usr/bin/env python3
"""
GFriend AI Agent — Knowledge Base Ingestion Script
Re-runnable: safe to run after adding new keywords or scripts.
"""

import chromadb
from pathlib import Path
from sentence_transformers import SentenceTransformer

CHROMA_PATH = "./vector_store"
KNOWLEDGE_BASE = "./knowledge_base"

client = chromadb.PersistentClient(path=CHROMA_PATH)
embedder = SentenceTransformer("all-MiniLM-L6-v2")

def ingest_keywords():
    collection = client.get_or_create_collection("gfk_keywords")
    for file in Path(f"{KNOWLEDGE_BASE}/keywords").glob("*.md"):
        chunks = chunk_by_keyword(file.read_text())
        for chunk in chunks:
            collection.upsert(
                ids=[chunk["id"]],
                documents=[chunk["document"]],
                metadatas=[chunk["metadata"]]
            )
    print(f"Keywords ingested: {collection.count()}")

def ingest_scripts():
    collection = client.get_or_create_collection("gf_scripts")
    for file in Path(f"{KNOWLEDGE_BASE}/scripts").rglob("*.txt"):
        chunks = chunk_script(file.read_text(), file.name)
        for chunk in chunks:
            collection.upsert(
                ids=[chunk["id"]],
                documents=[chunk["document"]],
                metadatas=[chunk["metadata"]]
            )
    print(f"Scripts ingested: {collection.count()}")

def ingest_kt_docs():
    collection = client.get_or_create_collection("kt_docs")
    for file in Path(f"{KNOWLEDGE_BASE}/kt_docs").glob("*.md"):
        chunks = chunk_by_heading(file.read_text(), file.stem)
        for chunk in chunks:
            collection.upsert(
                ids=[chunk["id"]],
                documents=[chunk["document"]],
                metadatas=[chunk["metadata"]]
            )
    print(f"KT docs ingested: {collection.count()}")

def ingest_dat_framework():
    collection = client.get_or_create_collection("dat_framework")
    for file in Path(f"{KNOWLEDGE_BASE}/dat_framework").glob("*.md"):
        chunks = chunk_by_heading(file.read_text(), file.stem)
        for chunk in chunks:
            collection.upsert(
                ids=[chunk["id"]],
                documents=[chunk["document"]],
                metadatas=[chunk["metadata"]]
            )
    print(f"DAT framework docs ingested: {collection.count()}")

def ingest_variable_files():
    collection = client.get_or_create_collection("gf_variables")
    for file in Path(f"{KNOWLEDGE_BASE}/variable_files").glob("*.gfvar"):
        collection.upsert(
            ids=[file.stem],
            documents=[file.read_text()],
            metadatas=[{"source": file.name, "doc_type": "variable_file"}]
        )
    print(f"Variable files ingested: {collection.count()}")

if __name__ == "__main__":
    ingest_keywords()
    ingest_scripts()
    ingest_kt_docs()
    ingest_dat_framework()
    ingest_variable_files()
    print("Ingestion complete.")
```

**Run:**

```bash
cd gfriend-ai-agent
python scripts/ingest.py
```

---

## Ingestion Checklist

Before running ingestion for the first time:

- [ ] All GFK.* keyword documentation files created in `knowledge_base/keywords/`
- [ ] At least 10 validated GFriend scripts copied to `knowledge_base/scripts/`
- [ ] `GFCore/README.md` converted and copied to `knowledge_base/kt_docs/`
- [ ] `Samples/*/README.md` files copied to `knowledge_base/kt_docs/`
- [ ] DAT framework documentation copied from `../manual/DAT/` to `knowledge_base/dat_framework/`
- [ ] Variable files created/copied to `knowledge_base/variable_files/`
- [ ] LM Studio running with embedding model loaded
- [ ] Python venv activated

---

## Re-ingestion Policy

| Trigger | Action |
|---------|--------|
| New GFK.* library added | Re-run `ingest_keywords()` |
| New validated scripts available | Re-run `ingest_scripts()` |
| GFCore README updated | Re-run `ingest_kt_docs()` |
| DAT framework documentation updated | Re-run `ingest_dat_framework()` |
| New variable file added | Re-run `ingest_variable_files()` |
| Full refresh | Re-run `python scripts/ingest.py` |

ChromaDB `upsert` is idempotent — re-running replaces existing entries by ID without duplicating.

---

## Minimum Viable Knowledge Base (for Phase 2 target)

| Collection | Minimum items | Target items |
|-----------|--------------|-------------|
| `gfk_keywords` | 50 keywords (top 5 libraries) | All 200+ keywords across 26 libraries |
| `gf_scripts` | 10 test cases | 89 SCL scripts + 30 Samples |
| `dat_framework` | 8 documents | Complete DAT architecture documentation |
| `kt_docs` | 5 sections | All GFCore/Samples README sections |
| `gf_variables` | 2 files | All project variable files |
