# GFriend Scripting Reference for AI

## Purpose

This document defines the complete GFriend scripting syntax and conventions that the AI agent must produce in every generated script. It is the **ground truth** that validators and prompt instructions are derived from.

---

## 1. File Types

| Extension | Purpose | Example |
|-----------|---------|---------|
| `.txt` | Test script / test suite (main input to GF_Runner) | `LoginTest.txt` |
| `.gflib` | Custom keyword library (reusable across scripts) | `PrintWorkflow.gflib` |
| `.gfvar` | Variable file (shared variables) | `DeviceConfig.gfvar` |

---

## 2. Script Structure

A valid GFriend script has this structure (order matters):

```javascript
// 1. Variable declarations (optional — or reference a .gfvar file)
${Variable_Name}=Variable_Value
${DUT_Address}=192.168.1.50

// 2. Variable file references (optional)
vars/DeviceConfig.gfvar

// 3. Resource declarations (optional)
Resource C:\TestFiles\Document.docx As PrintDoc

// 4. Dataset declaration (optional — for data-driven tests)
Dataset path/to/DataFile.xlsx

// 5. Using statements (required for each library used)
using Web
using JediOmni As PrinterPanel With Ruby
using Android As MobileDevice With Note9

// 6. Custom library references (optional)
using MyCustomLib.gflib As CustomLib

// 7. Test cases (one or more)
Test Case Name
{
    Library.Keyword(parameter1, parameter2)
    Sleep(2)
}
```

---

## 3. Variables

### 3a. Static Variables (pre-execution substitution)

```javascript
${Username}=admin
${Password}=Admin@123
${PrinterIP}=192.168.1.100
```

- Declared at top of script (before test cases)
- Replaced textually before execution (like `#define` in C)
- Variable names: `[A-Za-z0-9_-]`

### 3b. Dynamic Variables (set during execution)

```javascript
Web.Get Text(#result-text, ${Buffer})      // captures to ${Buffer}
Android.Get Text(textBoxId, ${CapturedValue})
JediOmni.Get Text(elementId, ${CapturedText})
```

### 3c. System Variables (pre-defined — never redefine these)

```javascript
${DUT_Address}        // Default device IP/hostname
${DUT_AdminID}        // Device admin username
${DUT_AdminPW}        // Device admin password
${DUT_ID}             // Device identifier
${DUT_TYPE}           // Device type string
${R}                  // Current repeat count (inside Repeat block)
${SCRIPT_FOLDER}      // Folder containing the .txt script
${OUTPUT_FOLDER}      // Folder for output files
${KEYWORD_RESULT}     // Result of last keyword (Pass/Fail/Error)
${KEYWORD_OUTPUT}     // Text output of last keyword
${EMPTY}              // Empty string
${CARRIAGE_RETURN}    // \r
${LINE_FEED}          // \n
```

---

## 4. Using Statement — Complete Syntax

```javascript
using <LibraryName> [As <Alias>] [With <DeviceID>] [At <RemoteExecutorID>]
```

**Examples:**
```javascript
using Web                                          // Basic
using Web As Chrome                                // Alias
using Android As Phone With Pixel7                 // Alias + device
using JediOmni As Printer With Ruby                // Alias + device
using Android As MobileA With Device1              // Multiple instances
using Android As MobileB With Device2
using JediOmni With Ruby At RemoteAgent01          // Remote execution
using MyLib.gflib As CustomKeywords                // Custom library file
```

**Generated script rule:** Every `GFK.*` library used in the test case body MUST have a corresponding `using` statement. The alias (if used) must match exactly in keyword calls.

---

## 5. Keyword Call Syntax

```javascript
LibraryName.KeywordName(param1, param2, ...)
```

- `LibraryName` = library name OR alias from `using As`
- `KeywordName` = exact keyword as defined in the `GFK.*` library
- Parameters: comma-separated, no quotes unless value contains special characters
- Empty parameter: use `${EMPTY}`

**Examples:**
```javascript
Web.Open With Chrome(https://app.example.com)
Web.Click(#login-button)
Web.Set Text(#username, admin)
Web.Wait For Text(Dashboard, 10)
Android.Touch Text(Print)
Android.Set Text(usernameField, ${Username})
JediOmni.Touch Id(homeButton)
JediOmni.Wait For Text(Ready, 30)
REST.Get(${BaseUrl}/api/users, ${Buffer})
SSH.Execute(${ServerIP}, ls -la /var/log)
Sleep(5)
```

