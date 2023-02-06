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
            binaryFormatter.Serialize(stream, model);
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
            var model2 = (Model)binaryFormatter.Deserialize(stream);
            logger.Log(LogLevel.Information, @"Model deserialized successfully!");
            model.CopyFrom(model2);

            return model2.Rtus.Count == model.Rtus.Count;
        }
        catch (Exception e)
        {
            logger.Log(LogLevel.Error, @"Model deserialization: " + e.Message);
            return false;
        }
    }

   
}