namespace SmartParking.Domain.Constants;

public static class Messages
{
    public static class Auth
    {
        public const string RegisterSuccess = "Đăng ký tài khoản thành công";
        public const string LoginSuccess = "Đăng nhập thành công";
        public const string LogoutSuccess = "Đăng xuất thành công";
        public const string TokenRefreshed = "Làm mới token thành công";
        public const string InvalidCredentials = "Email hoặc mật khẩu không đúng";
        public const string EmailAlreadyExists = "Email đã tồn tại";
        public const string UserNotFound = "Không tìm thấy người dùng";
        public const string AccountInactive = "Tài khoản đã bị vô hiệu hóa";
        public const string InvalidToken = "Token không hợp lệ hoặc đã hết hạn";
        public const string RoleNotFound = "Không tìm thấy vai trò";
        public const string PasswordChangedSuccess = "Đổi mật khẩu thành công";
        public const string OldPasswordIncorrect = "Mật khẩu cũ không đúng";
        public const string NewPasswordSameAsOld = "Mật khẩu mới phải khác mật khẩu cũ";

        // OTP Verification Messages
        public const string OtpSentSuccess = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
        public const string OtpVerifiedSuccess = "Xác thực email thành công. Tài khoản của bạn đã được tạo.";
        public const string OtpInvalid = "Mã OTP không đúng";
        public const string OtpExpired = "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.";
        public const string OtpAlreadyUsed = "Mã OTP này đã được sử dụng";
        public const string OtpNotFound = "Không tìm thấy mã OTP cho email này";
        public const string OtpResentSuccess = "Mã OTP mới đã được gửi đến email của bạn";
        public const string EmailNotVerified = "Vui lòng xác thực email trước khi đăng nhập";
        public const string TooManyOtpRequests = "Quá nhiều yêu cầu OTP. Vui lòng thử lại sau 60 giây.";
        public const string PendingRegistration = "Đăng ký đang chờ xử lý. Vui lòng xác thực email trước.";

        // Password Reset Messages
        public const string PasswordResetOtpSent = "Mã OTP đặt lại mật khẩu đã được gửi đến email của bạn";
        public const string PasswordResetSuccess = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới.";
        public const string InvalidPasswordResetOtp = "Mã OTP không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu đặt lại mật khẩu mới.";
        public const string PasswordResetUserNotFound = "Không tìm thấy tài khoản với email này";
    }

    public static class ParkingLot
    {
        public const string CreateSuccess = "Tạo bãi xe thành công";
        public const string UpdateSuccess = "Cập nhật bãi xe thành công";
        public const string DeleteSuccess = "Xóa bãi xe thành công";
        public const string NotFound = "Không tìm thấy bãi xe";
        public const string FullCapacity = "Bãi xe đã hết chỗ";
    }

    public static class Booking
    {
        public const string CreateSuccess = "Đặt chỗ thành công";
        public const string UpdateSuccess = "Cập nhật đặt chỗ thành công";
        public const string CancelSuccess = "Hủy đặt chỗ thành công";
        public const string CheckInSuccess = "Vào bãi thành công";
        public const string CheckOutSuccess = "Ra bãi thành công";
        public const string NotFound = "Không tìm thấy đặt chỗ";
        public const string AlreadyCancelled = "Đặt chỗ đã bị hủy";
        public const string CannotCancel = "Không thể hủy đặt chỗ này";
        public const string NoAvailableSlots = "Không còn chỗ trống";
        public const string InvalidStatusForCheckIn = "Trạng thái đặt chỗ không hợp lệ để vào bãi";
        public const string InvalidStatusForCheckOut = "Trạng thái đặt chỗ không hợp lệ để ra bãi";
    }

    public static class Vehicle
    {
        public const string CreateSuccess = "Đăng ký xe thành công";
        public const string UpdateSuccess = "Cập nhật xe thành công";
        public const string DeleteSuccess = "Xóa xe thành công";
        public const string NotFound = "Không tìm thấy xe";
        public const string PlateAlreadyExists = "Biển số xe đã được đăng ký";
    }

    public static class Payment
    {
        public const string CreateSuccess = "Khởi tạo thanh toán thành công";
        public const string PaymentSuccess = "Thanh toán thành công";
        public const string PaymentFailed = "Thanh toán thất bại";
        public const string InvalidSignature = "Chữ ký thanh toán không hợp lệ";
        public const string TransactionNotFound = "Không tìm thấy giao dịch";
        public const string PaymentStatusRetrieved = "Lấy trạng thái thanh toán thành công";
    }

    public static class Validation
    {
        public const string InvalidInput = "Dữ liệu đầu vào không hợp lệ";
        public const string RequiredField = "{0} là bắt buộc";
        public const string InvalidFormat = "Định dạng {0} không hợp lệ";
        public const string MinLength = "{0} phải có ít nhất {1} ký tự";
        public const string MaxLength = "{0} không được quá {1} ký tự";
    }

    public static class ParkingLocationMessages
    {
        public const string CreateSuccess = "Tạo vị trí đỗ xe thành công";
        public const string NotFound = "Không tìm thấy vị trí đỗ xe";
        public const string InvalidCoordinates = "Tọa độ vĩ độ hoặc kinh độ không hợp lệ";
        public const string InvalidRadius = "Bán kính tìm kiếm phải lớn hơn 0";
        public const string NearbyRetrieved = "Lấy danh sách bãi xe gần đây thành công";
        public const string AllRetrieved = "Lấy tất cả vị trí đỗ xe thành công";
    }

    public static class Common
    {
        public const string Success = "Yêu cầu đã được xử lý thành công";
        public const string InternalServerError = "Đã xảy ra lỗi máy chủ";
        public const string Unauthorized = "Không có quyền truy cập";
        public const string Forbidden = "Bị từ chối truy cập";
        public const string NotFound = "Không tìm thấy tài nguyên";
    }
}
