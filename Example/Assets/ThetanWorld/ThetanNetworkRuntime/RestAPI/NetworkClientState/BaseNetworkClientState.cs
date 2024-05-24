using System;
using System.Text;
using Wolffun.NetworkUtility;
using Wolffun.RestAPI.ThetanAuth;

namespace Wolffun.RestAPI.NetworkClientState
{
    /// <summary>
    /// Base class for every network client state
    /// </summary>
    internal abstract class BaseNetworkClientState : IDisposable
    {
        protected NetworkClient _networkClient;
        private Action<BaseNetworkClientState, BaseNetworkClientState> _requestChangeStateCallback;
        protected NetworkReachabilityDetect _networkReachabilityDetect;
        
        public BaseNetworkClientState(
            NetworkReachabilityDetect networkReachabilityDetect,
            NetworkClient networkClient, 
            Action<BaseNetworkClientState, BaseNetworkClientState> requestChangeStateCallback)
        {
            _networkReachabilityDetect = networkReachabilityDetect;
            _networkClient = networkClient;
            _requestChangeStateCallback = requestChangeStateCallback;
        }

        protected bool IsConnetedToNetwork()
        {
            if (_networkReachabilityDetect == null)
                return false;

            return _networkReachabilityDetect.CurrentNetworkState == NetworkReachabilityState.Reachable;
        }

        protected void RequestChangeState(BaseNetworkClientState nextState)
        {
            _requestChangeStateCallback?.Invoke(this, nextState);
        }

        public virtual void Dispose()
        {
            _networkClient = null;
            _requestChangeStateCallback = null;
            _networkReachabilityDetect = null;
        }

        public virtual void OnUserRequestLoggout(){}
        public virtual void OnUserBanned(){}
        public virtual void OnUserDeleteAccount(){}
        
        public abstract ThetanNetworkClientState ClientState { get; }
        public virtual void OnEnterState(BaseNetworkClientState prevState){}
        
        /// <summary>
        /// Function is called by network client every interval to update network client state
        /// </summary>
        public virtual void OnUpdateState(){}
        
        public virtual void OnExitState(BaseNetworkClientState nextState){}
    }
}