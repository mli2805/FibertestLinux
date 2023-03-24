using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Fibertest.Dto;

public class RequestAnswer
{
    public ReturnCode ReturnCode;
    public string? ErrorMessage;

    public RtuOccupationState? RtuOccupationState;

    public RequestAnswer()
    {
    }

    public RequestAnswer(ReturnCode returnCode)
    {
        ReturnCode = returnCode;
    }
}

public static class RequestAnswerExt
{
    public static RequestAnswer TurnInto(this RequestAnswer answer, ReturnCode newReturnCode, string errorMessage)
    {
        answer.ReturnCode = newReturnCode;
        answer.ErrorMessage = errorMessage;
        return answer;
    }

    public static string SerializeToJson(this RequestAnswer answer)
    {
        JsonSerializerSettings jsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
        var res = JsonConvert.SerializeObject(answer, jsonSerializerSettings);
        return res;
    }
}