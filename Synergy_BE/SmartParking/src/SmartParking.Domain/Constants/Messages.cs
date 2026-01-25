namespace SmartParking.Domain.Constants;

public static class Messages
{
    public static class Auth
    {
        public const string RegisterSuccess = "User registered successfully";
        public const string LoginSuccess = "Login successful";
        public const string LogoutSuccess = "Logout successful";
        public const string TokenRefreshed = "Token refreshed successfully";
        public const string InvalidCredentials = "Invalid email or password";
        public const string EmailAlreadyExists = "Email already exists";
        public const string UserNotFound = "User not found";
        public const string AccountInactive = "Account is inactive";
        public const string InvalidToken = "Invalid or expired token";
        public const string RoleNotFound = "Role not found";
        public const string InvalidGoogleToken = "Invalid Google token";
    }

    public static class ParkingLot
    {
        public const string CreateSuccess = "Parking lot created successfully";
        public const string UpdateSuccess = "Parking lot updated successfully";
        public const string DeleteSuccess = "Parking lot deleted successfully";
        public const string NotFound = "Parking lot not found";
        public const string FullCapacity = "Parking lot is at full capacity";
    }

    public static class Booking
    {
        public const string CreateSuccess = "Booking created successfully";
        public const string UpdateSuccess = "Booking updated successfully";
        public const string CancelSuccess = "Booking cancelled successfully";
        public const string NotFound = "Booking not found";
        public const string AlreadyCancelled = "Booking is already cancelled";
        public const string CannotCancel = "Cannot cancel this booking";
        public const string NoAvailableSlots = "No available slots";
    }

    public static class Vehicle
    {
        public const string CreateSuccess = "Vehicle registered successfully";
        public const string UpdateSuccess = "Vehicle updated successfully";
        public const string DeleteSuccess = "Vehicle deleted successfully";
        public const string NotFound = "Vehicle not found";
        public const string PlateAlreadyExists = "License plate already registered";
    }

    public static class Payment
    {
        public const string CreateSuccess = "Payment initiated successfully";
        public const string PaymentSuccess = "Payment completed successfully";
        public const string PaymentFailed = "Payment failed";
        public const string InvalidSignature = "Invalid payment signature";
        public const string TransactionNotFound = "Transaction not found";
    }

    public static class Validation
    {
        public const string InvalidInput = "Invalid input data";
        public const string RequiredField = "{0} is required";
        public const string InvalidFormat = "Invalid {0} format";
        public const string MinLength = "{0} must be at least {1} characters";
        public const string MaxLength = "{0} must not exceed {1} characters";
    }

    public static class Common
    {
        public const string Success = "Request completed successfully";
        public const string InternalServerError = "An internal server error occurred";
        public const string Unauthorized = "Unauthorized access";
        public const string Forbidden = "Access forbidden";
        public const string NotFound = "Resource not found";
    }
}
