namespace GdbWebApi.Application.Common.Security
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Teller = "Teller";
        public const string User = "User";

        // Combined role policy helpers
        public const string Staff = $"{Admin},{Manager},{Teller}";
        public const string TellerOrAdmin = $"{Admin},{Teller}";
        public const string ManagerOrAdmin = $"{Admin},{Manager}";
    }
}