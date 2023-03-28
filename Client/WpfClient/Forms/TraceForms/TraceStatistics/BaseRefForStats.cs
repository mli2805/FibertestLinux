using System;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class BaseRefModel
    {
        public BaseRefType BaseRefType { get; set; }
        public string BaseRefTypeString => BaseRefType.GetLocalizedString();
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
        public Guid BaseRefId { get; set; }

        public int SorFileId { get; set; }

    }

    public class BaseRefModelFactory
    {
        public BaseRefModel Create(BaseRef baseRef)
        {
            var result = new BaseRefModel()
            {
                BaseRefType = baseRef.BaseRefType,
                AssignedAt = baseRef.SaveTimestamp,
                AssignedBy = baseRef.UserName,
                BaseRefId = baseRef.Id,

                SorFileId = baseRef.SorFileId,
            };
            return result;
        }
    }
}