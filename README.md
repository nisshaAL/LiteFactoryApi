# LiteFactoryApi

LiteFactoryApi is the first local-development backend for LiteFactory accounts. It is for LiteFactory website, launcher, and later LiteFactory service authorization. It does not implement Minecraft, Mojang, or Microsoft authentication.

## Requirements

- .NET 10 SDK
- SQLite is used through Entity Framework Core

## Configure JWT Signing Key

Do not put real secrets in source code.

For local development, use either user secrets:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:SigningKey" "replace-with-at-least-32-random-characters"
```

Or an environment variable:

```powershell
$env:LITEFACTORY_JWT_SIGNING_KEY = "replace-with-at-least-32-random-characters"
```

The API fails on startup if no signing key with at least 32 bytes is configured.

## Database

The local SQLite database is:

```text
data/litefactory.db
```

Database files under `data/` are ignored by Git.

Create or update the local database with:

```powershell
dotnet ef database update
```

The API also applies pending migrations on startup. It does not delete or recreate the database.

## Run

```powershell
dotnet run --urls "http://localhost:5088"
```

Local API URL:

```text
http://localhost:5088
```

Swagger UI in Development:

```text
http://localhost:5088/swagger
```

## Example Requests

Register:

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "tester@example.com",
  "nickname": "Tester_01",
  "password": "example-password"
}
```

Login:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "tester@example.com",
  "password": "example-password"
}
```

Current account:

```http
GET /api/account/me
Authorization: Bearer <access-token>
```
