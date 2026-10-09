# Prerequisites

### 1. The print queue must be added in the STB plugin based on the solution configuration.

### 2. **Vision AI Server Details**  
   Provide the Vision AI server IP address and ensure it is configured with the correct API endpoint URL and port.

   **Example:**
  // ${STF_AzureEndpoint}=http://<IPAddress>:<port>
### 3. Before running the scripts, ensure the required support set images are added to the Vision AI server and that the server is running.

### 4. The gfvar file must be edited to match the required user configuration for the application to work correctly.

### 5. Before running Badge Authentication–related scripts, enable Cross-Origin Resource Sharing (CORS) on the EWS page:
  **Security → Web Service Security**

### 6. In the STB plugin, set the Pacekeeper scale to 3 for slow devices.

### 7. Ensure the BadgeBox is connected and that the badge/card is registered for a signed-in user.

### 8. Card indices are 0, 1, 2, and 3. Make sure the configured card index matches the card inserted in the BadgeBox.
  **To test Badge Authentication, set only ${Badge_Auth} to Y.**

### 9. In the STB plugin, configure the following values (used by the scripts):

  // STF_AzureEndpoint

  // STF_AzureKey

  // STF_UserName

  // STF_UserPassword

  // STF_BadgeIndex

### 10. Print Release Action Configuration
  Select the print action the script should perform:

  **SelectAllOption** – Releases and prints all jobs

  **SelectAllJobs** – Releases and prints all jobs under the Review Document option

  **Print_Release** – Prints or deletes selected jobs

  **DeleteAllJobs** – Deletes all jobs without printing
