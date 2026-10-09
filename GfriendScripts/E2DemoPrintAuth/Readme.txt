Steps to follow before running the script 
------------------------------------------
>Before deploying Pull Print solution to the printer. set guest user password in User Management tab and use it in the script
(script AutomateE2DemoPullPrintAuth.txt is being used password 1234 for guest user).   
>Deploy Pull Print solution to the printer.
>Run the script.


Note:- 
------------------------------------------
>AutomateE2DemoPullPrintAuth.txt has script to login as guest and Releasing pull print job and login as jDoe and Releasing pull print job.
>Test_SigninAsguest.txt for login as guest and Release Job as guest User.
>Test_SigninAsJDoe.txt for login as jDoe and Release Job as jDoe User.
>While running the whole script(AutomateE2DemoPullPrintAuth.txt) to login as guest and Releasing pull print job and login as jDoe 
and Releasing pull print job, devices are Entering into the Error State.
so, I have run the script for Release Job as guest User Separately(Test_SigninAsguest.txt) and Release Job as jDoe User
Separately(Test_SigninAsJDoe.txt), scripts are working with multiple sleep statements.


script(AutomateE2DemoPullPrintAuth.txt) follows the below steps
--------------------------------
0.At solution server web page (when you start the application from your visual studio), go to "User Management" menu and update password for "guest"

1.Launch "Sign In" of Control Panel

2.Select "Authentication agent"  at "Sign-In Method"

3.At User Name Field and type "guest"

4.At Password field, remove all characters and type passcode you set at step 0.

5.Click Login , Click Pull Print Auth Demo

6.Select the document (TestPattern.pdf)

7.Click Print Button 

8.Wait until the job is finished. Wait until you can see "Success" message right after "TestPattern.pdf."

9.go back to home screen and click "Sign out"

11.Launch "Sign In" of Control Panel

12.Select "Authentication agent"  at "Sign-In Method"

13.At User Name Field and type "jDoe"

14.At Password field, remove all characters and type passcode "janeDoe".

15.Click Login , Click Pull Print Auth Demo

16.Select two documents (city.jpg and frog.jpg)

17.Click Print Button 

18. Wait until you can see "Success" message right after two documents.

19.go back to home screen and click "Sign out"
