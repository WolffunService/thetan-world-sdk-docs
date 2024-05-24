using System;
using Wolffun.Log;

namespace ThetanSDK.UI
{
    public abstract class ScreenContentMainUI : Screen
    {
        protected UIHelperContainer _uiHelperContainer;

        public virtual void Initialize(UIHelperContainer uiHelperContainer)
        {
            _uiHelperContainer = uiHelperContainer;
        }

        /// <summary>
        /// Use for horizontal screen when change tab won't trigger OnBeforePush and OnAfterPush
        /// </summary>
        public virtual void OnReFocusScreen()
        {}
    }
}