# 🔐 SmartParking API - Security Quick Reference

Quick reference guide for developers working with the SmartParking API security features.

---

## 🚀 Quick Start

### 1. Controller Authorization Pattern

```csharp
using Microsoft.AspNetCore.Authorization;
using SmartParking.API.Authorization.Policies;

[Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
[HttpGet("{id:guid}")]
public async Task<ActionResult<ApiResponse<MyDto>>> GetById(Guid id, CancellationToken ct)
{
    var userId = GetUserIdFromToken();   // Extract from JWT
    var isAdmin = IsAdmin();              // Check Admin role
    var result = await _service.GetByIdAsync(id, userId, isAdmin, ct);
    return Ok(ApiResponse<MyDto>.SuccessResponse(result));
}

// Helper methods (copy to all controllers)
private Guid GetUserIdFromToken()
{
    var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    return Guid.Parse(userIdClaim!);
}

private bool IsAdmin()
{
    return User.IsInRole(AuthConstants.Roles.Admin);
}
```

### 2. Service Ownership Validation Pattern

```csharp
using SmartParking.Application.Common.Helpers;

public async Task<MyDto> GetByIdAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct)
{
    var resource = await _repository.GetByIdAsync(id, ct);
    if (resource == null)
    {
        throw new NotFoundException("Resource not found");
    }

    // SECURITY: Validate ownership or Admin access
    SecurityHelper.ValidateOwnership(resource.OwnerId, userId, isAdmin);

    return MapToDto(resource);
}
```

---

## 📋 Available Authorization Policies

| Policy | Roles | Use Case |
|--------|-------|----------|
| `AuthorizationPolicies.UserOnly` | User | User-exclusive endpoints |
| `AuthorizationPolicies.OwnerOnly` | Owner | Owner-exclusive endpoints |
| `AuthorizationPolicies.AdminOnly` | Admin | Admin-exclusive endpoints |
| `AuthorizationPolicies.OwnerOrAdmin` | Owner, Admin | Parking lot management |
| `AuthorizationPolicies.UserOrAdmin` | User, Admin | Bookings, vehicles, payments |

### Usage Examples

```csharp
// User (Driver) only
[Authorize(Policy = AuthorizationPolicies.UserOnly)]
[HttpGet("my-vehicles")]
public async Task<ActionResult> GetMyVehicles() { }

// Owner or Admin
[Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
[HttpPost("parking-lots")]
public async Task<ActionResult> CreateParkingLot() { }

// Public endpoint
[AllowAnonymous]
[HttpGet("parking-lots")]
public async Task<ActionResult> GetAllParkingLots() { }
```

---

## 🔒 Security Checklist for New Endpoints

- [ ] Controller has `[Authorize]` or `[AllowAnonymous]`
- [ ] Correct policy specified (UserOrAdmin, OwnerOrAdmin, etc.)
- [ ] UserId extracted from JWT (never from request body)
- [ ] isAdmin parameter passed to service method
- [ ] Service validates ownership using `SecurityHelper`
- [ ] Correct exception thrown (`ForbiddenException` for ownership)
- [ ] Security comment added for critical sections

---

## ⚠️ Common Security Mistakes

### ❌ DON'T: Trust client-sent userId

```csharp
// BAD - client can fake userId
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateRequest request)
{
    await _service.CreateAsync(request.UserId, request.Data);  // ❌
}
```

### ✅ DO: Extract userId from JWT

```csharp
// GOOD - userId from validated JWT
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateRequest request)
{
    var userId = GetUserIdFromToken();  // ✅ From JWT
    await _service.CreateAsync(userId, request.Data);
}
```

### ❌ DON'T: Use generic [Authorize]

```csharp
[Authorize]  // ❌ Any authenticated user can access
[HttpPost("parking-lots")]
public async Task<ActionResult> CreateParkingLot() { }
```

### ✅ DO: Use specific policy

