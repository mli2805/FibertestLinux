using Fibertest.Dto;

namespace Fibertest.Graph
{
    public static class BaseRefDtoFactory
    {
        // from client - Assign base
        public static BaseRefDto CreateFromFile(string filename, BaseRefType type, string username)
        {
            if (filename == "")
                return new BaseRefDto() { Id = Guid.Empty, BaseRefType = type }; // delete old base ref

            var bytes = File.ReadAllBytes(filename);
            return new BaseRefDto()
            {
                Id = Guid.NewGuid(),
                BaseRefType = type,
                UserName = username,
                //  SaveTimestamp = DateTime.Now,  // will be set on server
                SorBytes = bytes,
                //   Duration = TimeSpan.FromSeconds((int) otdrDataKnownBlocks.FixedParameters.AveragingTime)  // will be set on server
            };
        }

        // from client - Auto base
        public static BaseRefDto CreateFromBytes(BaseRefType type, byte[] sorBytes, string username)
        {
            return new BaseRefDto()
            {
                Id = Guid.NewGuid(),
                BaseRefType = type,
                UserName = username,
                SorBytes = sorBytes,
            };
        }


        // on server
        public static BaseRefDto CreateFromBaseRef(this BaseRef baseRef, byte[] sorBytes)
        {
            return new BaseRefDto()
            {
                Id = baseRef.Id,
                BaseRefType = baseRef.BaseRefType,
                Duration = baseRef.Duration,
                SaveTimestamp = baseRef.SaveTimestamp,
                UserName = baseRef.UserName,
                SorFileId = baseRef.SorFileId,

                SorBytes = sorBytes,
            };
        }

    }
}