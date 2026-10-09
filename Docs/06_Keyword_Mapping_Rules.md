# Keyword Mapping Rules

## Purpose

This document defines how natural-language test step actions (from manual TCs) map to specific GFriend `GFK.*` keyword calls. These rules are used to:

1. Guide the AI's prompt with correct keyword suggestions
2. Validate that the AI has correctly mapped actions to keywords
3. Provide a fallback rule-based mapping when the AI is uncertain

---

## How Mapping Works

Each test step action goes through three mapping attempts in order:

1. **Step-level `library_hint`**: If the TC step has an explicit `GFriend_Library_Hint` column value, use that library's keywords directly
2. **Semantic retrieval**: ChromaDB returns the most similar keyword definitions for this step
3. **Rule-based patterns** (this document): Pattern matching against common action verbs → direct keyword suggestions

The final prompt presents all three to the LLM as candidate keywords. The LLM selects the most appropriate one.

---

## Pattern → Keyword Mapping Tables

### Web Actions (GFK.Web)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Open browser / Launch Chrome / Start Firefox | `Web.Open With Chrome(url)` or `Web.Open With Firefox(url)` | URL from TC or variable |
| Navigate to / Go to URL / Open URL | `Web.Navigate(url)` | Use when browser already open |
| Click button/link/element | `Web.Click(selector)` | selector = CSS or XPath |
| Enter / Type / Input text into field | `Web.Set Text(selector, value)` | |
| Get / Read / Capture text from | `Web.Get Text(selector, ${Buffer})` | |
| Verify / Check / Assert text visible | `Web.Wait For Text(text, timeout)` or `Web.Check Text Exists(text)` | `Wait For Text` for dynamic content, `Check Text Exists` for static |
| Wait for page / Wait for text | `Web.Wait For Text(text, timeout)` | Default timeout 10s |
| Select dropdown option | `Web.Select Option(selector, value)` | |
| Check / tick checkbox | `Web.Check Checkbox(selector)` | |
| Scroll to element | `Web.Scroll To(selector)` | |
| Take screenshot / Capture | `Web.Capture Screen Shot(name)` | name = descriptive string |
| Logout / Sign out | `Web.Click(logoutSelector)` then `Web.Wait For Text(Login)` | Combine two keywords |
| Close browser | `Web.Close()` | |
| Refresh page | `Web.Refresh()` | |
| Go back / Navigate back | `Web.Navigate Back()` | |
| Verify page title | `Web.Check Text Exists(pageTitle)` | |
| Upload file | `Web.Set Text(fileInputSelector, filePath)` | For `<input type="file">` |

---

### Android Mobile Actions (GFK.Android)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Tap / Click / Touch text | `Android.Touch Text(text)` | Finds by visible text |
| Tap / Click by ID / resource | `Android.Touch Id(resourceId)` | Use resource-id |
| Enter / Type / Input text | `Android.Set Text(resourceId, value)` | |
| Get / Read text | `Android.Get Text(resourceId, ${Buffer})` | |
| Verify text on screen | `Android.Check Screen Contains Full Text(text)` | |
| Verify text NOT on screen | `Android.Check Screen Not Contains Full Text(text)` | |
| Wait for text to appear | `Android.Wait For Text(text, timeout)` | |
| Press back / Navigate back | `Android.Go Back()` | |
| Swipe | `Android.Swipe(direction)` | direction: up/down/left/right |
| Scroll down | `Android.Scroll Down()` | |
| Take screenshot | `Android.Capture Screen Shot(name)` | |
| Install / Launch app | `Android.Launch App(packageName)` | |

---

### Printer Panel / OCP Actions (GFK.JediOmni)

**Framework Note:** GFriend printer/device libraries (`JediOmni`, `OXPD`, `Hallasan`, `Dune`, `Sirius`) are built on top of the Device Automation Toolkit (DAT), which provides the foundational device control and communication layer. See `../manual/DAT/` for DAT architecture details.

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Tap / Touch / Press text on panel | `JediOmni.Touch Text(text)` | |
| Tap / Touch element by ID | `JediOmni.Touch Id(elementId)` | |
| Enter / Type into field | `JediOmni.Set Text(elementId, value)` | |
| Read / Get text from panel | `JediOmni.Get Text(elementId, ${Buffer})` | |
| Wait for text on panel | `JediOmni.Wait For Text(text, timeout)` | |
| Verify / Check text on panel | `JediOmni.Check Text Exists(text)` | |
| Sign in / Login to device | `JediOmni.Sign In(username, password)` | |
| Navigate to home screen | `JediOmni.Navigate To Home()` | |
| Take screenshot of panel | `JediOmni.Capture Screen Shot(name)` | |
| Wait for printer to be ready | `JediOmni.Wait For Text(Ready, 60)` | 60s timeout for printer |

