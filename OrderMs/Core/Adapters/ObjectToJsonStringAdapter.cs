using Newtonsoft.Json;
using System.Text.Json;

namespace Core.Adapters
{
    public static class ObjectToJsonStringAdapter
    {
        public static string ConvertToJsonString(object obj)
        {
            string json;

            if (obj is JsonElement jsonElement)
                json = jsonElement.ToString();
            else
                json = JsonConvert.SerializeObject(obj);

            return json;
        }
    }
}
