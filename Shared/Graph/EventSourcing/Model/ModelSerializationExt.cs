using System.Runtime.Serialization.Formatters.Binary;
using Fibertest.Utils;
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
            logger.Info(Logs.DataCenter, $@"Model serialization: data size = {buf.Length:0,0.#}");
            return buf;
        }
        catch (Exception e)
        {
            logger.Exception(Logs.DataCenter, e, @"Model serialization: ");
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
            logger.Info(Logs.DataCenter, @"Model deserialized successfully!");
            model.CopyFrom(model2);

            return model2.Rtus.Count == model.Rtus.Count;
        }
        catch (Exception e)
        {
            logger.Exception(Logs.DataCenter, e, @"Model deserialization: ");
            return false;
        }
    }

   
}