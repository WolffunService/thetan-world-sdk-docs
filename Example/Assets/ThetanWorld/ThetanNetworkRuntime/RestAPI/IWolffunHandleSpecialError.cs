using System;
using UnityEngine;

namespace Wolffun.RestAPI
{
    /// <summary>
    /// Implement special error code handler.
    /// You SHOULD NOT use this unless you know what you are doing
    /// </summary>
    public interface IWolffunHandleSpecialError
    {
        public void HandleMaintain(Action<WSErrorCode> doneCallback = null);
        public void HandleNewVersion(Action<WSErrorCode> doneCallback = null);
        public void HandleLostConnection(WSErrorCode lastErrorCode, Action<WSErrorCode> doneCallback = null);
    }
}