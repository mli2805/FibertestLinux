using Fibertest.OtdrDataFormat;

namespace Fibertest.Utils;

public static class SorDataExt
{
    public static int GetLandmarkIndexForKeyEventIndex(this OtdrDataKnownBlocks sorData, int keyEventIndex)
    {
        var keyEventNumber = keyEventIndex + 1;
        for (int i = 0; i < sorData.LinkParameters.LandmarksCount; i++)
        {
            if (sorData.LinkParameters.LandmarkBlocks[i].RelatedEventNumber == keyEventNumber)
                return i;
        }

        return -1;
    }

    public static int GetLandmarkToTheLeftFromOwt(this OtdrDataKnownBlocks sorData, int owt)
    {
        var leftLandmarkIndex = 0;
        for (int i = 1; i < sorData.LinkParameters.LandmarksCount; i++)
        {
            if (sorData.LinkParameters.LandmarkBlocks[i].Location < owt)
                leftLandmarkIndex = i;
            else return leftLandmarkIndex;
        }

        return leftLandmarkIndex; // owt to the right of end
    }

    public static double GetDeltaLen(this OtdrDataKnownBlocks sorData, char code)
    {
        var param = code == 'R'
            ? sorData.RftsParameters.UniversalParameters.First(p => p.Name == "EvtRDetectDeltaLen")
            : sorData.RftsParameters.UniversalParameters.First(p => p.Name == "EvtDetectDeltaLen");

        return (double)param.Value / param.Scale;
    }

    public static void EmbedBaseRef(this OtdrDataKnownBlocks measSorData, byte[] baseBytes)
    {
          
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (measSorData.EmbeddedData.EmbeddedDataBlocks != null)
        {
            var embeddedData = measSorData.EmbeddedData.EmbeddedDataBlocks.ToList();
            embeddedData.Add(BufferToEmbeddedDataBlock(baseBytes));
            measSorData.EmbeddedData.EmbeddedDataBlocks = embeddedData.ToArray();
            measSorData.EmbeddedData.EmbeddedBlocksCount = (ushort)embeddedData.Count;
        }
        else
        {
            var embeddedData = new List<EmbeddedData> { BufferToEmbeddedDataBlock(baseBytes) };
            measSorData.EmbeddedData.EmbeddedDataBlocks = embeddedData.ToArray();
            measSorData.EmbeddedData.EmbeddedBlocksCount = (ushort)embeddedData.Count;

        }
    }

    private static EmbeddedData BufferToEmbeddedDataBlock(byte[] buffer)
    {
        return new EmbeddedData
        {
            Description = "SOR",
            DataSize = buffer.Length,
            Data = buffer.ToArray()
        };
    }
}