# NICE Home Assignment – SuggestTaskService

## Overview
A minimal **ASP.NET Core (.NET 8)** Web API that exposes a single endpoint:
- **POST** `/suggestTask` – receives a user utterance and returns a **suggested task** based on case‑insensitive keyword matching.

The project demonstrates:
- API design and routing
- Input validation with **FluentValidation**
- **Console logging** for requests and responses
- Simple **business logic** for utterance→task mapping
- **Unit** and **Integration** tests (xUnit + WebApplicationFactory)

---

## Tech Stack
- **Runtime**: .NET 8
- **Web**: ASP.NET Core Minimal Hosting (Controllers)
- **Validation**: FluentValidation.AspNetCore
- **Docs**: Swagger / OpenAPI (enabled in *Development*)
- **Tests**: xUnit, Microsoft.AspNetCore.Mvc.Testing

NuGet packages (main project):
- `FluentValidation.AspNetCore` (validation)
- `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.OpenApi` (Swagger)

NuGet packages (test project):
- `xunit`, `xunit.runner.visualstudio`
- `Microsoft.AspNetCore.Mvc.Testing`
- `System.Net.Http.Json`
- `coverlet.collector` (coverage)

---

## Repository Layout (exact names)
```
home-assignment-nice/
├─ HomeAssignmentNice.sln
├─ README.md
├─ SuggestTaskService/                 # ASP.NET Core Web API
│  ├─ Program.cs                      # App setup, controllers, Swagger
│  ├─ SuggestTaskService.csproj
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  ├─ SuggestTaskService.http         # Local HTTP scratch file (template)
│  ├─ controllers/
│  │  └─ SuggestTaskController.cs     # Endpoint + matching logic + logs
│  ├─ models/
│  │  └─ SuggestTaskRequest.cs        # DTO for the incoming request
│  └─ validators/
│     └─ SuggestTaskRequestValidator.cs   # FluentValidation rules
│
└─ SuggestTaskService.Tests/           # Test project
   ├─ SuggestTaskService.Tests.csproj
   ├─ MatchTaskTests.cs                # Unit tests for matching logic
   └─ SuggestTaskIntegrationTests.cs   # In‑memory integration tests
```

> Notes:
> - The **matching logic** is implemented in `controllers/SuggestTaskController.cs` as a static regex table (extended synonyms & order‑agnostic patterns).  
> - Validation is performed via **FluentValidation** and the `[ApiController]` automatic 400 on invalid `ModelState`.  
> - Logging is done with `Console.WriteLine` (INFO/WARN style).

---

## API Contract

### Request (JSON)
```json
{
  "utterance": "I need help resetting my password",
  "userId": "12345",
  "sessionId": "abcde-67890",
  "timestamp": "2025-08-21T12:00:00Z"
}
```

- `timestamp` must be **ISO‑8601**; validation is enforced by `SuggestTaskRequestValidator`.

### Response (200 OK)
```json
{
  "task": "ResetPasswordTask",
  "timestamp": "2025-08-21T12:00:01Z"
}
```

### Response (400 Bad Request)
Returned automatically when a required field is missing or invalid (e.g., empty `utterance`, bad `timestamp`).

---

## Matching Rules (as required by the assignment)
- **reset password** → `ResetPasswordTask`
- **forgot password** → `ResetPasswordTask`
- **check order** → `CheckOrderStatusTask`
- **track order** → `CheckOrderStatusTask`
- Otherwise → `"NoTaskFound"`

**Implementation detail**: the controller uses **compiled, case‑insensitive regex patterns** to cover natural variations (e.g., “can’t remember password”, “password reset please”, extra spaces, punctuation).

---

## Getting Started

### Prerequisites
- **.NET 8 SDK** installed

### Build and Run
From the repository root:
```bash
dotnet restore
dotnet build
dotnet run --project SuggestTaskService
```
The console will print the listening URL (e.g., `http://localhost:52xx`).

### Swagger (Dev only)
When `ASPNETCORE_ENVIRONMENT=Development`, navigate to:
```
http://localhost:<PORT>/swagger
```
to explore and test the endpoint.

### cURL example
```bash
curl -X POST "http://localhost:<PORT>/suggestTask"   -H "Content-Type: application/json"   -d '{
    "utterance":"please reset password",
    "userId":"u1",
    "sessionId":"s1",
    "timestamp":"2025-08-21T12:00:00Z"
  }'
```

---

## Tests
Run all tests from the repository root:
```bash
dotnet test
```
Includes:
- **Unit** tests for `MatchTask` coverage (positive/negative cases).
- **Integration** tests that boot the API in‑memory and verify:
  - `POST /suggestTask` → 200 with a matched task
  - `POST /suggestTask` with missing fields → **400 Bad Request**
  - Unmatched utterances → `"NoTaskFound"`

---

## Design Notes
- **Simplicity over layering**: For the scope of this assignment, the matching logic lives in the controller. In production, move it into a dedicated **service** with DI and add a richer logging/observability stack.
- **Validation**: Implemented with FluentValidation; integrates with the `[ApiController]` filter to surface **400** automatically.

---


