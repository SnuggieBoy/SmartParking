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
        public const string PasswordChangedSuccess = "Password changed successfully";
        public const string OldPasswordIncorrect = "Old password is incorrect";
        public const string NewPasswordSameAsOld = "New password must be different from old password";
        
        // OTP Verification Messages
        public const string OtpSentSuccess = "OTP has been sent to your email. Please check your inbox.";
        public const string OtpVerifiedSuccess = "Email verified successfully. Your account has been created.";
        public const string OtpInvalid = "Invalid OTP code";
        public const string OtpExpired = "OTP code has expired. Please request a new one.";
        public const string OtpAlreadyUsed = "This OTP has already been used";
        public const string OtpNotFound = "No OTP found for this email";
        public const string OtpResentSuccess = "A new OTP has been sent to your email";
        public const string EmailNotVerified = "Please verify your email before logging in";
        public const string TooManyOtpRequests = "Too many OTP requests. Please try again in 60 seconds";
        public const string PendingRegistration = "Registration pending. Please verify your email first.";
        
        // Password Reset Messages
        public const string PasswordResetOtpSent = "Password reset OTP has been sent to your email";
        public const string PasswordResetSuccess = "Password reset successfully. You can now login with your new password.";
        public const string InvalidPasswordResetOtp = "Invalid or expired OTP. Please request a new password reset.";
        public const string PasswordResetUserNotFound = "No account found with this email address";
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
        public const string CheckInSuccess = "Checked in successfully";
        public const string CheckOutSuccess = "Checked out successfully";
        public const string NotFound = "Booking not found";
        public const string AlreadyCancelled = "Booking is already cancelled";
        public const string CannotCancel = "Cannot cancel this booking";
        public const string NoAvailableSlots = "No available slots";
        public const string InvalidStatusForCheckIn = "Booking status is not valid for check-in";
        public const string InvalidStatusForCheckOut = "Booking status is not valid for check-out";
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
        public const string PaymentStatusRetrieved = "Payment status retrieved successfully";
    }

    public static class Validation
    {
        public const string InvalidInput = "Invalid input data";
        public const string RequiredField = "{0} is required";
        public const string InvalidFormat = "Invalid {0} format";
        public const string MinLength = "{0} must be at least {1} characters";
        public const string MaxLength = "{0} must not exceed {1} characters";
    }

    public static class ParkingLocationMessages
    {
        public const string CreateSuccess = "Parking location created successfully";
        public const string NotFound = "Parking location not found";
        public const string InvalidCoordinates = "Invalid latitude or longitude coordinates";
        public const string InvalidRadius = "Search radius must be greater than 0";
        public const string NearbyRetrieved = "Nearby parking locations retrieved successfully";
        public const string AllRetrieved = "All parking locations retrieved successfully";
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
