namespace Singular
{
    public interface SingularConversionValuesUpdatedHandler
    {
        void OnConversionValuesUpdated(int value, int coarse, bool _lock);
    }
}