```csharp
[Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]  // ✅ Explicit
[HttpPost("parking-lots")]
public async Task<ActionResult> CreateParkingLot() { }
```

### ❌ DON'T: Throw UnauthorizedException for ownership

```csharp
if (resource.OwnerId != userId)
{
    throw new UnauthorizedException();  // ❌ Wrong exception (401)
}
```

### ✅ DO: Throw ForbiddenException for ownership

```csharp
SecurityHelper.ValidateOwnership(resource.OwnerId, userId, isAdmin);  // ✅ (403)
```

---

## 🔐 JWT Claims Reference

### Reading JWT Claims

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// Get UserId
var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
var userId = Guid.Parse(userIdClaim!);

// Get Email
var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);

// Check Role
var isAdmin = User.IsInRole(AuthConstants.Roles.Admin);
var isOwner = User.IsInRole(AuthConstants.Roles.Owner);
var isUser = User.IsInRole(AuthConstants.Roles.User);
```

### JWT Token Structure

```json
{
  "sub": "user-guid",           // UserId
  "email": "user@example.com",  // Email
  "role": "User",               // Role (User/Owner/Admin)
  "jti": "unique-token-id",     // Token ID
  "exp": 1706177400,            // Expires (15 minutes)
  "iss": "SmartParkingAPI",     // Issuer
  "aud": "SmartParkingClient"   // Audience
}
```

---

## 🛡️ SecurityHelper Methods

### ValidateOwnership

```csharp
// Throws ForbiddenException if user doesn't own resource and is not Admin
SecurityHelper.ValidateOwnership(
    resourceOwnerId: vehicle.UserId,
    currentUserId: userId,
    isAdmin: isAdmin
);

// With custom message
SecurityHelper.ValidateOwnership(
    resourceOwnerId: parkingLot.OwnerId,
    currentUserId: userId,
    isAdmin: isAdmin,
    errorMessage: "You can only modify your own parking lots"
);
```

### CanAccess (Boolean Check)

```csharp
// Returns bool instead of throwing
if (SecurityHelper.CanAccess(resource.OwnerId, userId, isAdmin))
{
    // User has access
}
else
{
    // User does not have access
}
```

---

## 🔄 VNPay Callback Security

### Controller (Public)

```csharp
[AllowAnonymous]  // VNPay cannot send JWT
[HttpGet("vnpay-callback")]
public async Task<IActionResult> VnPayCallback(
    [FromQuery] VnPayCallbackDto callback,
    CancellationToken ct)
{
    var isSuccess = await _vnPayService.ProcessCallbackAsync(callback, ct);
    return isSuccess 
        ? Redirect("/payment-success") 
        : Redirect("/payment-failed");
}
```

### Service (Secure Processing)

VNPay callback automatically:
1. ✅ Validates `vnp_SecureHash` (prevents tampering)
2. ✅ Checks idempotency (prevents duplicate processing)
3. ✅ Logs all attempts (audit trail)
4. ✅ Updates payment status atomically

**No additional validation needed in controller.**

---

## 📊 HTTP Status Codes

| Code | Exception | Meaning |
|------|-----------|---------|
| 200 | - | Success |
| 201 | - | Created |
| 400 | `BadRequestException` | Invalid input |
| 401 | `UnauthorizedException` | Missing/invalid JWT |
| 403 | `ForbiddenException` | Valid JWT, but insufficient permissions |
| 404 | `NotFoundException` | Resource not found |
| 500 | `Exception` | Server error |

### When to Use 401 vs 403

```csharp
// 401 - No token or invalid token
// Handled automatically by JWT middleware
[Authorize]
[HttpGet("protected")]
public async Task<ActionResult> Protected() { }
// No JWT → 401 Unauthorized

