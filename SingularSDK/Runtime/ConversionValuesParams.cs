using System;
using Newtonsoft.Json;

namespace Singular
{
    [Serializable]
    public class ConversionValuesParams
    {
        private int _value;
        private int _coarse;
        private bool _lock;

        public ConversionValuesParams()
        {
        }

        [JsonProperty(PropertyName = "value")]
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        [JsonProperty(PropertyName = "coarse")]
        public int Coarse
        {
            get { return _coarse; }
            set { _coarse = value; }
        }

        [JsonProperty(PropertyName = "lock")]
        public bool Lock
        {
            get { return _lock; }
            set { _lock = value; }
        }
    }
}