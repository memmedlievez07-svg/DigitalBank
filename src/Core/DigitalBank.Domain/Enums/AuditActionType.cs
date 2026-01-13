namespace DigitalBank.Domain.Enums
{
    public enum AuditActionType
    {
        Register = 1,
        Login = 2,
        Refresh = 3,
        Logout = 4,

        Transfer = 10,
        TopUp = 11,
        Withdraw = 12,
        Adjustment = 13,

             // Admin user management
        UserLock = 20,
        UserUnlock = 21,
        UserSetRole = 22
    }
}