---

### Print Driver / Print Job Actions (GFK.Hallasan)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Open file for printing | `Hallasan.Open File For Print(resourceAlias, driverName)` | Resource must be declared |
| Set print setting / Configure print | `Hallasan.Set Print Settings(setting, value)` | |
| Send to printer / Print | `Hallasan.Print()` | |
| Cancel print / Close print dialog | `Hallasan.Cancel Print()` | |

---

### OXPD / Workpath Actions (GFK.OXPD)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Navigate to OXPD / Open OXPD | `OXPD.Navigate(url)` | |
| Click OXPD element | `OXPD.Click(selector)` | |
| Enter text in OXPD | `OXPD.Set Text(selector, value)` | |
| Wait for OXPD content | `OXPD.Wait For Text(text, timeout)` | |
| Verify OXPD content | `OXPD.Check Text Exists(text)` | |
| Get OXPD text | `OXPD.Get Text(selector, ${Buffer})` | |

---

### REST API Actions (GFK.REST)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Send GET request / Call API | `REST.Get(url, ${Buffer})` | |
| Send POST request | `REST.Post(url, requestBody, ${Buffer})` | body = JSON string or variable |
| Send PUT request | `REST.Put(url, requestBody)` | |
| Delete / Send DELETE request | `REST.Delete(url)` | |
| Verify response code / HTTP status | `REST.Check Response Code(200)` | After GET/POST/PUT/DELETE |
| Get / Read response body | `REST.Get Response Body(${Buffer})` | |
| Add header / Set auth header | `REST.Set Header(Authorization, Bearer ${Token})` | |

---

### SSH / Shell Actions (GFK.SSH)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Connect via SSH | `SSH.Connect(host, username, password)` | |
| Run command / Execute | `SSH.Execute(command, ${Buffer})` | |
| Verify output / Check result | `SSH.Check Output Contains(expectedText)` | |
| Disconnect / Close SSH | `SSH.Disconnect()` | |

---

### iOS Actions (GFK.IOS)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Tap / Touch text | `IOS.Touch Text(text)` | |
| Tap by accessibility ID | `IOS.Touch Id(accessibilityId)` | |
| Enter text | `IOS.Set Text(accessibilityId, value)` | |
| Navigate back | `IOS.Go Back()` | |
| Take screenshot | `IOS.Capture Screen Shot(name)` | |

---

### Windows Desktop Actions (GFK.Windows)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Click control / button | `Windows.Click(automationId)` | Use UI Automation ID |
| Enter / Type text | `Windows.Set Text(automationId, value)` | |
| Read / Get control text | `Windows.Get Text(automationId, ${Buffer})` | |
| Wait for window | `Windows.Wait For Window(windowTitle, timeout)` | |
| Verify control exists | `Windows.Check Control Exists(automationId)` | |

---

### Vision / Image Recognition (GFK.Vision)

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Find image on screen | `Vision.Find Image(imageFilePath)` | |
| Click on image | `Vision.Click Image(imageFilePath)` | |
| Read text via OCR | `Vision.Read Text(region, ${Buffer})` | |
| Verify image present | `Vision.Check Image Exists(imageFilePath)` | |

---

### Universal / Built-in Actions

| Step Action Pattern | Suggested GFriend Keyword | Notes |
|--------------------|--------------------------|-------|
| Wait / Pause / Sleep | `Sleep(seconds)` | Convert to seconds |
| Verify previous step passed | `If: ${KEYWORD_RESULT} == Pass` block | Use system variable |
| Pass test / Mark as pass | `Pass(reason)` | |
| Fail test / Mark as fail | `Fail(reason)` | |
| Repeat N times | `Repeat:N { ... }` | |
| Retry until condition | `While:Library.CheckKeyword():N { ... }` | |
| Conditional action (if X then Y) | `If: Library.CheckKeyword() { ... }` | |

