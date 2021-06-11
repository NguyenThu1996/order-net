using System.ComponentModel;

namespace OPS.Utility
{
    public enum GeneralErrors
    {
        Success,
        Failed,
        ModelStateError,
        IdentityError,
        RequiresTwoFactor,
        SessionExpired
    }

    public enum Role
    {
        [Description("User")]
        User,
        [Description("Admin")]
        Admin,
        [Description("Survey")]
        Survey
    }
}
