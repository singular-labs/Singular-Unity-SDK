using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Singular
{
    [Preserve]
    [Serializable]
    public class ConversionValuesParams
    {
        [Preserve] private int _value;
        [Preserve] private int _coarse;
        [Preserve] private bool _lock;

        [Preserve]
        public ConversionValuesParams()
        {
        }

        [Preserve]
        [JsonProperty(PropertyName = "value")]
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        [Preserve]
        [JsonProperty(PropertyName = "coarse")]
        public int Coarse
        {
            get { return _coarse; }
            set { _coarse = value; }
        }

        [Preserve]
        [JsonProperty(PropertyName = "lock")]
        public bool Lock
        {
            get { return _lock; }
            set { _lock = value; }
        }
    }
}