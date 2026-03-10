namespace LeaveManagement.Infrastructure.Identity;

public static class Permissions
{
    public static class LeaveRequests
    {
        public const string View = "Permissions.LeaveRequests.View";
        public const string Create = "Permissions.LeaveRequests.Create";
        public const string Edit = "Permissions.LeaveRequests.Edit";
        public const string Delete = "Permissions.LeaveRequests.Delete";
        public const string Approve = "Permissions.LeaveRequests.Approve";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
    }

    public static List<string> GetAll()
    {
        return new List<string>
        {
            LeaveRequests.View, LeaveRequests.Create, LeaveRequests.Edit, LeaveRequests.Delete, LeaveRequests.Approve,
            Roles.View, Roles.Create, Roles.Edit, Roles.Delete,
            Users.View, Users.Create, Users.Edit, Users.Delete
        };
    }
}
