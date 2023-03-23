using System;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public class CurrentUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public Role Role { get; set; }
        public Guid ZoneId { get; set; }
        public string ZoneTitle { get; set; } = null!;
        public string ConnectionId { get; set; } = null!;

        public bool IsDefaultZoneUser => ZoneId == Guid.Empty;

        public void FillIn(ClientRegisteredDto dto)
        {
            UserId = dto.UserId;
            Role = dto.Role;
            ZoneId = dto.ZoneId;
            ZoneTitle = dto.ZoneTitle ?? "";
            ConnectionId = dto.ConnectionId ?? "";
        }
    }
}