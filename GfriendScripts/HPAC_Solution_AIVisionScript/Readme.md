
# Prerequisites

  

Before running the script, ensure the following configurations are set correctly.

  

---

  

### 1. The printer must have the HP AC Secure Pull Print solution installed.

  

---

  

### 2. Username and Password Setup

```

// ${STF_UserName}=

// ${STF_UserPassword}=

```

  

---

  

### 3. Authentication Type Selection

Choose how the script should authenticate.

**Y → Badge authentication**

**N → Username/password authentication**

  

Example:

```

${BADGE_AUTH}= N

```

  

---

  

### 4. Badge Index (Required Only for Badge Authentication)

If badge authentication is enabled, specify the badge index value.

  

Example:

```

${STF_BadgeIndex}= 1

```

  

---

  

### 5. Print Release Action Configuration

Select the print action the script should perform.

Available options:

-  **PrintAll** – Releases and prints all jobs

-  **PrintKeep** – Prints jobs but keeps them stored

-  **PrintDelete** – Prints and deletes jobs

-  **Delete** – Deletes jobs without printing

  

Example (active option):

```

${PRINT_RELEASE_ACTION}= PrintKeep

```

  

Other options:

```

// ${PRINT_RELEASE_ACTION}= PrintAll

// ${PRINT_RELEASE_ACTION}= PrintDelete

// ${PRINT_RELEASE_ACTION}= Delete

```

  

---

  

### 6. Vision AI Server Details

Provide the Vision AI server IP and ensure it is set to the correct API endpoint URL and port.

  

Example:

```

// ${STF_AzureEndpoint}=serverip:port(end point)

```