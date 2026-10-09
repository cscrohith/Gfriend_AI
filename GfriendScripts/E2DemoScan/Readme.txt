Steps to follow before running the script 
------------------------------------------  
>Deploy Simple Scan solution to the printer.
>Run the script(AutomateE2DemoScan.txt).


Note:- 
------------------------------------------
>AutomateE2DemoScan.txt has script to Release Scan job,
>While running the script(AutomateE2DemoScan.txt) to Release Scan job, devices are Entering into the Error State.
so, I have used sleep statement after every statements to Run duration test for 1 hour. 
script is working fine with multiple sleep statements.
>verify the scan job. we can able to see scanned files in scan folder
(path: OXPd2-SDK-Preview-5-20230320.1\demos\simpleScan\dotnet\SimpleScan\ScanFiles).


script(AutomateE2DemoScan.txt) follows the below steps
--------------------------------
1.Launch Scan App through Dune Keyword.

2.Wait until to see "Please press the Start button to initiate a scan"

3.Click "Start" button right bottom in green.

4.Wait until "Scan" job is finisehd through Dune Keyword.

5.Back to Home.