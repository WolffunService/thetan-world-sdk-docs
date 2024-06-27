using System;
using Newtonsoft.Json;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Wolffun.RestAPI
{
	/// <summary>
	/// Reponse request status http
	/// </summary>
	public class WolffunResponseCommon : IDisposable
	{
		private long _status;
		private string _body;
		private byte[] _rawBody;

		public static WolffunResponseCommon Create(long status, string body, byte[] rawBody)
		{
			var res = Utils.WolffunResponseCommonPool.Get();
			res.SetUp(status, body, rawBody);
			return res;
		}
		public void SetUp(long status, string body, byte[] rawBody)
		{		
			_status = status;
			_body = body;
			_rawBody = rawBody;
		}

		public DataResponse<T> To<T>(JsonSerializerSettings jsonSerializerSettings)
		{
			//return jsonConverter != null ? JsonConvert.DeserializeObject<DataResponse<T>>(_body, jsonConverter) : JsonConvert.DeserializeObject<DataResponse<T>>(_body, DefaultJsonSerializerSettings);
			return JsonConvert.DeserializeObject<DataResponse<T>>(_body, jsonSerializerSettings);
		} 
   

		public WolffunResponseError Error()
		{
			//Debug.LogError(_body);
			return JsonConvert.DeserializeObject<WolffunResponseError>(_body);
		}

		public long Status() =>_status;
		
		public string Body() =>_body;
	
		public byte[] RawBody() =>_rawBody;
	
		public bool IsOk() => _status >= 200 && _status < 300;

		public override string ToString()
		{
			var builder = Utils.StringBuilderPool.Get();
			builder.Clear();
			builder.AppendFormat("status:{0} - - response: {1}", _status.ToString(), _body);
			var s = builder.ToString();
			Utils.StringBuilderPool.Release(builder);
			return s;
		}

		public static WolffunResponseCommon From(BestHTTP.HTTPRequest www) =>  Create(www.Response.StatusCode, www.Response.DataAsText, www.Response.Data);

		public void Dispose() => Utils.WolffunResponseCommonPool.Release(this);
	}
}
