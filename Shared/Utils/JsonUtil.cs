using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Fibertest.Utils
{
    public static class JsonUtil
    {
        public static string ToCamelCaseJson(this object o)
        {
            return JsonConvert.SerializeObject(o,
                new JsonSerializerSettings() {
                    ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
                });
        }
    }
}
