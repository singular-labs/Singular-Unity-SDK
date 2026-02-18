using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Singular
{
    public interface SingularLinkHandler
    {
        void OnSingularLinkResolved(SingularLinkParams linkParams);
    }

    [Preserve]
    [Serializable]
    public class SingularLinkParams
    {
        [Preserve] private string _deeplink;
        [Preserve] private string _passthrough;
        [Preserve] private bool _isDeferred;
        [Preserve] private Dictionary<string, string> _urlParameters;

        [Preserve]
        public SingularLinkParams()
        {
        }

        [Preserve]
        [JsonProperty(PropertyName = "deeplink")]
        public string Deeplink
        {
            get { return _deeplink; }
            set { _deeplink = value; }
        }

        [Preserve]
        [JsonProperty(PropertyName = "passthrough")]
        public string Passthrough
        {
            get { return _passthrough; }
            set { _passthrough = value; }
        }

        [Preserve]
        [JsonProperty(PropertyName = "is_deferred")]
        public bool IsDeferred
        {
            get { return _isDeferred; }
            set { _isDeferred = value; }
        }

        [Preserve]
        [JsonProperty(PropertyName = "url_parameters")]
        public Dictionary<string, string> UrlParameters
        {
            get { return _urlParameters; }
            set { _urlParameters = value; }
        }
    }

}