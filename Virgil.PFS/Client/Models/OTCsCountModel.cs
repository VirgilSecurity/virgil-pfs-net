using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virgil.PFS
{
    public class OTCsCountModel
    {
        [JsonProperty("active")]
        public int Active { get; set; }

        [JsonProperty("exhausted")]
        public int Exhausted { get; set; }
    }
}