# Data Driven Test

## Basic repetation with Excel file
You can repeat each row of Excel table while GFriend execution. With this you can load variables from Excel file and write result to it.

Following table is option combination test cases of Printer driver. (You can get full Excel file [here](.resources/HallasanCombination.xlsx).)


| TC_ID | TestFile | DocumentMargin | Orientation | ColorMode  | HPEasyColor             | TestResult |
|-------|----------|----------------|-------------|------------|-------------------------|------------|
| 1     | pdf      | On             | Portrait    | Color      | HPEasyColorConservative |            |
| 2     | xls      | Off            | Landscape   | Grayscale  | HPEasyColorOn           |            |
| 3     | doc      | Off            | Portrait    | Monochrome | HPEasyColorOff          |            |
| 4     | doc      | On             | Portrait    | Color      | HPEasyColorOff          |            |
| 5     | pdf      | On             | Landscape   | Grayscale  | HPEasyColorOn           |            |

With this table, you can make option combination test as follow:

```javascript
DataSet HallasanCombination.xlsx as HallasanOptions
Resource TESTFILES\office_A4.pdf As pdf
Resource TESTFILES\Office_A4.xls As xls
Resource TESTFILES\Office_A4.doc As doc

using Hallasan

Option Combination Test
{
    For Each Row:HallasanOptions.OptionCombination.Combination1
    {
      Hallasan.Open File For Print (${TestFile},Hallasan)
      Hallasan.Get Driver Window(Hallasan)
      Hallasan.Set Toggle (Part.DocumentMargins,${DocumentMargin})
      Hallasan.Set Radio (Option.${Orientation})
      Hallasan.Set Radio (Option.${ColorMode})
      Hallasan.Set Combobox (Part.DocumentHPEasyColor,Option.${HPEasyColor})
      Hallasan. Confirm
      sleep(5)
      Hallasan.Click Print Button
      sleep(5)
      Hallasan.Close Application
    }
}
```

## Guided semi auto test with GFriend

Following table is option combination test cases with paper handling. (You can get full Excel file [here](.resources/BasicPrintTest.xlsx).)


| TC ID | Tray  | Paper Size | Paper Type | Test Prn               | Result |
|-------|-------|------------|------------|------------------------|--------|
| 1     | Tray2 | Letter     | Plain      | Letter_Plain_Tray2.prn |        |
| 2     | Tray3 | A4         | Thick      | A4_Thick_Tray3.prn     |        |
| 3     | Tray1 | A3         | Plain      | A3_Plain_Tray1.prn     |        |
| 4     | Tray3 | Letter     | Thin       | Letter_Thin_Tray3.prn  |        |
| 5     | Tray2 | Legal      | Thin       | Legal_Thin_Tray2.prn   |        

With this data set, you can create guide auto test as below:

```javascript
Dataset BasicPrintTest.xlsx as BasicPrint
using Fleet

Guided Autotest Sample
{
    For Each Row:BasicPrint.Combination.PaperTest
    {
        // Request user to load proper paper
        Wait For User Confirm(Please load ${PaperType} ${PaperSize} at ${Tray} and Click OK)

        // Send prn file to device
        Fleet.Send File (TestFile/${TestPrn})

        // Request user to verify the printed output
        Request User Verification (Check output)

        // Update result to Excel file
        Write To Data Set (Result,${KEYWORD_OUTPUT})
    }
}
```
Tester's verification message will be written in result Excel file after test execution.

## Run Keyword with Data Driven

You can call keyword which is defined in dataset by using Builtin.Run Keyword. Following table is sample web open test cases. (You can get full Excel file [here](.resources/WebTestData.xlsx).)

| TCNum | Keyword To Run  | Start Time | End Time | Result | 
|-------|-----------------------|--------|-------|---------|
| 1     | WebOpen.Open Google(${TCNum}) |||||
| 2     | WebOpen.Open HP(${TCNum}) |||||
| 3     | WebOpen.Open Microsoft(${TCNum}) |||||
| 4     | WebOpen.Open HP(${TCNum}) |||||
| 5     | WebOpen.Open Microsoft(${TCNum}) |||||

And WebOpen.gflib is defined as below

```javascript
using Web

Open Google(${iteration})
{
    Web.Open with Chrome (google.com)
    Web.Capture Screenshot (${iteration}_google.com)
    Web.Close
}

Open HP(${iteration})
{
    Web.Open With Chrome(www.hp.com)
    Web.Capture Screenshot (${iteration}_hp.com)
    Web.Close
}

Open Microsoft(${iteration})
{
    Web.Open with Chrome (www.microsoft.com)
    Web.Capture Screenshot (${iteration}_ms.com)
    Web.Close
}
```

With dataset and custom library above, you can create test suite like below:

