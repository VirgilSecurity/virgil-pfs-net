using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Virgil.PFS.Client
{
    /// <summary>
    /// Serializes objects to the JavaScript Object Notation (JSON) and 
    /// deserializes JSON data to objects. 
    /// </summary>
    public class JsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter()
            },
            MissingMemberHandling = MissingMemberHandling.Error
        };

        public static string Serialize(object model)
        {
            return JsonConvert.SerializeObject(model, Settings);
        }

        public static TModel Deserialize<TModel>(string json)
        {
            return JsonConvert.DeserializeObject<TModel>(json, Settings);
        }
    }
}
