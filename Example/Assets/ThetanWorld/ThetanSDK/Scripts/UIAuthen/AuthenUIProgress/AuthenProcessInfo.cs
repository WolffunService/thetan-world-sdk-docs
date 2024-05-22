using System.Collections.Generic;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public class AuthenProcessInfo
    {
        public List<AuthenStepInfo> ListStepProcess;
        public int currentStepIndex;
    }

    public struct AuthenStepInfo
    {
        public string StepName;
    }
}