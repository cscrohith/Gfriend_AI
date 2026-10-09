Prerequisites

1. Enable **USB Debugging** and **Wi-Fi Debugging** on the Android test device.

2. Connect the device to the laptop using a USB cable.

   * Note: This test case is not supported on servers.

3. Ensure the Android Platform Tools package is available on the desktop.

   * Update the following variable with the Platform Tools path:

     * `${Platformtool_Path}`

4. Ensure the SafeQ application is not already installed on the mobile device before executing the test case.

   * Verify the package name configured in:

     * `${Package_name}`

5. On the Android device, create or copy a Word document named as **test.docx** and save it under the **Documents** folder.

   * Example path:

     * `/storage/emulated/0/Documents/test.docx`

6. Create a Pull Print Queue in the SafeQ tenant.

   * The Pull Print Queue name should match the printer name being used for testing.
   * Update:

     * `${Print_Queue_Name}`

7. Update the GFVAR file with the required test environment details:

   * `${Site}` – SafeQ tenant URL (e.g., hpautomation.us.ysoft.cloud)
   * `${Username}` – SafeQ user name
   * `${Password}` – SafeQ password
   * `${Package_name}` – SafeQ Android application package name
   * `${Platformtool_Path}` – Android Platform Tools path
   * `${Print_Queue_Name}` – Pull Print Queue name
   * `${Job_Name}` – Print job name (if applicable for the test case)
