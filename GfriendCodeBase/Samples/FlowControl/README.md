# Flow Control and Operators

## Repeat
For simple repeating the Keywords use Repeat with repeat count.

```javascript
using Android

Repetation Test
{
    // Press back key for 5 times
    Repeat:5
    {
        Android.Press Back Key
    }
}
```

Can use system defined variable ${R} within the Repeat to get the current repeat count.

```javascript
using Android

Create Multiple Job
{
    Repeat:10
    {
        Android.Touch Text(Scan)
        Android.Set Text(hp.scan:id/JobName,ScanJob${R})
        Android.Touch Text(Send)
        Sleep(10)
    }
}
```

By using Repeat and Assert Last Repeat keyword, you can create stability, reliability test script as below example. (See also usage of ignore fail operator @)

```javascript
using Web

Cloud Solution Reliability Test
{
    Web.Open (localhost:8000/mysolution)
    
    // use @ to ignore result of Repeat block
    @Repeat:1000
    {
        Web.Click Object(//*[@id='AddButton'])
        Web.Set Text(//*[@id='Name'],User${R})
        Web.Click Object(//*[@id='OkButton'])
        Web.Wait For Text(User${R})
    }
    
    // out of 1000 repetation, pass rate should be 99%
    Assert Last Repeat(PASS,>990)
}
```

## New Feature in Repeat

For repeating the Keywords for specified duration, can use Repeat with time in seconds/minutes/hours.

```javascript
${count}=0

Repetition Test With Time In Seconds
{
    //will repeat the below repeat block for 90 seconds
    Repeat:90s
    {
        Calculate And Assign (${count}+1,${count})
        Pass (${count})
    }
}
```

For repeating the Keywords for minutes use Repeat with time in minutes.

```javascript
using JediOmni

Repetition Test With Time In Seconds
{
    //will repeat the below block for 10 minutes
    Repeat:10m
    {
        JediOmni.Wait For Object (hpid-reports-app-screen,5)
        JediOmni.Touch ID (hpid-reports-app-screen)
    
        JediOmni.Wait For Object (hpid-tree-node-listitem-configurationpages,5)
        JediOmni.Touch ID (hpid-tree-node-listitem-configurationpages)
    
        JediOmni.Wait For Object (hpid-report-page-checkbox-5fa225be-ceec-4152-987c-e2417938de74,5)
        JediOmni.Touch ID (hpid-report-page-checkbox-5fa225be-ceec-4152-987c-e2417938de74)
    
        JediOmni.Touch Text (Cancel)
        sleep(3)

        JediOmni.Press Home Key
    }
}
```

For repeating the Keywords for hours use Repeat with time in hours.

```javascript
using JediOmni

Repetition Test With Time In Seconds
{
    //will repeat the below block for 72 hours
    Repeat:72h
    {
        JediOmni.Wait For Object (hpid-reports-app-screen,5)
        JediOmni.Touch ID (hpid-reports-app-screen)
    
        JediOmni.Wait For Object (hpid-tree-node-listitem-configurationpages,5)
        JediOmni.Touch ID (hpid-tree-node-listitem-configurationpages)
    
        JediOmni.Wait For Object (hpid-report-page-checkbox-5fa225be-ceec-4152-987c-e2417938de74,5)
        JediOmni.Touch ID (hpid-report-page-checkbox-5fa225be-ceec-4152-987c-e2417938de74)
    
        JediOmni.Touch Text (Cancel)
        sleep(3)

        JediOmni.Press Home Key
    }
}
```

## While
With While, you can repeat certaion block with condition.

```javascript
using Android

HP Smart App Launch
{
    Android.Launch (com.hp.printercontrol/com.hp.printercontrol.base.PrinterControlActivity)
    
    // Wait until GFriend can touch App Settings text
    While:!Android.Touch Text (App Settings):10
    {
        Sleep(1)
    }
}
```
As example above, you can use ! operator to use false condition.

While loop is usuallly used for waiting job is finished. Following example is sample linux printing.

