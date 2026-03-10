namespace LeaveManagement.Infrastructure.Identity;

public static class Roles
{
    // Platform Level Roles
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";

    // No hardcoded Employee role.
    // Tenant Admins will create their own roles and set a default for new users.
}
