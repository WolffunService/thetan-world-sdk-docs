using BestHTTP;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.WebSocket.Extensions;
using BestHTTP.WebSocket.Frames;
using UnityEngine;
using Wolffun.RestAPI;

namespace Wolffun.RestAPI.ThetanAuth
{
    internal struct ThetanSecretKeyPlugins : IExtension
    {
        private string _appClientId;
        private string _appClientSecret;
        
        internal ThetanSecretKeyPlugins(string appClientId, string appClientSecret)
        {
            _appClientId = appClientId;
            _appClientSecret = appClientSecret;
        }
        
        public void AddNegotiation(HTTPRequest request)
        {
            Debug.Log(WolffunUnityHttp.GenerateThetanSecretKey(_appClientId, _appClientSecret));
            request.AddHeader("X-ThetanSecretKey", WolffunUnityHttp.GenerateThetanSecretKey(_appClientId, _appClientSecret));
        }

        public bool ParseNegotiation(HTTPResponse resp)
        {
            return true;
        }

        public byte GetFrameHeader(WebSocketFrame writer, byte inFlag)
        {
            return inFlag;
        }

        public BufferSegment Encode(WebSocketFrame writer)
        {
            return writer.Data;
        }

        public BufferSegment Decode(byte header, BufferSegment data)
        {
            return data;
        }
    }
}