Prerequisites
1. Device must be enabled with HP Workpath Debug Bridge.
2. HP Workpath platform must be enabled in the EWS Page.
3. Device must have RAM greater than 3GB.
4. Secure Print & Scan(YSoft SafeQ Cloud) Workapath app should be installed on the device from HPK Tool.
5. gfvar file must be edit in order to make application work on required user configuration.
6. Make sure Badgebox is connected and card is registerd for signed In
7. Card indices are 0, 1, 2, 3 so make sure the card index matches the card inserted in the badge box.
8. Need to install the SafQ-Cloud client and log in with credentials. 
9. If there are any existing print jobs, it will not allow to give prints.
10.Remove all the available print jobs in the Secure Print & Scan->"My Print Jobs" folder

Device Details :
Device Ip : 146.204.94.12
Device name : Ammolite
Model name : HP Color LaserJet MFP E78523 
Firmware version : 2507252_046153
Solution Name: YSoft SafeQ Cloud Workpath App
Solution Version: 3.43.4


Test Results : 
1) Duration:1hr   StartDate:21-06-2024   EndDate:21-06-2024   Total:37   Pass:31   Fail:6   Error:0   Pass%:83.70%
   Fail 1 ::
   Fail Line		: Android.Wait For Object (com.ysoft.safeq.terminals.hp.hcp:id/printAll,20)
   Fail Reason 1	: For the first time Badge swipe is getting passed , but the device is not signed in.Once clicked on the Secure print & Scan getting userlogin screen for 1 second and going to the authentication error screen 
   Fail Reason 2	: For the first time Badge swipe is getting passed , but the device is not signed in.Once clicked on the Secure print & Scan getting userlogin screen and it stays in same screen between 1m 20s to 1m 30s
   Fail Type		: Intermittent

   Fail 2 ::
   Fail Line		: Android.Wait For Object (com.ysoft.safeq.terminals.hp.hcp:id/success_job_image,300)
   Fail Reason		: Device went to loading screen(hanged state)
   Fail Type		: Intermittent


2) Duration:72hr   StartDate:21-06-2024   EndDate:24-06-2024   Total:2893   Pass:2882   Fail:11   Error:0   Pass%:99.61%
   Fail 1 ::
   Fail Line		: Android.Wait For Object (com.ysoft.safeq.terminals.hp.hcp:id/print_cancel,10)
   Fail Reason 1	: After clicking on My print jobs , the print job screen is loading and the print job screen is shown after 10-15 seconds
   Fail Reason 2	: After clicking on My print jobs , the jobs are not available and the folder is empty.
   Fail Reason 3	: After submitting the job , the print cancel popup disapperas within a second and moves to success popup
   Fail Type		: Intermittent

   Fail 2 ::
   Fail Line		: JediOmni.Wait For Object (hpid-5f271ead-1a46-4f72-b261-22f9d769ea1c-homescreen-button,20)
   Fail Reason		: For the first time Badge swipe is getting passed , but the device is not signed in and the screen goes to user login screen 
   Fail Type		: Intermittent