**Built-in keywords (no library prefix needed):**
```javascript
Sleep(seconds)
Pass(optional_message)
Fail(optional_message)
Error(optional_message)
```

---

## 6. Flow Control

### 6a. Repeat

```javascript
Repeat:N
{
    // Repeated N times
    // Use ${R} for current iteration count
}

Repeat:5s     // Repeat for 5 seconds
Repeat:10m    // Repeat for 10 minutes
Repeat:72h    // Repeat for 72 hours
```

### 6b. While

```javascript
While:Library.SomeCheckKeyword(params):MaxLoops
{
    // Executes while condition PASSES
}

While:!Library.SomeCheckKeyword(params):MaxLoops
{
    // Executes while condition FAILS (note ! prefix)
}

// Default MaxLoops = 50 if omitted
While:Web.Wait For Text(Loading)
{
    Sleep(2)
}
```

### 6c. If / Fail / Error

```javascript
If: Library.CheckKeyword(params)
{
    // Runs if keyword returns Pass
}
Fail:
{
    // Runs if keyword returns Fail (can be empty)
}
Error:
{
    // Runs if keyword returns Error (can be empty)
}
```

- `Fail:` and `Error:` blocks are optional
- Blocks can be empty `{}`

---

## 7. Custom Library (.gflib) Structure

```javascript
// CustomLib.gflib
using JediOmni
using Web

// Keyword definition: KeywordName(${param1}, ${param2})
Login(${UserId}, ${Password})
{
    JediOmni.Touch Text(${UserId})
    JediOmni.Set Text(passwordField, ${Password})
    JediOmni.Touch Id(loginButton)
    JediOmni.Wait For Text(Dashboard, 10)
}

PrintDocument(${FileName})
{
    Web.Click(#print-button)
    Web.Wait For Text(Print dialog, 5)
}
```

---

## 8. Dataset / Data-Driven Tests

```javascript
Dataset path/to/TestData.xlsx

Data Driven Test Case
{
    // Column headers from Excel become ${ColumnName} variables
    Web.Set Text(#username, ${Username})
    Web.Set Text(#password, ${Password})
    Web.Click(#login)
    Web.Wait For Text(${ExpectedPage})
}
```

---

## 9. Resource Files

```javascript
Resource C:\TestFiles\Document.docx As PrintDoc
Resource ../resources/Image.png As TestImage

Print Test Case
{
    Hallasan.Open File For Print(PrintDoc, ${PrinterDriver})
}
```

---

## 10. Comments

```javascript
// Single-line comment
```

- Double-slash only
- No block comments
- Do NOT add comments in `.gfvar` files on the same line as a variable declaration

---

## 11. Return Statements

```javascript
Pass()                          // Force pass result
Pass(Verification successful)   // Pass with message
Fail()                          // Force fail
Fail(Element not found)         // Fail with message
Error()                         // Force error
Error(Unexpected state)         // Error with message
```

---

## 12. GFK.* Library Quick Reference

### GFK.Web

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `Web.Open With Chrome` | `url` | Open Chrome browser |
| `Web.Open With Firefox` | `url` | Open Firefox browser |
| `Web.Open With Edge` | `url` | Open Edge browser |
| `Web.Click` | `selector` | Click element by CSS/XPath |
| `Web.Set Text` | `selector, text` | Type text into input |
| `Web.Get Text` | `selector, ${var}` | Get text from element |
| `Web.Wait For Text` | `text [, timeout]` | Wait until text visible |
| `Web.Check Text Exists` | `text` | Assert text visible (Pass/Fail) |
| `Web.Navigate` | `url` | Navigate to URL |
| `Web.Navigate Back` | | Browser back |
| `Web.Refresh` | | Refresh page |
| `Web.Capture Screen Shot` | `name` | Take screenshot |
| `Web.Close` | | Close browser |
| `Web.Select Option` | `selector, value` | Select dropdown option |
| `Web.Check Checkbox` | `selector` | Check a checkbox |
| `Web.Scroll To` | `selector` | Scroll element into view |

### GFK.Android

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `Android.Touch Text` | `text` | Tap by visible text |
| `Android.Touch Id` | `resourceId` | Tap by resource ID |
| `Android.Set Text` | `resourceId, text` | Type into field |
| `Android.Get Text` | `resourceId, ${var}` | Get field text |
| `Android.Check Screen Contains Full Text` | `text` | Check text on screen |
| `Android.Check Screen Not Contains Full Text` | `text` | Check text NOT on screen |
| `Android.Go Back` | | Press back button |
| `Android.Capture Screen Shot` | `name` | Screenshot |
| `Android.Swipe` | `direction` | Swipe screen |
| `Android.Scroll Down` | | Scroll down |
| `Android.Wait For Text` | `text [, timeout]` | Wait for text |

