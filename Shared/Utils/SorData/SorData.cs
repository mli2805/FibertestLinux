using Fibertest.OtdrDataFormat;
using Fibertest.OtdrDataFormat.IO;
using BinaryReader = System.IO.BinaryReader;

namespace Fibertest.Utils;

public static class SorData
{
    public static string TryGetFromBytes(byte[]? buffer, out OtdrDataKnownBlocks? otdrDataKnownBlocks)
    {
        if (buffer == null)
        {
            otdrDataKnownBlocks = null;
            return "buffer is null";
        }
        using var stream = new MemoryStream(buffer);
        try
        {
            otdrDataKnownBlocks = new OtdrDataKnownBlocks(new OtdrReader(stream).Data);
            return "";
        }
        catch (Exception e)
        {
            otdrDataKnownBlocks = null;
            return e.Message;
        }
    }

    public static OtdrDataKnownBlocks FromBytes(byte[] buffer)
    {
        using var stream = new MemoryStream(buffer);
        return new OtdrDataKnownBlocks(new OtdrReader(stream).Data);
    }

    public static byte[] GetRidOfBase(byte[] sorbytes)
    {
        var otdrDataKnownBlocks = FromBytes(sorbytes);
        var blocks = otdrDataKnownBlocks.EmbeddedData.EmbeddedDataBlocks.Where(block => block.Description != @"SOR").ToArray();
        otdrDataKnownBlocks.EmbeddedData.EmbeddedDataBlocks = blocks;
        return otdrDataKnownBlocks.ToBytes();
    }

    public static byte[] ToBytes(this OtdrDataKnownBlocks sorData)
    {
        using var stream = new MemoryStream();
        sorData.Save(stream);
        return stream.ToArray();
    }

    public static void Save(this OtdrDataKnownBlocks sorData, string filename)
    {
        if (File.Exists(filename))
            File.Delete(filename);
        var folder = Path.GetDirectoryName(filename);
        if (folder != null && !Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        using FileStream fs = File.Create(filename);
        sorData.Save(fs);
    }

    private const double LightSpeed = 0.000299792458; // km/ns
    public static double OwtToLenKm(this OtdrDataKnownBlocks sorData, double owt)
    {
        var owt1 = owt - sorData.GeneralParameters.UserOffset;
        return owt1 * GetOwtToKmCoeff(sorData);
    }

    public static double KeyEventDistanceKm(this OtdrDataKnownBlocks sorData, int eventIndex)
    {
        return sorData.KeyEvents.KeyEvents[eventIndex].EventPropagationTime * GetOwtToKmCoeff(sorData);
    }

    // public static double LandmarkDistanceKm(this OtdrDataKnownBlocks sorData, int landmarkIndex)
    // {
    //     return sorData.LinkParameters.LandmarkBlocks[landmarkIndex].Location * GetOwtToKmCoeff(sorData);
    // }

    public static double GetOwtToKmCoeff(this OtdrDataKnownBlocks sorData)
    {
        return LightSpeed / sorData.FixedParameters.RefractionIndex / 10;
    }

    private static double GetOwtToMmCoeff(this OtdrDataKnownBlocks sorData)
    {
        return LightSpeed * 100000 / sorData.FixedParameters.RefractionIndex;
    }

    public static double GetTraceLengthKm(this OtdrDataKnownBlocks sorData)
    {
        var owt = sorData.KeyEvents.KeyEvents[sorData.KeyEvents.KeyEventsCount - 1].EventPropagationTime;
        return sorData.OwtToLenKm(owt);
    }


    public static double GetDistanceBetweenLandmarksInMm(
        this OtdrDataKnownBlocks sorData, int leftIndex, int rightIndex)
    {
        var owt1 = sorData.LinkParameters.LandmarkBlocks[leftIndex].Location;
        var owt2 = sorData.LinkParameters.LandmarkBlocks[rightIndex].Location;
        return (owt2 - owt1) * GetOwtToMmCoeff(sorData);
    }

    public static int GetOwtFromMm(this OtdrDataKnownBlocks sorData, int distance)
    {
        return (int)(distance / sorData.GetOwtToMmCoeff());
    }

    public static OtdrDataKnownBlocks? GetBase(this OtdrDataKnownBlocks sorData)
    {
        var baseBuffer = sorData.EmbeddedData.EmbeddedDataBlocks.FirstOrDefault(b => b.Description == @"SOR");
        return baseBuffer == null ? null : FromBytes(baseBuffer.Data);
    }

    public static IEnumerable<RftsEventsBlock> GetRftsEventsBlockForEveryLevel(this OtdrDataKnownBlocks sorData)
    {
        for (int i = 0; i < sorData.EmbeddedData.EmbeddedBlocksCount; i++)
        {
            if (sorData.EmbeddedData.EmbeddedDataBlocks[i].Description != @"RFTSEVENTS") continue;

            var embData = sorData.EmbeddedData.EmbeddedDataBlocks[i];
            var stream = new MemoryStream(embData.Data, 0, embData.DataSize);
            var reader = new BinaryReader(stream);
            var opxReader = new OtdrDataFormat.IO.BinaryReader(reader);
            var revNumber = opxReader.ReadUInt16();

            var deserializer = new OpxDeserializer(opxReader, revNumber);
            yield return (RftsEventsBlock)deserializer.Deserialize(typeof(RftsEventsBlock));
        }
    }
}