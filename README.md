# QTip – Data Classification & Tokenisation Prototype

## How to run the application


### 1. Clone the repository

```
git clone https://github.com/SandersTDavid/QTip.git
cd qtip
```
### 2. Run full stack via Docker Compose
```
docker compose up
```
This starts:

PostgreSQL (port 15432 → 5432)

API (http://localhost:5080)

Frontend (http://localhost:5173)

Open a browser at:

http://localhost:5173

## Architectural decisions

Three-layer structure with Domain, Application and Infrastructure, plus Api project.

EF Core + PostgreSQL where migrations live in Infrastructure and DbContext implements an interface in Application.

Token vault split by database:

**Submissions** stores tokenised text only.

**ClassifiedItems** stores the original email, tag, token, hash.

**SubmissionClassifications** joins the two.

Token reuse per unique email, same email always maps to the same token.

Minimal surface with only one pipeline, submit text, detect emails, store results, update stats.

HTTP Controller + MediatR Commands/Queries with clean, testable entry points.

Regex-based email detection.

Stats endpoint feeds UI persistently from database.

## Assumptions / trade-offs

No user accounts or multi-tenant concerns.

HTML content editable used for highlighting, no use of UI library.

No editing of past submissions, write-only model.

Statistic for Total PII emails submitted shows all submitted emails and not all matched, classified emails.

No text validation, we limit text up to 50k chars through backend, but front end will not limit or display char count.

No token expiry or encryption layer.
