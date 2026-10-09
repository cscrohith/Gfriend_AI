Prerequisites :

Scripts for HP mode:
1. Make sure it's in HP Mode OR else need to set. In EWS page go to general -> HomeScreen Customization in that Home Screen App option select HP and click Apply.
2. Make sure Badgebox is connected and card is registerd for signed In
3. Card indices are 0, 1, 2, 3 so make sure the card index matches the card inserted in the badge box.
4. To check badge authentication, set only ${Badge_Auth} to Y 
5. And set the other variables to N (${LAZY_AUTH} ,${SHORT_ID}) in ac variable file.
6. while adding print jobs in print queue, need to check any local printers are added or not.
  (if we use any remote network then click show option->click local resourse and uncheck printers and clipboard)
7. Need to set the inactivity time as 300 seconds.
8. Device Details :
 Device Name: Jasper
 IP : 146.204.93.46 
 FW :  2507252_046156
 Model Name : HP Color LaseJet Flow E87760
Solution Name : YSoft SafeQ Solution
YSoft SafeQ Diver version : 3.43.4

Duration Test Results :
1) Test results for 24-hours 
   Date : 13-june-2024
   Total: 1053 Pass: 1051 Fail: 2 Error: 0
   Line numbers :938 and 940 
   Error type: Intermittent issue
   Failure : In failure case, twice i got connection error screen as after first time swiping badge and checking for sign-out is available in Home screen.


2) Test results for 72-hours 
   Date : 17-june-2024
   Total: 3175 Pass: 3174 Fail: 1 Error: 0
   Line numbers :938 and 940 
   Error type: Intermittent issue
   Failure : For the first time swiping the badge in Home screen instead of signin screen, will get user login screen.
   If we are in user login screen, swiping the badge for 2nd time but screen went back to Home screen.  

3) Test results for 24-hours 
   Date : 20-june-2024
   Total: 3140 Pass: 3137 Fail: 3 Error: 0
   Line numbers :1211, 1227 and 2606 
   Error type: Intermittent issue
   Failure :
   1. Waiting time issue, after getting Print Document screen, it went back to home screen and sigining-out.
   2. After click on Secure Print, intermittently will get user login screen. so, we are failing that iteration.
   3. In Print Document Screen, we are selecting print job, through script its passed but it's not selected print job in the control panel.

4) Test results for 4-hours 
   Date : 21-june-2024
   Total: 218 Pass: 217 Fail: 1 Error: 0
   Line number :136 
   Error type: Intermittent issue
   Failure : The failure is related to waiting time, i gave 30 sec to wait for print screen appear "Oxpd.Wait For Text (Document Name,30)" but it waited only for 9sec. 
   Getting Exception - "Wait for text failed with give timeout : Document Name"

5) Test results for 72-hours 
   Date : 24-june-2024
   Total: 3863 Pass: 3862 Fail: 1 Error: 0
   Line numbers :136 
   Error type: Intermittent issue
   Failure : After swiping the badge for second time if it is in user login screen, fail the iteration with message as "Badge was not SignedIn in the user login screen for second time".
   if it's not swiped, check again the user login screen is still visible and if the user login screen exists , fail the iteration with message as "Badge is scanned but still present in the user login screen".


