using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Extensions.Logging;

namespace Fibertest.Graph;

public static class ModelSerializationExt
{
    public static async Task<byte[]?> Serialize(this Model model, ILogger logger)
    {
        try
        {
            using MemoryStream stream = new MemoryStream();

            await Task.Delay(1);
            var binaryFormatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
            binaryFormatter.Serialize(stream, model);
#pragma warning restore SYSLIB0011
            var buf = stream.ToArray();
            logger.Log(LogLevel.Information, $@"Model serialization: data size = {buf.Length:0,0.#}");
            return buf;
        }
        catch (Exception e)
        {
            logger.Log(LogLevel.Error, @"Model serialization: " + e.Message);
            return null;
        }
    }

    public static async Task<bool> Deserialize(this Model model, ILogger logger, byte[] buffer)
    {
        try
        {
            using var stream = new MemoryStream(buffer);

            await Task.Delay(1);
            var binaryFormatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
            var model2 = (Model)binaryFormatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
            logger.Log(LogLevel.Information, @"Model deserialized successfully!");
            model2.AdjustModelDeserializedFromSnapshotMadeByOldVersion();
            model.CopyFrom(model2);

            return model2.Rtus.Count == model.Rtus.Count;
        }
        catch (Exception e)
        {
            logger.Log(LogLevel.Error, @"Model deserialization: " + e.Message);
            return false;
        }
    }

    // if snapshot was made before v926
    private static void AdjustModelDeserializedFromSnapshotMadeByOldVersion(this Model model)
    {
        model.Licenses = new List<License>()
        {
            new License()
            {
                LicenseId = new Guid(),
                ClientStationCount = new LicenseParameter()
                {
                    Value = 1,
                    ValidUntil = DateTime.MaxValue,
                },
                RtuCount = new LicenseParameter(),
                WebClientCount = new LicenseParameter(),
                SuperClientStationCount = new LicenseParameter(),
                CreationDate = DateTime.Today,
                LoadingDate = DateTime.Today,
            }
        };


    }
}