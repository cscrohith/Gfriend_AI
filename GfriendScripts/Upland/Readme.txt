Steps to Configure Test Scenario for Upland Intelligent Capture:
---------------------------------------------------------------
Current Script works for both Myfiles and Accounts Payable Workflows

MyFiles:
--------
1.To the Test Scenario add Authentication Plugin and configure it with Printer Device.
2.Also add Gfriend Execution Plugin and attatch the GFriend Scripts which are placed in the Local path 
and configure the Printer device.
3.Select Upland.gfvar file and click on Edit Script.
4.Provide Profile Name as ${Profile_NAME} = MyFiles.
5.Make sure to Check the Checkbox Use ${STF_ScanFileName} Varaible to generate the scan file name automatically.
6.Save the changes and Continue with Running the Scenario.

Test Results :
Device - 192.168.0.190 (Ammolite)
Firmware version - 2507252_046153 (FS5.7.1.1)
Solution - HP Intelligent Capture
Solution version - V2024R2.1
Results - 1hr duration (Total:32 Pass:32 Fail:0 Error:0 Pass%:100%)

Accounts Payable:
-----------------
1.To the Test Scenario add Authentication Plugin and configure it with Printer Device.
2.Also add Gfriend Execution Plugin and attatch the GFriend Scripts which are placed in the Local path 
and configure the Printer device.
3.Select Upland.gfvar file and click on Edit Script.
4.Provide Profile Name as ${Profile_NAME} = Accounts Payable.
5.Make sure to Check the Checkbox Use ${STF_ScanFileName} Varaible to generate the scan file name automatically.
6.Save the changes and Continue with Running the Scenario.

Google Chrome Site Settings:
----------------------------
This step is to add Intelligent Capture and FileBound urls to trusted sites inorder to load properly in the Automation Process.

1. Opening settings in Chrome
2.Settings-->Security and Privacy-->Site Settings. Scroll down to the bottom of the page.
3.Click on Insecure Content and Add below IC and FB urls to Show insecure content.
https://hpbevankirby.filebound.com
https://uic-stg.uplandcapture.com