```javascript
Resource test.pdf As TestPDF
using Ssh
using LP

Linux Print Test
{
    Ssh.Connect
    
    // Preparing document to print
    Ssh.Push File (TestPDF,test.pdf)
    
    // Setting jobs
    LP.Select File (test.pdf)
    LP.Select Printer (testPrinter)
    LP.Set Option (MediaType,Plain)
    LP.Set Option (PageSize,Letter)
    LP.Get Lp Command (${printcmd})
    
    
    // Send Job
    Ssh.Send (${printcmd})
    
    Ssh.Send (lpstat -R)
    
    // Wait until job done
    // Check lpstat -R output is empty
    While:!Equals(${KEYWORD_OUTPUT},${EMPTY})
    {
        Sleep(2)
        Ssh.Send (lpstat -R)
    }
    
    // Download File to local
    Ssh.Sudo Send (chmod 777 test.prn)
    Ssh.Get File (test.prn,${OUTPUT_FOLDER}\test.prn)
    
    // Verify prn file
    File Contains Text (${OUTPUT_FOLDER}\test.prn,<</MediaType \(Plain\)>> setpagedevice)
    File Contains Text (${OUTPUT_FOLDER}\test.prn,<</PageSize [612 792] /ImagingBBox null>> setpagedevice)
    
    Ssh.Disconnect
}
```

## IF

You can handle pop-up message or unexpected behavior during test with if blocks.

```javascript
using Android

SIO_Logout 
{
    If: Android.Wait For Object(com.hp.print.horizontalconnector.box:id/fab,30)
    {
        Android.Press Back Key
        If:Android.Wait For Text (Exit,10)
        {
            Android.Touch Text (Exit)
        }
    }
}
```

Stop Test Suite or Stop Test Case can be used with if block to insure clean up.

```javascript
using Web
using BHST.gflib

TestRail Create Test Run
{
    BHST.TestRail Login (${UserID},${UserPW})
    
    // Create Test Run
    Web.Go To (https://testrail.tools.cso-hp.com/index.php?/runs/add/${BH_SuiteID})
    
    // Input Test Run Details
    Web.Set Text (//*[@id="name"],${TestRunName})
    Web.Click Object (//*[@id="includeSpecific"])
    Web.Click Object (//*[@id="includeSpecificInfo"]/a)
    
    // Select Secton
    Web.Click Text (GoogleTest)
    Web.Wait For Text (Google Positive Test,10)
    Web.Click Object (//*[@id="selectCasesGroupAll"])
    Web.Click Object (//*[@id="selectCasesSubmit"])
    
    @Web.Wait For Object Disappear (//*[@id="selectCasesSubmit"],30)
    If:Web.Click Object (//*[@id="form"]/div[9]/button)
    {
        Pass(Test Run Created)
    }
    
    // If test fail or error delete script which is uploaded in previous test case
    Fail:
    {
        Fail(Fail to create test run)
        BlackHole.Delete Script
        Stop Test Suite
    }
    Error:
    {
        Error(Error during create test run)
        BlackHole.Delete Script
        Stop Test Suite
    }
    
    Web.Get Text (//*[@id="content-header"]/div/div[1],${TestRunID})
    BuiltIn.Trim (${TestRunID},R)
    Web.Close
}
```

## For

It is a control flow statement and executes the code written in the body of the loop for each element of the array or collection.

```javascript
${DocTypes}=Pdf, Doc, xls, xlsx, CSV

Test_ForLoop
{
        for:${DocTypes}
        {
            Pass (${ITEM}) //prints each item of ${DocTypes} in the report file
        }
} 
```

Nested for loop : can use for loop within another for loop.

```javascript
${DocTypes}=Pdf, Doc, xls, xlsx, CSV
${Iteartions}=1,2,3

Test_ForLoop
{
        for:${DocTypes}
        {
            Pass (${ITEM}) //prints each item of ${DocTypes} in the report file
            for:${Iteartions}
            {
                Pass (${ITEM}) //prints each item of ${Iteartions} in the report file
            }
        }
} 
```