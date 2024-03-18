using System;
using Newtonsoft.Json;

namespace Singular
{
    public interface SingularConversionValueUpdatedHandler
    {
        void OnConversionValueUpdated(int value);
    }
}