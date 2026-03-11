namespace LeaveManagement.Application.Constants;

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

    public static class LeaveTypes
    {
        public const string View = "Permissions.LeaveTypes.View";
        public const string Create = "Permissions.LeaveTypes.Create";
        public const string Edit = "Permissions.LeaveTypes.Edit";
        public const string Delete = "Permissions.LeaveTypes.Delete";
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

    public static List<string> GetAll() =>
    [
        LeaveRequests.View, LeaveRequests.Create, LeaveRequests.Edit, LeaveRequests.Delete, LeaveRequests.Approve,
        LeaveTypes.View, LeaveTypes.Create, LeaveTypes.Edit, LeaveTypes.Delete,
        Roles.View, Roles.Create, Roles.Edit, Roles.Delete,
        Users.View, Users.Create, Users.Edit, Users.Delete
    ];
}
