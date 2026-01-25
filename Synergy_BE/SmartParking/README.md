# SmartParking Backend API

Modern Smart Parking Management System built with ASP.NET Core 8.0

## 🏗️ Architecture

This project follows Clean Architecture with 3-layer separation:

- **Domain**: Entities, Enums, Constants
- **Application**: Business Logic, Services, DTOs
- **Infrastructure**: Data Access, External Services
- **API**: Controllers, Endpoints

## 🚀 Quick Start

```bash
# 1. Restore dependencies
dotnet restore

# 2. Update connection string in appsettings.json

# 3. Run database migration (see Database/README.md)

# 4. Run application
cd src/SmartParking.API
dotnet run
```

## 📁 Project Structure

```
SmartParking/
├── SmartParking.sln
├── README.md
├── src/
│   ├── SmartParking.Domain/
│   ├── SmartParking.Application/
│   ├── SmartParking.Infrastructure/
│   └── SmartParking.API/
├── Database/
│   ├── Migration_*.sql
│   └── SeedData.sql
└── docs/
    ├── PROJECT_READY.md
    └── DEPLOYMENT_GUIDE.md
```

## 📚 Documentation

- [Project Status](docs/PROJECT_READY.md)
- [Deployment Guide](docs/DEPLOYMENT_GUIDE.md)
- [Database Setup](Database/README.md)

## 🔑 Features

- ✅ JWT Authentication + Refresh Tokens
- ✅ Google OAuth Integration
- ✅ Vehicle Management
- ✅ Parking Lot Management
- ✅ Booking System with Occupancy Tracking
- ✅ VNPay Payment Integration
- ✅ Role-based Authorization (User, Owner, Admin)
- ✅ Standardized API Responses (`ApiResponse<T>`)
- ✅ Exception Handling Middleware
- ✅ Input Validation

## 🛠️ Tech Stack

- **.NET 8.0** - Web API framework
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Database with Geography support
- **JWT Bearer** - Authentication
- **BCrypt** - Password hashing
- **VNPay** - Payment gateway

## 📡 API Endpoints

### Authentication (`/api/auth`)
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/google` - Login with Google OAuth
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout and revoke token

### Vehicles (`/api/vehicles`)
- `GET /api/vehicles/my-vehicles` - Get my vehicles
- `GET /api/vehicles/{id}` - Get vehicle by ID
- `POST /api/vehicles` - Add new vehicle
- `PUT /api/vehicles/{id}` - Update vehicle
- `DELETE /api/vehicles/{id}` - Delete vehicle

### Parking Lots (`/api/parking-lots`)
- `GET /api/parking-lots` - List all parking lots
- `GET /api/parking-lots/{id}` - Get parking lot by ID
- `GET /api/parking-lots/my-parking-lots` - My parking lots (Owner)
- `POST /api/parking-lots` - Create parking lot (Owner/Admin)
- `PUT /api/parking-lots/{id}` - Update parking lot (Owner/Admin)
- `DELETE /api/parking-lots/{id}` - Delete parking lot (Owner/Admin)

### Bookings (`/api/bookings`)
- `GET /api/bookings/my-bookings` - Get my bookings
- `GET /api/bookings/{id}` - Get booking by ID
- `POST /api/bookings` - Create booking
- `PUT /api/bookings/{id}` - Update booking
- `POST /api/bookings/{id}/cancel` - Cancel booking

### Payments (`/api/payments`)
- `POST /api/payments/create` - Create VNPay payment URL
- `GET /api/payments/vnpay-callback` - VNPay callback handler

## 🧪 Testing

### Using Swagger UI
1. Navigate to `https://localhost:7000/swagger`
2. Click "Authorize" button
3. Enter: `Bearer YOUR_ACCESS_TOKEN`
4. Test endpoints interactively

### Testing Flow
1. Register a new user → Get accessToken
2. Add vehicle → Create a vehicle
3. List parking lots → Browse available lots
4. Create booking → Book a parking slot
5. Create payment → Generate VNPay URL
6. Check bookings → View booking history

## 🔒 Security

- ✅ Password hashing with BCrypt
- ✅ JWT with configurable expiration
- ✅ Refresh token rotation
- ✅ Token revocation on logout
- ✅ Role-based access control
- ✅ Input validation
- ✅ SQL injection protection (EF Core)

## 📊 API Response Format

All endpoints return standardized JSON:

**Success (200/201):**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": null,
  "timestamp": "2026-01-25T12:30:00Z"
}
```

**Error (400/401/404/500):**
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": ["Error detail 1", "Error detail 2"],
  "timestamp": "2026-01-25T12:30:00Z"
}
```

## 🚦 Next Steps

### For Development
- Update `appsettings.json` with your database connection string
- Configure CORS for your frontend/mobile app
- Set up logging (Serilog recommended)
- Add integration tests

### For Production
- Use environment variables for secrets
- Enable HTTPS only
- Set up monitoring (Application Insights)
- Configure rate limiting
- Add caching (Redis)

## 📝 License

This project is for educational purposes.

---

**Status**: ✅ Production Ready  
**Version**: 1.0  
**Last Updated**: January 25, 2026
