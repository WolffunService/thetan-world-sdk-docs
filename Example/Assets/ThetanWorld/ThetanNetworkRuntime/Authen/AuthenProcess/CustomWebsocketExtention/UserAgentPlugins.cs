using BestHTTP;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.WebSocket.Extensions;
using BestHTTP.WebSocket.Frames;

namespace Wolffun.RestAPI.ThetanAuth
{
    public class UserAgentPlugins : IExtension
    {
        public void AddNegotiation(HTTPRequest request)
        {
            request.AddHeader("User-Agent", WolffunUnityHttp.Instance.GeneratedUserAgentStringString);
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