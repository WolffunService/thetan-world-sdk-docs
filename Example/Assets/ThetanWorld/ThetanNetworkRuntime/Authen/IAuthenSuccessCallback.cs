using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wolffun.RestAPI.ThetanAuth
{
    public interface IAuthenSuccessListener
    {
        public void HandleAuthenSuccess(string accessToken, string refreshToken);
    }

    public interface IAuthenSuccessCallback
    {
        public void RegisterAuthenSuccessCalback(IAuthenSuccessListener listener);
        public void UnregisterAuthenSuccessCalback(IAuthenSuccessListener listener);
    }

    /// <summary>
    /// Interface for all authen processor
    /// </summary>
    public interface IAuthenProcessor
    {
        public void RegisterPostAuthenProcessor(IPostAuthenProcessor processor);
        public void UnRegisterPostAuthenProcessor(IPostAuthenProcessor processor);
    }

    /// <summary>
    /// Interface for authen post processor.
    /// Post processor will be invoked right after user logged in using authen processor
    /// </summary>
    public interface IPostAuthenProcessor
    {
        public UniTask ProcessPostAuthenProcess(PostAuthenSuccessMetaData metaData);
    }

    public struct PostAuthenSuccessMetaData
    {
        public LoginType loginType;
        public string deviceId;
    }

    public enum LoginType
    {
        WolffunId,
        LinkAccount,
        GuessAccount,
        QRCodeOrThetanApp
    }
}

