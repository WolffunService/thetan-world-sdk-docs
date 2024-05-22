using System;

namespace ThetanSDK.SDKServices.Analytic
{
    [Serializable]
    internal struct AnalyticPCParamModel
    {
        public string key;
        public string value;
    }
    
    [Serializable]
    internal struct SDKAnalyticRequestModel
    {
        public string eventName;
        public long timeStamp;
        public string country;
        public string userId;
        public string platform;
        public string thetan;
        
        public AnalyticPCParamModel[] eventParams;
    }
}