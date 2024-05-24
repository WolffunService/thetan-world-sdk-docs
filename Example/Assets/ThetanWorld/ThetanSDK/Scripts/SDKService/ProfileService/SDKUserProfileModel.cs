using Cysharp.Threading.Tasks.Triggers;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKServices.Profile
{
    public struct SDKUserProfileModel : ICustomDefaultable<SDKUserProfileModel>
    {
        /// <summary>
        /// Wallet address
        /// </summary>
        public string address;
        
        /// <summary>
        /// Age range of user
        /// </summary>
        public int ageRange;
        
        /// <summary>
        /// AvatarId of profile
        /// </summary>
        public int avatar;
        
        /// <summary>
        /// AvatarFrameId of profile
        /// </summary>
        public int avatarFrame;
        
        /// <summary>
        /// Biographic of profile
        /// </summary>
        public string biographic;
        
        /// <summary>
        /// Count how many change name ticket user have
        /// </summary>
        public int changeNameTicket;
        
        /// <summary>
        /// Country code of user
        /// </summary>
        public string country;
        
        /// <summary>
        /// epoch timestamp of when this account is created
        /// </summary>
        public long createdAt;
        
        /// <summary>
        /// Id of this account
        /// </summary>
        public string id;
        
        /// <summary>
        /// Name color Id of profile
        /// </summary>
        public int nameColor;
        
        /// <summary>
        /// Username
        /// </summary>
        public string nickname;
        
        /// <summary>
        /// Email of profile
        /// </summary>
        public string email;
        
        /// <summary>
        /// Wallet provider of account
        /// </summary>
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