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

    public interface IAuthenProcessor
    {
        public void RegisterPostAuthenProcessor(IPostAuthenProcessor processor);
        public void UnRegisterPostAuthenProcessor(IPostAuthenProcessor processor);
    }

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

