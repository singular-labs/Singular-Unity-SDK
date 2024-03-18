namespace Singular
{
    public interface SingularSdidAccessorHandler
    {
        void DidSetSdid(string result);
        void SdidReceived(string result);
    }
}