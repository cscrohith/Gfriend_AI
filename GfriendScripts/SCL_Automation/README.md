# SCL_Automation

Comprehensive test automation library for **SafeQ Cloud (SCL)** solutions using the HP GFriend test automation framework.

## Overview

This repository contains automated test libraries, scripts, and utilities for validating SafeQ Cloud functionality across multiple deployment types:

- **OXPd (Open eXtensibility Platform daemon)** - Embedded solution
- **Workpath** - Agentless cloud solution

## Repository Structure

```text
SCL_Automation/
├── Functional_OXPD/           # Functional test cases for OXPd embedded solution
├── Functional_Workpath/       # Functional test cases for Workpath agentless solution
├── Non_Functional_OXPD/       # Non-functional tests for OXPd (performance, stress, etc.)
├── NonFunctional_Workpath/    # Non-functional tests for Workpath
├── OXPD_Common/              # Shared libraries and utilities for OXPd tests
├── Workpath_Common/          # Shared libraries and utilities for Workpath tests
├── Cardreader_OXPD/          # Card reader authentication tests for OXPd
├── Cardreader_Workpath/      # Card reader authentication tests for Workpath
└── Mobile_Testing/           # Mobile device testing scripts
```

## File Types

Each test suite typically contains three file types:

| Extension | Description | Purpose |
|-----------|-------------|---------|
| `.gflib` | GFriend Library | Contains reusable function definitions and keywords |
| `.gfvar` | GFriend Variables | Stores test configuration variables and parameters |
| `.txt` | Test Scripts | Main test case definitions that orchestrate library functions |

## Key Test Areas

### Functional Testing

- Authentication (PIN, Username/Password, Short ID, Badge, SIO)
- Pull Print functionality
- Scan operations
- Document management and upload
- Web portal interactions
- Device configuration
- Report generation (Device/User/Department × Print/Scan)

### Non-Functional Testing

- Performance metrics (boot time, authentication timing)
- Wake from sleep scenarios
- Card swipe response times
- Print job throughput

### Authentication Methods

- Username & Password
- Short ID (PIN)
- Badge/Card reader (BadgeBox)
- Sign-In Once (SIO) - HP/Azure/Google
- Two-Factor Authentication

## Getting Started

### Prerequisites

- HP GFriend test automation framework installed
- Access to SafeQ Cloud tenant
- HP printer/MFP device with firmware supporting OXPd or Workpath
- Card reader (for badge authentication tests)
- Windows environment with required dependencies

### Configuration Steps

1. **Configure Variables**
   - Update `.gfvar` files with your environment-specific settings:

     ```text
     ${Site} = https://your-tenant.safeqcloud.com
     ${Username} = your-username
     ${Password} = your-password
     ${Device_IP} = 192.168.x.x
     ${Printer} = your-print-queue-name
     ```

2. **Set Up Test Dependencies**
   - Ensure web drivers (Chrome/Edge) are configured
   - Install BadgeBox if testing card authentication
   - Configure JediOmni for device control panel automation

3. **Run Test Cases**
   - Open test script (`.txt` file) in GFriend
   - Configure required variables
   - Execute desired test case

## Common Libraries

### OXPD_Common

Shared utilities for OXPd-based tests:

- `OXPD_Common_Library.gflib` - Core functions for device operations
- `OXPD_Common_Variables.gfvar` - Common variable definitions
- Functions: Portal login, device login/logout, job operations, pull print

### Workpath_Common

Shared utilities for Workpath-based tests:

- `Common_Workpath_Library.gflib` - Core functions for Workpath operations
- `Common_Workpath_Variables.gfvar` - Common variable definitions  
- Functions: Portal login, device operations, app interactions

## How to Run Tests Using GFriend

### Step-by-Step Execution Guide

1. **Launch GFriend Application**
   - Open the HP GFriend test automation tool on your Windows machine
   - Ensure all required GFriend plugins are installed (Web, JediOmni, Vision, BadgeBox, etc.)

2. **Open Test Script**
   - Navigate to `File > Open` or use Ctrl+O
   - Browse to the SCL_Automation folder
   - Select the desired `.txt` test script file (e.g., `Functional_OXPD.txt`)

3. **Configure Test Variables**
   - Before running, update the corresponding `.gfvar` file with your test environment details:
     - Device IP address and credentials
     - SafeQ Cloud portal URL and login credentials
     - Print queue names
     - File paths for test documents
   - Alternatively, you can override variables directly in GFriend's variable editor

4. **Select Test Case to Execute**
   - In the GFriend test script view, locate the specific test case you want to run
   - Test cases are named with format: `T<TaskID>_Test_Description`
   - Right-click on the test case name

5. **Run the Test**
   - Click **"Run Test Case"** from the context menu, OR
   - Use the Run button in the toolbar
   - Select execution options:
     - **Run All** - Execute all test cases in the script
     - **Run Selected** - Execute only the highlighted test case
     - **Debug Mode** - Step through test execution for troubleshooting

6. **Monitor Execution**
   - Watch the GFriend console for real-time execution logs
   - Green indicators show passed steps
   - Red indicators show failed steps
   - View detailed error messages in the output panel

7. **Review Results**
   - After execution completes, review the test report
   - Check pass/fail status for each step
   - Analyze screenshots captured during execution

### Tips for Successful Test Execution

- **Start Simple**: Begin with basic authentication tests before running complex scenarios
- **Check Prerequisites**: Verify device is powered on, connected, and at home screen
- **One at a Time**: Run individual test cases first to validate setup before batch execution
- **Watch for Prompts**: Some tests require manual intervention (e.g., badge swipe)
- **Network Stability**: Ensure stable network connection for web portal interactions
- **Clean State**: Reset device to home screen between test runs for consistency

### Troubleshooting GFriend Execution Issues

| Issue | Solution |
| ------- | ---------- |
| GFriend won't start | Check license activation and plugin installations |
| Script syntax errors | Verify `.gflib` and `.gfvar` files are in correct locations |
| Device not responding | Verify JediOmni connection and device IP address |
| Web portal timeout | Increase timeout values in Web.Wait operations |
| Badge reader errors | Ensure BadgeBox is running and card is enrolled |

## GFriend Documentation

For comprehensive information about using the GFriend test automation framework, including:

- Installation and setup guides
- Keyword reference documentation
- Plugin configuration (Web, JediOmni, Vision, BadgeBox, Android)
- Scripting best practices and examples
- Advanced features and troubleshooting

**Please refer to the official GFriend documentation:**