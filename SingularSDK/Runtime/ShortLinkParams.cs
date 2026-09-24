using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Singular
{
    [Preserve]
    [Serializable]
    public class ShortLinkParams
    {
        [Preserve] private string data;
        [Preserve] private string error;

        [Preserve]
        public ShortLinkParams()
        {
        }

        [Preserve]
        [JsonProperty(PropertyName = "data")]
        public string Data
        {
            get { return data; }
            set { data = value; }
        }

        [Preserve]
        [JsonProperty(PropertyName = "error")]
        public string Error
        {
            get { return error; }
            set { error = value; }
        }
    }
}