```javascript
Dataset WebTestData.xlsx As Data
using WebOpen.gflib

Page Open Test
{
    For Each Row:Data.WebTest.TestCases
    {
        // Write start time to dataset
        Get Time Stamp (${time})
        Write To Data Set (StartTime,${time})
        
        // Run dataset defined keyword
        Run Keyword (${KeywordToRun})
        
        // Write test result to dataset
        Write To Data Set (Result,${KEYWORD_RESULT})
        
        // Write end time to dataset
        Get Time Stamp (${time})
        Write To Data Set (EndTime,${time})
        
    }
}
```
This test will repeatably execute keyword in Keyword To Run column within For Each Row block.

# For Selected Row

## For Selected Row: 
Filters the dataset based on specific conditions you define (e.g., !=, <, >, =, <=, >=).
## Test Execution:
Only the rows that match the filter criteria will be executed in the test script.
## Conditions Used:
!= : Not equal to.
= : Equal to (can be used for multiple values separated by commas).
<, >, <=, >= : Comparison operators for numerical or date values.


| TCID   | PaperSource | PaperSize   | PaperType  | TestPrn               | ColorMode |
|-------|--------------|-------------|------------|------------------------|-----------|
| 1     | Tray2        | Letter      | Plain      | Letter_Plain_Tray2.prn |Color      |
| 2     | Tray3        | A4          | Thick      | A4_Thick_Tray3.prn     |Black      |
| 3     | Tray1        | A3          | Plain      | A3_Plain_Tray1.prn     |Black      |
| 4     | Tray3        | Letter      | Thin       | Letter_Thin_Tray3.prn  |Color      |
| 5     | Tray2        | Legal       | Thin       | Legal_Thin_Tray2.prn   |Color      |
| 6     | Tray2        | Letter      | Plain      | Letter_Plain_Tray2.prn |Color      |
| 7     | Tray3        | A4          | Thick      | A4_Thick_Tray3.prn     |Black      |
| 8     | Tray1        | A3          | Plain      | A3_Plain_Tray1.prn     |Black      |
| 9     | Tray3        | Letter      | Thin       | Letter_Thin_Tray3.prn  |Black      |
| 10    | Tray2        | Legal       | Thin       | Legal_Thin_Tray2.prn   |Color      |



```javascript
tc_PaperSourceNotTray1AndTray3
{
    
        //Condition: Select rows where the PaperSource is not Tray1 or Tray3.
       //This allows you to filter out specific paper sources and run tests for all others
    For Selected Row:Test.OptionCombination.Combination1.PaperSource!=Tray1,Tray3
    {
        Pass (${TCID}--${PaperSize}--${PaperType}--${PaperSource}---${TestPrn}--${ColorMode})

    }
}
```

```javascript
tc_PaperSourceEqualtoTray1
{
    //Condition: Select rows where the PaperSource is specifically Tray1.
     //This ensures that only tests for Tray1 will be executed
    For Selected Row:Test.OptionCombination.Combination1.PaperSource=Tray1
    {
        Pass (${TCID}--${PaperSize}--${PaperType}--${PaperSource}---${TestPrn}--${ColorMode})
    }
}

```

```javascript
tc_TCIDEqualtoSpecificNumbers
{
   // Condition: Select rows where TCID is one of the values 1, 5, 7, 9, or 10.
   //This filter helps you execute tests only for specific test cases based on their TCID
    For Selected Row:Test.OptionCombination.Combination1.TCID=1,5,7,9,10
    {
        Pass (${TCID}--${PaperSize}--${PaperType}--${PaperSource}---${TestPrn}--${ColorMode})
    }
}

```

```javascript
tc_TCIDGrearthan
{
    //Condition: Select rows where TCID is greater than 7.
    //This will filter in test cases where the TCID is larger than 7 and execute them.

    For Selected Row:Test.OptionCombination.Combination1.TCID>7
    {
        Pass (${TCID}--${PaperSize}--${PaperType}--${PaperSource}---${TestPrn}--${ColorMode})
    }
}
```

```javascript
tc_TCIDLessthanorEqual
{
   // Condition: Select rows where TCID is less than or equal to 5.
   //This allows you to filter in test cases with a TCID of 5 or lower.

    For Selected Row:Test.OptionCombination.Combination1.TCID<=5
    {
        Pass (${TCID}--${PaperSize}--${PaperType}--${PaperSource}---${TestPrn}--${ColorMode})
    }
}
```

```javascript
tc_TCIDGreaterthanorEqual
{
    //Condition: Select rows where TCID is greater than or equal to 4.
   //This filter focuses on test cases with TCID 4 and higher.
    For Selected Row:Test.OptionCombination.Combination1.TCID>=4
    {
        Pass (${TCID}--${PaperSize}--${PaperType}--${PaperSource}---${TestPrn}--${ColorMode})
    }
}

```

```javascript
tc_ColorModeequalBlack
{
    //Condition: Select rows where ColorMode is Black.
    //This will only run the tests for rows where the ColorMode is set to Black
    For Selected Row:Test.OptionCombination.Combination1.ColorMode=Black
    {
        Pass (${TCID}--${PaperSize}--${PaperType}--${PaperSource}---${TestPrn}--${ColorMode})
    }
}

```