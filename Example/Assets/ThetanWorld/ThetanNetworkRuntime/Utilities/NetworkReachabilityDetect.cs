using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Wolffun.Log;

namespace Wolffun.NetworkUtility
{
    public enum NetworkReachabilityState
    {
        Unknown = 0,
        Reachable = 1,
        NotReachable = 2,
    }
    
    public class NetworkReachabilityDetect : MonoBehaviour
    {
        private const int NUMBER_NETWORK_STATE_DIFFERENCE_TO_TRIGGER_CHANGE = 3;
        private NetworkReachabilityState _currentNetworkState;

        public NetworkReachabilityState CurrentNetworkState => _currentNetworkState;

        private Action<NetworkReachabilityState> _onChangeNetworkStateCallback;

        private int _countNumberNetworkStateChange = 0;
        
        private void Awake()
        {
            _currentNetworkState = NetworkReachabilityState.Unknown;

            _currentNetworkState = Application.internetReachability == UnityEngine.NetworkReachability.NotReachable
                ? NetworkReachabilityState.NotReachable
                : NetworkReachabilityState.Reachable;
            
            _onChangeNetworkStateCallback?.Invoke(_currentNetworkState);
        }

        private void FixedUpdate()
        {
            var newNetworkState = _currentNetworkState = Application.internetReachability == UnityEngine.NetworkReachability.NotReachable
                ? NetworkReachabilityState.NotReachable
                : NetworkReachabilityState.Reachable;
            
            if (newNetworkState == _currentNetworkState)
            {
                _countNumberNetworkStateChange = 0;
                return;
            }
            
            _countNumberNetworkStateChange++;
            if (_countNumberNetworkStateChange < NUMBER_NETWORK_STATE_DIFFERENCE_TO_TRIGGER_CHANGE)
                return;

            _countNumberNetworkStateChange = 0;
            _currentNetworkState = newNetworkState;
            _onChangeNetworkStateCallback?.Invoke(newNetworkState);
        }

        public void RegisterChangeNetworkStateCallback(Action<NetworkReachabilityState> callback)
        {
            _onChangeNetworkStateCallback += callback;
        }
        
        public void UnRegisterChangeNetworkStateCallback(Action<NetworkReachabilityState> callback)
        {
            _onChangeNetworkStateCallback -= callback;
        }
    }
}