// 403 - Valid token, but user doesn't own resource
[Authorize]
[HttpGet("vehicles/{id}")]
public async Task<ActionResult> GetVehicle(Guid id)
{
    var userId = GetUserIdFromToken();  // JWT is valid
    var vehicle = await _service.GetByIdAsync(id, userId, false);
    // If vehicle belongs to another user → ForbiddenException → 403 Forbidden
}
```

---

## 🧪 Testing Security

### Test 1: Role Enforcement

```bash
# Register as User
POST /api/auth/register
{ "email": "user@test.com", "password": "Test@123", ... }

# Try to create parking lot (should fail)
POST /api/parking-lots
Authorization: Bearer USER_TOKEN
# Expected: 403 Forbidden
```

### Test 2: Ownership Validation

```bash
# User A creates vehicle
POST /api/vehicles
Authorization: Bearer USER_A_TOKEN
{ "licensePlate": "29A-12345", ... }
# Response: { "vehicleId": "abc-123" }

# User B tries to access User A's vehicle
GET /api/vehicles/abc-123
Authorization: Bearer USER_B_TOKEN
# Expected: 403 Forbidden
```

### Test 3: Admin Bypass

```bash
# Admin can access any resource
GET /api/vehicles/abc-123
Authorization: Bearer ADMIN_TOKEN
# Expected: 200 OK (even if vehicle belongs to another user)
```

---

## 📝 Security Code Templates

### New Controller Template

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

[Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
[ApiController]
[Route("api/[controller]")]
public sealed class MyController : ControllerBase
{
    private readonly IMyService _service;

    public MyController(IMyService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MyDto>>> GetById(
        Guid id, 
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var result = await _service.GetByIdAsync(id, userId, isAdmin, ct);
        return Ok(ApiResponse<MyDto>.SuccessResponse(result));
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(userIdClaim!);
    }

    private bool IsAdmin()
    {
        return User.IsInRole(AuthConstants.Roles.Admin);
    }
}
```

### New Service Template

```csharp
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Helpers;
using SmartParking.Domain.Constants;

namespace SmartParking.Application.Services;

public sealed class MyService : IMyService
{
    private readonly IMyRepository _repository;

    public MyService(IMyRepository repository)
    {
        _repository = repository;
    }

    public async Task<MyDto> GetByIdAsync(
        Guid id, 
        Guid userId, 
        bool isAdmin, 
        CancellationToken ct = default)
    {
        var resource = await _repository.GetByIdAsync(id, ct);
        if (resource == null)
        {
            throw new NotFoundException(Messages.Common.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(resource.OwnerId, userId, isAdmin);

        return MapToDto(resource);
    }
}
```

---

## 🎯 Security Decision Tree

```
New Endpoint
│
├─ Public access needed?
│  ├─ YES → [AllowAnonymous]
│  └─ NO  → [Authorize(Policy = ...)]
│
├─ Which roles can access?
│  ├─ User only       → AuthorizationPolicies.UserOnly
│  ├─ Owner only      → AuthorizationPolicies.OwnerOnly
│  ├─ Admin only      → AuthorizationPolicies.AdminOnly
│  ├─ User OR Admin   → AuthorizationPolicies.UserOrAdmin
│  └─ Owner OR Admin  → AuthorizationPolicies.OwnerOrAdmin
│
├─ Resource ownership check needed?
│  ├─ YES → Pass isAdmin to service
│  │       Service calls SecurityHelper.ValidateOwnership()
│  └─ NO  → No ownership check (e.g., create new resource)
│
└─ Error handling
   ├─ Not found      → throw NotFoundException (404)
   ├─ Invalid input  → throw BadRequestException (400)
   └─ No permission  → throw ForbiddenException (403)
```

---

## 🔗 Related Documentation

- [Full Security Audit Report](./SECURITY_AUDIT_REPORT.md)
- [API Features v1.2](./API_FEATURES_v1.2.md)
- [Migration Guide](../Database/README_Migration_v1.2.md)

---

**Last Updated**: January 25, 2026  
**Version**: v1.2 (Security Hardened)
