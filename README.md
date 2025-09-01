# NICE Home Assignment – SuggestTaskService

## Overview
An **ASP.NET Core (.NET 8)** Web API implemented in **C#** that exposes a single endpoint:  
- **POST** `/suggestTask` – receives a user utterance and returns a **suggested task** based on **regular expression matching** (case-insensitive).  

The project demonstrates:  
1. **Validate input** using FluentValidation in C#.  
2. **Add logging** – console logs for requests and responses.  
3. **Match the utterance** against predefined task patterns (via regex). 
4. **Retry mechanism** – simulates an external dependency that may randomly fail (50%), with up to 3 retry attempts before returning an error.
5. **Return a JSON response** with the suggested task, or `"NoTaskFound"` if no match is found.  
6. **Unit and integration tests** to validate both the matching logic and the API behavior.  
 
---

## Installation, Setup & Dependencies  

### Prerequisites  
- .NET 8 SDK installed  

### Setup  
1. Clone the repository:  
   ```bash
   git clone <repo-url>
   cd home-assignment-nice
   ```  

2. Restore dependencies:  
   ```bash
   dotnet restore
   ```  

3. Build the project:  
   ```bash
   dotnet build
   ```  

4. Run the API:  
   ```bash
   dotnet run --project SuggestTaskService
   ```  

The service will start and be available at `http://localhost:5000` (or a similar port printed in the console).  

### NuGet Packages  
- These are .NET packages (not folders) restored by `dotnet restore`.

**Main project**:  
- `FluentValidation.AspNetCore` – input validation  
- `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.OpenApi` – Swagger / OpenAPI documentation  

**Test project**:  
- `xunit`, `xunit.runner.visualstudio` – unit testing framework  
- `Microsoft.AspNetCore.Mvc.Testing` – integration testing utilities  
- `System.Net.Http.Json` – helpers for HTTP testing  
- `coverlet.collector` – code coverage collection  

---

## Project Structure


```
home-assignment-nice/
├─ SuggestTaskService/                # Main Web API project
│  ├─ Properties/                     # Project metadata and launch settings
│  ├─ controllers/                    # API controllers (SuggestTaskController.cs)
│  ├─ models/                         # Data transfer objects (e.g., SuggestTaskRequest.cs)
│  ├─ validators/                     # Input validation rules with FluentValidation
│  ├─ Program.cs                      # Application entry point and configuration
│  ├─ SuggestTaskService.csproj       # Project definition file
│  ├─ SuggestTaskService.http         # Local HTTP test file for API requests
│  ├─ appsettings.json                # Application configuration (base)
│  └─ appsettings.Development.json    # Development-specific configuration
│
├─ SuggestTaskService.Tests/          # Test project
│  ├─ MatchTaskTests.cs               # Unit tests for the matching logic
│  ├─ SuggestTaskIntegrationTests.cs  # Integration tests for the API endpoint
│  ├─ ExternalCallTests.cs            # Unit tests for the retry simulation (SimulatedExternalCall)
│  └─ SuggestTaskService.Tests.csproj # Test project definition
│
├─ .gitignore                         # Git ignore rules
├─ HomeAssignmentNice.sln             # Solution file
└─ README.md                          # Project documentation
```

---


## Usage  

The API exposes a single endpoint that uses **regular expression (regex) matching** to identify tasks.  
Regex patterns allow more flexible matching than a strict dictionary: for example, both *"reset my password"* and *"I forgot the password"* will be matched to `ResetPasswordTask`.  
The matching is **case-insensitive** and ignores extra spaces or punctuation.  
Additionally, for each request the server simulates a call to an **external dependency**:  
- If the dependency succeeds (within up to 3 attempts), the API responds normally.  
- If all 3 attempts fail consecutively, the API returns **503 Service Unavailable** with a JSON error:
  ```json
  {
    "error": "External dependency failed after 3 consecutive attempts"
  }

**Endpoint**:  
```
POST /suggestTask
Content-Type: application/json
```

**Request example**:  
```json
{
  "utterance": "I need help resetting my password",
  "userId": "12345",
  "sessionId": "abcde-67890",
  "timestamp": "2025-08-21T12:00:00Z"
}
```

**Successful response (200 OK)**:  
```json
{
  "task": "ResetPasswordTask",
  "timestamp": "2025-08-21T12:00:01Z"
}
```

**When no match is found**:  
```json
{
  "task": "NoTaskFound",
  "timestamp": "2025-08-21T12:00:01Z"
}
```

**When input is invalid (400 Bad Request)**:  
```json
{
  "errors": {
    "utterance": ["Utterance must not be empty"],
    "timestamp": ["Timestamp must be a valid ISO-8601 date string"]
  }
}
```

**cURL example**:  
```bash
curl -X POST "http://localhost:5000/suggestTask"   -H "Content-Type: application/json"   -d '{
    "utterance":"reset password",
    "userId":"12345",
    "sessionId":"abcde-67890",
    "timestamp":"2025-08-21T12:00:00Z"
  }'
```


## Tests

This repository includes **unit tests** and **integration tests**.

### How to Run All Tests
From the repository root:
```bash
dotnet test
```

### What the Tests Cover
- **Unit tests (`MatchTaskTests.cs`)**
  - Validate the regex-based matching logic.
  - Examples:
    - `"reset password"` → `ResetPasswordTask`
    - `"forgot the password"` → `ResetPasswordTask`
    - `"check order status"` / `"track my order"` → `CheckOrderStatusTask`
    - Non-matching utterances → `NoTaskFound`

- **Integration tests (`SuggestTaskIntegrationTests.cs`)**
  - Spin up the API in-memory and send HTTP **POST** `/suggestTask`.
  - Assert on HTTP status codes and response payloads.
  - Scenarios:
    - Valid request returns **200 OK** with the matched `task`.
    - Unmatched utterance returns **200 OK** with `task = "NoTaskFound"`.
    - Missing/invalid fields return **400 Bad Request** with validation errors.

### Running a Specific Test Project
To run only the tests project:
```bash
dotnet test ./SuggestTaskService.Tests/SuggestTaskService.Tests.csproj
```