---

## Compound Action Patterns

These test step patterns require **multiple keywords** combined:

| Step Pattern | Generated GFriend Pattern |
|-------------|--------------------------|
| "Login to web application" | `Web.Set Text(#username, ${Username})` + `Web.Set Text(#password, ${Password})` + `Web.Click(#login-btn)` + `Web.Wait For Text(Dashboard)` |
| "Login to printer panel" | `JediOmni.Sign In(${AdminID}, ${AdminPW})` + `JediOmni.Wait For Text(Home)` |
| "Print document from mobile" | `Android.Touch Text(Print)` + `Android.Wait For Text(Print dialog)` + `JediOmni.Wait For Text(Job received, 60)` |
| "Verify API endpoint works" | `REST.Set Header(Authorization, Bearer ${Token})` + `REST.Get(${BaseUrl}/api/resource, ${Buffer})` + `REST.Check Response Code(200)` |
| "Wait and retry" | `While:Library.CheckCondition():N { Sleep(2) }` |

---

## Action Verb → Intent Mapping

Use this table to interpret ambiguous action verbs:

| Verb | Intent | Primary Keyword Pattern |
|------|--------|------------------------|
| Open, Launch, Start | Open app/browser/session | `Web.Open With Chrome`, `Android.Launch App`, `SSH.Connect` |
| Navigate, Go to, Browse | Navigate to location | `Web.Navigate`, `JediOmni.Navigate To Home` |
| Click, Tap, Touch, Press, Select | Interact with element | `Web.Click`, `Android.Touch Text`, `JediOmni.Touch Id` |
| Enter, Type, Input, Fill | Enter text | `Web.Set Text`, `Android.Set Text`, `JediOmni.Set Text` |
| Verify, Check, Assert, Confirm, Ensure | Assertion | `Web.Check Text Exists`, `Android.Check Screen Contains Full Text` |
| Wait for, Expect | Synchronization | `Web.Wait For Text`, `JediOmni.Wait For Text`, `Sleep` |
| Get, Read, Capture, Retrieve | Data capture | `Web.Get Text`, `Android.Get Text`, `REST.Get Response Body` |
| Send, Submit, Post | Send data | `Web.Click(submit)`, `REST.Post` |
| Close, Exit, Quit | End session | `Web.Close`, `SSH.Disconnect` |
| Scroll | Navigate within page | `Web.Scroll To`, `Android.Scroll Down` |
| Screenshot, Capture screen | Evidence collection | `Web.Capture Screen Shot`, `Android.Capture Screen Shot` |

---

## Precondition → Script Preamble Mapping

Preconditions from the TC typically map to variable declarations or setup keywords:

| Precondition Statement | Script Mapping |
|-----------------------|---------------|
| "Printer IP: 192.168.1.50" | `${DUT_Address}=192.168.1.50` |
| "Admin credentials available" | `${Username}=admin` + `${Password}=Admin@123` |
| "Browser installed" | No code needed — informational |
| "App server running at http://..." | `${BaseUrl}=http://server.example.com` |
| "Test file at C:\files\doc.docx" | `Resource C:\files\doc.docx As TestDoc` |
| "Android device ID: Pixel7" | `using Android With Pixel7` |

---

## Domain + Step → Library Confidence Score

The agent uses this matrix to resolve ambiguity when a step could map to multiple libraries:

| Step Keyword | Domain=Web | Domain=Android | Domain=Printer_Panel | Domain=Multi_Domain |
|-------------|-----------|---------------|---------------------|---------------------|
| "click" | Web ✓✓✓ | Android ✓✓ | JediOmni ✓✓ | Web (default) |
| "print" | Hallasan ✓✓ | Android ✓ | JediOmni ✓✓✓ | Hallasan ✓✓ |
| "login" | Web ✓✓✓ | — | JediOmni ✓✓✓ | Based on context |
| "send request" | REST ✓✓✓ | — | — | REST ✓✓✓ |
| "run command" | — | — | — | SSH ✓✓✓ |
| "tap" | — | Android ✓✓✓ | JediOmni ✓✓ | Android |
| "wait for" | Web ✓✓ | Android ✓✓ | JediOmni ✓✓ | Match domain |
