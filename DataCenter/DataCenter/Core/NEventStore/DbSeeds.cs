using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public static class DbSeeds
    {
        public static readonly List<object> Collection = new List<object>()
        {
            new AddZone() { IsDefaultZone = true, Title = StringResources.Resources.SID_Default_Zone },
            new AddUser() { UserId = Guid.NewGuid(), Title = "developer",
                EncodedPassword = "developer".GetSha256(), Role = Role.Developer, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "root",
                EncodedPassword = "root".GetSha256(), Role = Role.Root, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "operator", 
                EncodedPassword = "operator".GetSha256(), Role = Role.Operator, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "supervisor", 
                EncodedPassword = "supervisor".GetSha256(), Role = Role.Supervisor, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "weboperator", 
                EncodedPassword = "weboperator".GetSha256(), Role = Role.WebOperator, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "websupervisor", 
                EncodedPassword = "websupervisor".GetSha256(), Role = Role.WebSupervisor, ZoneId = Guid.Empty },
            new AddUser() { UserId = Guid.NewGuid(), Title = "superclient", 
                EncodedPassword = "superclient".GetSha256(), Role = Role.SuperClient, ZoneId = Guid.Empty },
        };
    }
}