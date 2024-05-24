using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wolffun.RestAPI.ThetanAuth
{
    /// <summary>
    /// Base class for all authen processor monobehavior
    /// </summary>
    public class MonoBehaviorAuthenProcess : MonoBehaviour, IAuthenSuccessCallback, IAuthenProcessor
    {
        private List<IAuthenSuccessListener> _listAuthenSuccessListeners = new List<IAuthenSuccessListener>();
        
        private List<IPostAuthenProcessor> _listPostAuthenProcessors = new List<IPostAuthenProcessor>();

        /// <summary>
        /// When authen user success, MUST call this function
        /// </summary>
        protected UniTask ProcessAuthenSucceed(LoginType loginType, string accessToken, string refreshToken)
        {
            if (_listAuthenSuccessListeners != null)
            {
                foreach (var listener in _listAuthenSuccessListeners)
                {
                    listener.HandleAuthenSuccess(accessToken, refreshToken);
                }
            }

            return ProcessPostAuthenSuccess(loginType);
        }

        /// <summary>
        /// Processing after user authen succeed
        /// </summary>
        /// <param name="loginType"></param>
        protected async UniTask ProcessPostAuthenSuccess(LoginType loginType)
        {
            if (_listPostAuthenProcessors == null || _listPostAuthenProcessors.Count == 0)
                return;
            
            List<UniTask> listTask = new List<UniTask>();

            var postAuthenProcessMetaData = new PostAuthenSuccessMetaData()
            {
                loginType = loginType
            };
            postAuthenProcessMetaData = AddCustomDataToMetaData(postAuthenProcessMetaData);

            foreach (var processor in _listPostAuthenProcessors)
            {
                listTask.Add(processor.ProcessPostAuthenProcess(postAuthenProcessMetaData));
            }

            await UniTask.WhenAll(listTask);
        }

        /// <summary>
        /// Used to add custom meta data about authen process
        /// Usually used for analytic purpose
        /// </summary>
        protected virtual PostAuthenSuccessMetaData AddCustomDataToMetaData(PostAuthenSuccessMetaData metaData)
        {
            return metaData;
        }

        public void RegisterAuthenSuccessCalback(IAuthenSuccessListener listener)
        {
            if (_listAuthenSuccessListeners == null)
                _listAuthenSuccessListeners = new List<IAuthenSuccessListener>();
        
            _listAuthenSuccessListeners.Add(listener);
        }

        public void UnregisterAuthenSuccessCalback(IAuthenSuccessListener listener)
        {
            if (_listAuthenSuccessListeners == null)
            {
                _listAuthenSuccessListeners = new List<IAuthenSuccessListener>();
                return;
            }
        
            _listAuthenSuccessListeners.Remove(listener);
        }

        public void RegisterPostAuthenProcessor(IPostAuthenProcessor processor)
        {
            if (_listPostAuthenProcessors == null)
            {
                _listPostAuthenProcessors = new List<IPostAuthenProcessor>();
            }

            if (_listPostAuthenProcessors.Contains(processor))
                return;
            
            _listPostAuthenProcessors.Add(processor);
        }

        public void UnRegisterPostAuthenProcessor(IPostAuthenProcessor processor)
        {
            if (_listPostAuthenProcessors == null)
            {
                _listPostAuthenProcessors = new List<IPostAuthenProcessor>();
                return;
            }

            _listPostAuthenProcessors.Remove(processor);
        }
    }

}
