namespace Fibertest.Dto;

public enum Role
{
    System = 0,
    Developer = 1,
    Root = 2,

    Operator = 3,
    WebOperator = 31,

    Supervisor = 4,
    WebSupervisor = 41,

    SuperClient = 5,

    SecurityAdmin = 98,
    NotificationReceiver = 99,
}

public static class RoleExt
{
    public static bool IsWebPermitted(this Role role)
    {
        return role == Role.Developer || role == Role.Root ||
               role == Role.WebOperator || role == Role.WebSupervisor;
    }

    public static bool IsSuperClientPermitted(this Role role)
    {
        return role == Role.Developer || role == Role.SuperClient;
    }

    public static bool IsDesktopPermitted(this Role role)
    {
        return role == Role.Developer || role == Role.Root ||
               role == Role.Operator || role == Role.Supervisor;
    }
}