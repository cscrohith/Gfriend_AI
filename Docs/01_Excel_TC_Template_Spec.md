# Excel TC Template Specification

## Purpose

This document defines the two accepted input formats for the GFriend AI Agent:

1. **TestRail Export Format** — CSV/Excel exported directly from TestRail
2. **GFriend Custom TC Template** — A purpose-built Excel template for teams writing manual TCs outside of TestRail

The agent's Excel parser handles both formats automatically by detecting the column header signature.

---

## Format 1 — TestRail Export

### How to Export from TestRail

1. Open TestRail → navigate to the Test Suite or Test Run
2. Click **Export** → select **Export to CSV** or **Export to Excel (.xlsx)**
3. Ensure the following columns are included in the export (TestRail allows column selection)

### Required TestRail Columns

| TestRail Column Name | Agent Field Mapping | Notes |
|---------------------|---------------------|-------|
| `ID` | `tc_id` | Numeric TC identifier (e.g., C1234) |
| `Title` | `tc_title` | Short name of the test case |
| `Section` | `domain_hint` | Used to infer which GFK library to use |
| `Type` | `tc_type` | Functional, Non-Functional, Regression, etc. |
| `Priority` | `priority` | Low / Medium / High / Critical |
| `Preconditions` | `preconditions` | Setup steps before test execution |
| `Steps` | `steps_raw` | **Primary field** — numbered step actions |
| `Expected Result` | `expected_results_raw` | Expected result per step (may be combined in Steps) |
| `References` | `references` | Jira ticket / requirements link |
| `Automation Type` | `automation_type` | Should be empty or "GFriend" for agent processing |

### TestRail Steps Column Format

TestRail can export steps in two sub-formats:

**Sub-format A — Separate columns (one row per TC):**
```
Steps: "1. Open browser\n2. Navigate to login page\n3. Enter credentials"
Expected Result: "1. Browser opens\n2. Login page displayed\n3. User logged in"
```

**Sub-format B — Expanded rows (one row per step):**
```
| ID    | Title       | Step # | Step Description          | Expected Result        |
|-------|-------------|--------|---------------------------|------------------------|
| C1001 | Login Test  | 1      | Open browser              | Browser opens          |
| C1001 | Login Test  | 2      | Navigate to login page    | Login page displayed   |
```

The agent handles **both sub-formats** automatically.

### Optional but Recommended TestRail Columns

| Column | Purpose |
|--------|---------|
| `Milestone` | Tracks which release this TC belongs to |
| `Custom: Device Type` | Custom field — printer model, mobile OS, etc. |
| `Custom: GFriend Library` | Custom field — explicitly tag which GFK library to use |
| `Custom: DUT Address` | Custom field — Device Under Test IP/hostname |

---

## Format 2 — GFriend Custom TC Template

### When to Use

Use this template when:
- Your team is **not using TestRail**
- You are creating TCs specifically for GFriend automation (new feature, sprint-based)
- You want finer control over GFriend-specific metadata (device type, variable hints, library override)

### Template File

> Generate the `.xlsx` template using: `python tools/generate_tc_template.py`

The template is saved as `GFriend_TC_Template.xlsx`.

### Sheet Structure

The template has **two sheets**:

#### Sheet 1: `TestCases` (main data entry sheet)

| Column | Column Header | Type | Required | Description |
|--------|--------------|------|----------|-------------|
| A | `TC_ID` | Text | Yes | Unique identifier. Use format `TC-001`, `TC-002` or match your tracking system |
| B | `TC_Title` | Text | Yes | Short descriptive title (max 100 chars) |
| C | `Domain` | Dropdown | Yes | Primary domain — see Domain Values table below |
| D | `DUT_Type` | Dropdown | Recommended | Device under test — see DUT Values below |
| E | `DUT_Address` | Text | Recommended | IP address or hostname of target device (e.g., `192.168.1.100`) |
| F | `TC_Type` | Dropdown | Yes | Functional / Non-Functional / Regression / Smoke / Sanity |
| G | `Priority` | Dropdown | Yes | Critical / High / Medium / Low |
| H | `Preconditions` | Text (multi-line) | Recommended | Setup requirements before test starts. One condition per line. |
| I | `Step_Number` | Number | Yes | Sequential step number: 1, 2, 3... |
| J | `Step_Action` | Text (multi-line) | Yes | What the tester does in this step |
| K | `Step_Expected_Result` | Text (multi-line) | Yes | What should happen after this step |
| L | `GFriend_Library_Hint` | Dropdown | Optional | Override — explicitly specify which GFK library maps to this step |
| M | `Variables_Needed` | Text | Optional | Comma-separated variable names this step needs (e.g., `${Username},${Password}`) |
| N | `Notes` | Text | Optional | Additional context, known edge cases, workarounds |

> **Row structure:** One row per test step. All steps of the same TC share the same `TC_ID` across multiple rows.

