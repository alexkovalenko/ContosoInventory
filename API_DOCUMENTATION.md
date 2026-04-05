# Authentication API Documentation

## Overview
The Authentication API provides endpoints for user authentication and session management. All endpoints follow RESTful conventions and return JSON responses.

---

## Endpoints

### 1. Login
**Endpoint:** `POST /api/auth/login`

**Description:**
Authenticates a user with their email and password, establishes an authenticated session via cookies, and returns the user's information. Includes account lockout protection after multiple failed attempts and uniform response timing to prevent user enumeration attacks.

**Request Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "securePassword123"
}
```

**Request Body Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| email | string | Yes | The user's email address used for authentication |
| password | string | Yes | The user's password |

**Response Status Codes:**
| Status Code | Description |
|------------|-------------|
| 200 OK | Login successful. Returns user information and sets authentication cookie |
| 401 Unauthorized | Invalid credentials or account is locked |

**Success Response (200 OK):**
```json
{
  "email": "user@example.com",
  "displayName": "John Doe",
  "role": "Admin"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "message": "Invalid email or password."
}
```

**Alternate Error Response (401 Unauthorized - Locked Account):**
```json
{
  "message": "Account is temporarily locked. Please try again later."
}
```

**Security Notes:**
- Passwords are validated using ASP.NET Core Identity
- Accounts are locked after multiple failed login attempts
- Response time is standardized to 1 second minimum to prevent timing-based user enumeration
- Invalid credential messages are intentionally non-specific

---

### 2. Logout
**Endpoint:** `POST /api/auth/logout`

**Description:**
Signs out the currently authenticated user and clears the authentication cookie. Requires the user to be authenticated.

**Request Headers:**
```
Authorization: Bearer <cookie>
Content-Type: application/json
```

**Authentication:**
Requires user to be authenticated (uses cookie-based authentication).

**Request Body:**
None

**Response Status Codes:**
| Status Code | Description |
|------------|-------------|
| 200 OK | Logout successful |
| 401 Unauthorized | User is not authenticated |

**Success Response (200 OK):**
```json
{
  "message": "Logged out successfully."
}
```

---

### 3. Get Current User
**Endpoint:** `GET /api/auth/me`

**Description:**
Returns information about the currently authenticated user, including their email, display name, and role. Requires the user to be authenticated.

**Request Headers:**
```
Authorization: Bearer <cookie>
```

**Authentication:**
Requires user to be authenticated (uses cookie-based authentication).

**Query Parameters:**
None

**Response Status Codes:**
| Status Code | Description |
|------------|-------------|
| 200 OK | Request successful. Returns current user information |
| 401 Unauthorized | User is not authenticated |

**Success Response (200 OK):**
```json
{
  "email": "user@example.com",
  "displayName": "John Doe",
  "role": "Admin"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "HTTP 401": "Unauthorized"
}
```

---

## Data Models

### UserInfoDto
Returned by all successful authentication endpoints.

```json
{
  "email": "string",
  "displayName": "string",
  "role": "string"
}
```

| Property | Type | Description |
|----------|------|-------------|
| email | string | The user's email address |
| displayName | string | The user's display name (from claims or email as fallback) |
| role | string | The user's primary role (first role from their assigned roles) |

### LoginDto
Used as request body for the Login endpoint.

```json
{
  "email": "string",
  "password": "string"
}
```

| Property | Type | Description |
|----------|------|-------------|
| email | string | The user's email address |
| password | string | The user's password |

---

## Authentication Method

**Type:** Cookie-based Authentication

The API uses ASP.NET Core Identity with cookie-based authentication. After successful login, an authentication cookie is set in the response. This cookie must be included in subsequent requests to protected endpoints.

---

## Error Handling

All endpoints follow consistent error handling practices:

- **Validation Errors:** Returned as 400 Bad Request (for invalid LoginDto if applicable)
- **Authentication Failures:** Returned as 401 Unauthorized with descriptive messages
- **Server Errors:** Would return 500 Internal Server Error with appropriate error messages

All errors are logged using structured logging with `ILogger<AuthController>`.

---

## Usage Examples

### Example 1: Login Flow

**Request:**
```http
POST /api/auth/login HTTP/1.1
Host: api.contoso.com
Content-Type: application/json

{
  "email": "admin@contoso.com",
  "password": "MySecurePassword123!"
}
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json
Set-Cookie: .AspNetCore.Identity.Application=<cookie-value>; Path=/; HttpOnly; SameSite=Strict

{
  "email": "admin@contoso.com",
  "displayName": "Administrator",
  "role": "Admin"
}
```

### Example 2: Get Current User

**Request:**
```http
GET /api/auth/me HTTP/1.1
Host: api.contoso.com
Cookie: .AspNetCore.Identity.Application=<cookie-value>
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "email": "admin@contoso.com",
  "displayName": "Administrator",
  "role": "Admin"
}
```

### Example 3: Logout

**Request:**
```http
POST /api/auth/logout HTTP/1.1
Host: api.contoso.com
Cookie: .AspNetCore.Identity.Application=<cookie-value>
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "message": "Logged out successfully."
}
```

---

## Implementation Details

### Security Features
- **Lockout Protection:** Accounts are locked after multiple failed login attempts
- **Timing Attack Mitigation:** All login responses take a minimum of 1 second to prevent user enumeration via response time analysis
- **Password Validation:** Uses ASP.NET Core Identity's secure password hashing
- **Claims-Based Identity:** Supports user claims for display name and other user properties
- **Role-Based Access:** User roles are retrieved and included in responses

### Logging
All endpoint actions are logged:
- Successful logins: Information level
- Failed logins: Warning level
- Account lockouts: Warning level
- Logouts: Information level
- Failed user lookups: Error level (if user not found after successful password verification)

