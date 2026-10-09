# Automation Scripts for 'HP Scan AI Enhanced'
	When run, these scripts will automate the flow of 'HP Scan AI Enhanced'

## Installation Instructions
	Please follow the steps mentioned in confluence document for installing the application.
	[Installing HP Scan AI Enhanced on printer](https://rndwiki.inc.hpicorp.net/confluence/display/SQEI/Installing+%27HP+Scan+AI+Enhanced%27+On+Printers)
	[Onboarding printer in HP Command center](https://rndwiki.inc.hpicorp.net/confluence/pages/viewpage.action?pageId=161403992
	[Installing HP Scan AI Enhanced using HPCC for printers in production mode](https://rndwiki.inc.hpicorp.net/confluence/pages/viewpage.action?pageId=1611517276)
	[Installing HP Scan AI Enhanced via sideloading using HPKTool and MocTool](https://rndwiki.inc.hpicorp.net/confluence/pages/viewpage.action?pageId=1611517280)

 
## Usage Instructions
	These automation scripts work only for printers that are in staging mode and configured to use flatbed instead of ADF.
	Please make sure that configuration has been changed to use flatbed instead of ADF. This can be done while configuring
	the application using MOCTool as mentioned in the document  [Installing HP Scan AI Enhanced](https://rndwiki.inc.hpicorp.net/confluence/pages/viewpage.action?pageId=1611517280)
	via sideloading using HPKTool and MocTool

	To simulate the ADF, we are scanning single page multiple times (Default is 5). If you wish to increase the number of 
	scanned pages in a document, you can do so by changing the value of `${ScanCount}` variable in `app-or-portal-validation/printer-identifier.gfvar`,
	`delivery-notes-document-seperation/identifier-variables.gfvar`, `demonstration-project-uk-invoices-and-delivery-notes/identifier-variables.gfvar` or
	`production-firmware-vision/identifier-variables.gfvar` file. We suggest against it, because there is no functionality available to scroll down in document validation screen.

	You also need to change the value of `${UserEmail}` and `${UserPassword}` in app-or-portal-validation/web-portal-identifier.gfvar file. Please use the email Id
	and password of the Aluma account you've used to install application on the printer. The documents will be uploaded to that account and can only be accessed in that.

	Follow the steps mentioned below to run these automation scripts:
		1. Open GFriend in your computer and add the target device by giving the IP Address of the printer.
		2. Clone/Download the ScanAI folder as zipped folder into your computer.
		3. Open the ScanAI folder in GFriend.You can do this by clicking on folder icon and navigating to the folder in your
			drive.
		4. Open *.test-driver in GFriend, select the test cases you want to run and click on green play button on right side of 
			Gfriend.