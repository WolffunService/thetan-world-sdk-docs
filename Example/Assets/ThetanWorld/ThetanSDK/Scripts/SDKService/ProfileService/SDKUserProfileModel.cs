using Cysharp.Threading.Tasks.Triggers;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKServices.Profile
{
    public struct SDKUserProfileModel : ICustomDefaultable<SDKUserProfileModel>
    {
        public string address;
        public int ageRange;
        public int avatar;
        public int avatarFrame;
        public string biographic;
        public int changeNameTicket;
        public string country;
        public long createdAt;
        public string id;
        public int nameColor;
        public string nickname;
        public string email;
        public string walletProvider;
        
        public SDKUserProfileModel SetDefault()
        {
            address = string.Empty;
            id = string.Empty;
            nickname = string.Empty;
            country = string.Empty;
            biographic = string.Empty;
            email = string.Empty;

            return this;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(id);
        }
    }
}