using Wolffun.RestAPI;

namespace ThetanSDK
{
    public struct VersionDataModel : ICustomDefaultable<VersionDataModel>
    {
        public string[] supportedVersions;
        
        public VersionDataModel SetDefault()
        {
            supportedVersions = null;
            return this;
        }

        public bool IsEmpty()
        {
            return supportedVersions == null;
        }
    }
}