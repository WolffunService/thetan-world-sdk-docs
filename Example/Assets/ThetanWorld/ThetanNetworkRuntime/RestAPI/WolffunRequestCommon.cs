using System;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using Wolffun.Log;

namespace Wolffun.RestAPI
{
    public class WolffunRequestCommon : IDisposable
    {
        private string _url;
        private HTTPMethods _method;
        private readonly Dictionary<string, string> _headers;
        private WolffunRequestBody _body;
        private TimeSpan _timeout;

        public string Url() => _url;
        public string URL => _url;

        public HTTPMethods Method() => _method;

        public WolffunRequestBody Body() => _body;

        public Dictionary<string, string> Headers() => _headers;

        public TimeSpan Timeout() => _timeout;

        public static WolffunRequestCommon Create(string url)
        {
            var request = Utils.WolffunRequestCommonPool.Get();
            request._method = HTTPMethods.Get;
            request._url = url;
            return request;
        }

        public WolffunRequestCommon()
        {
            _method = HTTPMethods.Get;
            _body = null;
            _timeout = TimeSpan.FromSeconds(30);
            _headers = new Dictionary<string, string>();
        }

        public void SetUrl(string url) => _url = url;

        public void AddHeader(string name, string value) => _headers.Add(name, value);

        public void RemoveHeader(string name) => _headers.Remove(name);

        public void SetTimeout(TimeSpan timeout) => _timeout = timeout;

        private WolffunRequestCommon Method(HTTPMethods method, WolffunRequestBody body)
        {
            this._method = method;
            this._body = body;
            return this;
        }

        public WolffunRequestCommon Get() => Method(HTTPMethods.Get, null);

        public WolffunRequestCommon GetQuery(KeyValuePair<string, string>[] query)
        {
            if (query == null)
            {
                AdminLog.LogError("Query keyValuePair is NULL");
                return this;
            }

            bool isFirstQuery = true;
            
            var builder = Utils.StringBuilderPool.Get();
            builder.Clear();
            builder.Append(_url);
            builder.Append('?');
            var length = builder.Length;
            foreach (var keyValuePair in query)
            {
                if (!isFirstQuery)
                {
                    builder.Append('&');
                }
                
                isFirstQuery = false;
                builder.Append(keyValuePair.Key);
                builder.Append('=');
                builder.Append(UnityWebRequest.EscapeURL(keyValuePair.Value));
            }

            _url = builder.ToString();
            Utils.StringBuilderPool.Release(builder);
            return this;
        }
        
        public WolffunRequestCommon GetQuery(KeyValuePair<string, string> pair)
        {
            var builder = Utils.StringBuilderPool.Get();
            builder.Clear();
            builder.Append(_url);
            builder.Append('?');
            builder.Append('&');
            builder.Append(pair.Key);
            builder.Append('=');
            builder.Append(UnityWebRequest.EscapeURL(pair.Value));
            _url = builder.ToString();
            Utils.StringBuilderPool.Release(builder);
            return this;
        }

        public WolffunRequestCommon ApplyPathParam(KeyValuePair<string, string> pair)
        {
            var key = "{" + ConvertToSnakeCase(pair.Key) + "}";
            _url = _url.Replace(key, pair.Value);
            return this;
        }

        public WolffunRequestCommon ApplyPathParam(KeyValuePair<string, string>[] pairs)
        {
            foreach (var pair in pairs)
            {
                ApplyPathParam(pair);
            }

            return this;
        }

        private static string ConvertToSnakeCase(string input)
        {
            string snakeCase = Regex.Replace(input, "(?<!^)([A-Z])", "_$1").ToLower();
            return snakeCase;
        }

        public WolffunRequestCommon Post(WolffunRequestBody requestBody)
        {
            if (requestBody == null)
            {
                requestBody = WolffunRequestBody.From<object>(null);
            }
            return Method(HTTPMethods.Post, requestBody);
        }
        public WolffunRequestCommon Patch(WolffunRequestBody requestBody)
        {
            if (requestBody == null)
            {
                requestBody = WolffunRequestBody.From<object>(null);
            }
            return Method(HTTPMethods.Patch, requestBody);
        }

        public WolffunRequestCommon Put(WolffunRequestBody requestBody) => Method(HTTPMethods.Put, requestBody);

        public WolffunRequestCommon Delete(WolffunRequestBody requestBody = null) => Method(HTTPMethods.Delete, requestBody);

        public void Dispose()
        {
            _url = string.Empty;
            _method = HTTPMethods.Get;
            _headers?.Clear();
            _body?.Dispose();
            _timeout = TimeSpan.FromSeconds(30);
            Utils.WolffunRequestCommonPool.Release(this);
        }
    }
}