#### Example Data Rows

```
TC_ID  | TC_Title              | Domain | DUT_Type | Step# | Step_Action                          | Step_Expected_Result
TC-001 | Web Login Test        | Web    | -        | 1     | Open Chrome browser                  | Browser opens successfully
TC-001 | Web Login Test        | Web    | -        | 2     | Navigate to https://app.example.com  | Login page is displayed
TC-001 | Web Login Test        | Web    | -        | 3     | Enter username "admin" in Username   | Username field shows "admin"
TC-001 | Web Login Test        | Web    | -        | 4     | Enter password "Admin123" in Password| Password field is masked
TC-001 | Web Login Test        | Web    | -        | 5     | Click Login button                   | Dashboard page is displayed
```

#### Sheet 2: `Reference_Values` (dropdowns reference — do not delete)

Contains the valid values for dropdown columns:

**Domain Values (Column C)**

| Value | GFK Library(ies) | Use Case |
|-------|-----------------|---------|
| `Web` | GFK.Web | Browser-based application testing |
| `Android` | GFK.Android, GFK.Android2 | Android mobile device testing |
| `iOS` | GFK.IOS | iPhone/iPad testing |
| `Windows_Desktop` | GFK.Windows | Windows application UI testing |
| `Printer_Panel` | GFK.JediOmni | HP printer OCP (panel) testing |
| `Printer_OXPD` | GFK.OXPD | OXPD/Workpath server-side testing |
| `Printer_Hallasan` | GFK.Hallasan | HP printer driver / print job testing |
| `Mobile_Print` | GFK.Android + GFK.JediOmni | End-to-end mobile print workflows |
| `REST_API` | GFK.REST | REST API endpoint testing |
| `SSH` | GFK.SSH | SSH command / shell testing |
| `Telnet` | GFK.Telnet | Telnet protocol testing |
| `Vision` | GFK.Vision | Image recognition / OCR testing |
| `SpreadSheet` | GFK.SpreadSheet | Excel/spreadsheet operations |
| `MSOffice` | GFK.MSOffice | MS Office document handling |
| `Mac_Desktop` | GFK.Mac | macOS application testing |
| `CardReader` | GFK.JediOmni + GFK.BadgeBox | Card reader / badge authentication testing |
| `Fleet` | GFK.Fleet | Fleet management testing |
| `Multi_Domain` | Multiple | TC spans multiple library domains — agent will infer per step |

**DUT_Type Values (Column D)**

| Value | Description |
|-------|-------------|
| `Printer` | HP printer device |
| `Android_Phone` | Android smartphone |
| `Android_Tablet` | Android tablet |
| `iPhone` | Apple iPhone |
| `iPad` | Apple iPad |
| `Windows_PC` | Windows desktop/laptop |
| `Mac` | macOS desktop/laptop |
| `Server` | Server endpoint (REST, SSH) |
| `-` | Not applicable |

---

## Column Mapping — TestRail → Custom Template

| Custom Template Field | TestRail Column | Transformation |
|----------------------|----------------|----------------|
| `TC_ID` | `ID` | Direct (strip "C" prefix if present) |
| `TC_Title` | `Title` | Direct |
| `Domain` | `Section` | AI-inferred from section name |
| `TC_Type` | `Type` | Direct |
| `Priority` | `Priority` | Direct |
| `Preconditions` | `Preconditions` | Direct |
| `Step_Action` | `Steps` | Split by newline/numbered items |
| `Step_Expected_Result` | `Expected Result` | Split aligned with steps |
| `Notes` | `References` + `Custom fields` | Merged |

---

## Validation Rules

The agent will **reject** or **flag** an input Excel file if:

| Rule | Condition | Agent Behavior |
|------|-----------|----------------|
| Missing TC_ID | `TC_ID` column is blank for a row | Skip row, log warning |
| Missing steps | TC has no `Step_Action` rows | Skip TC, log error |
| Empty expected result | `Step_Expected_Result` is blank | Generate script with `// TODO: verify expected result` comment |
| Unknown domain | `Domain` value not in reference list | Default to multi-domain inference, log warning |
| Duplicate TC_ID | Same TC_ID with inconsistent TC_Title | Log warning, use first occurrence |

---

## Sample Filled Template Rows

```
TC_ID   TC_Title                         Domain         DUT_Type    DUT_Address    TC_Type       Priority  Preconditions
TC-001  Verify User Login via Web        Web            -           -              Functional    High      Browser installed. App server running.
TC-002  Print from Android to HP Printer Mobile_Print   Printer     192.168.1.50   Functional    High      Android device paired. Printer online.
TC-003  Validate REST API Login Endpoint REST_API       Server      192.168.1.100  Functional    Critical  Server accessible. Valid credentials available.
TC-004  Badge-in Authentication          CardReader     Printer     192.168.1.50   Functional    High      Card reader attached. Badge provisioned.
```
