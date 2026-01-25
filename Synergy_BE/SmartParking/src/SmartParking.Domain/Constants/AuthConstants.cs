namespace SmartParking.Domain.Constants;

public static class AuthConstants
{
    public const string LocalProvider = "Local";
    public const string GoogleProvider = "Google";
    
    public static class Roles
    {
        public const string User = "User";
        public const string Owner = "Owner";
        public const string Admin = "Admin";
    }
}