### GFK.JediOmni (Printer Panel)

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `JediOmni.Touch Text` | `text` | Tap by visible text |
| `JediOmni.Touch Id` | `elementId` | Tap by element ID |
| `JediOmni.Set Text` | `elementId, text` | Type into field |
| `JediOmni.Get Text` | `elementId, ${var}` | Get element text |
| `JediOmni.Wait For Text` | `text [, timeout]` | Wait until text visible |
| `JediOmni.Check Text Exists` | `text` | Assert text visible |
| `JediOmni.Capture Screen Shot` | `name` | Screenshot |
| `JediOmni.Navigate To Home` | | Go to home screen |
| `JediOmni.Sign In` | `username, password` | Sign in to device |

### GFK.OXPD

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `OXPD.Navigate` | `url` | Navigate OXPD web interface |
| `OXPD.Click` | `selector` | Click UI element |
| `OXPD.Set Text` | `selector, text` | Set text field |
| `OXPD.Wait For Text` | `text [, timeout]` | Wait for content |
| `OXPD.Check Text Exists` | `text` | Verify text present |
| `OXPD.Get Text` | `selector, ${var}` | Capture text value |

### GFK.Hallasan (Print Driver)

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `Hallasan.Open File For Print` | `resourceAlias, driverName` | Open file in print dialog |
| `Hallasan.Set Print Settings` | `setting, value` | Configure print settings |
| `Hallasan.Print` | | Send print job |
| `Hallasan.Cancel Print` | | Cancel print dialog |

### GFK.REST

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `REST.Get` | `url [, ${responseVar}]` | HTTP GET request |
| `REST.Post` | `url, body [, ${responseVar}]` | HTTP POST request |
| `REST.Put` | `url, body` | HTTP PUT request |
| `REST.Delete` | `url` | HTTP DELETE request |
| `REST.Check Response Code` | `expectedCode` | Assert HTTP status code |
| `REST.Get Response Body` | `${var}` | Capture response body |
| `REST.Set Header` | `key, value` | Add request header |

### GFK.SSH

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `SSH.Connect` | `host, username, password` | Open SSH connection |
| `SSH.Execute` | `command [, ${outputVar}]` | Run shell command |
| `SSH.Check Output Contains` | `text` | Assert command output |
| `SSH.Disconnect` | | Close SSH session |

### GFK.Vision

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `Vision.Find Image` | `imageFile` | Locate image on screen |
| `Vision.Click Image` | `imageFile` | Click on image match |
| `Vision.Read Text` | `region, ${var}` | OCR text from screen area |
| `Vision.Check Image Exists` | `imageFile` | Assert image visible |

### GFK.IOS

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `IOS.Touch Text` | `text` | Tap by visible text |
| `IOS.Touch Id` | `accessibilityId` | Tap by accessibility ID |
| `IOS.Set Text` | `accessibilityId, text` | Type into field |
| `IOS.Go Back` | | Navigate back |
| `IOS.Capture Screen Shot` | `name` | Screenshot |

### GFK.Windows (Desktop UI)

| Keyword | Parameters | Description |
|---------|-----------|-------------|
| `Windows.Click` | `automationId` | Click control by automation ID |
| `Windows.Set Text` | `automationId, text` | Type into control |
| `Windows.Get Text` | `automationId, ${var}` | Get control text |
| `Windows.Wait For Window` | `title [, timeout]` | Wait for window to appear |
| `Windows.Check Control Exists` | `automationId` | Assert control present |

---

## 13. Critical Rules — AI Must Never Violate

1. **Never use a keyword not in the knowledge base** — if unsure, add `// TODO: verify keyword`
2. **Never set `AlwaysPass`** — this bypasses result validation
3. **Never auto-commit** — scripts are drafts requiring human review
4. **Every library used needs a `using` statement** — placed before test cases
5. **Variable syntax is `${VarName}`** — no other formats
6. **Comments are `//` only** — no `/* */`, no `#`
7. **Test case name must not contain `{` or `}`** — these delimit the block
8. **Parameter values with spaces**: wrap in the manner expected by the keyword (check keyword definition)
9. **Sleep is in seconds**: `Sleep(5)` = 5 seconds, not milliseconds
10. **Keyword calls use dots**: `Library.Keyword(params)` — never `Library::Keyword` or `Library->Keyword`
