using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Wolffun.Log;

namespace Wolffun.RestAPI
{
    public class WolffunRequestBody : IDisposable
    {
        private string contentType;
		private byte[] body;


        WolffunRequestBody(string contentType, byte[] body)
		{
			this.contentType = contentType;
			this.body = body;
		}

        public static WolffunRequestBody From(string value)
        {
	        CommonLog.Log("Body : " + value);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(value.ToCharArray());
            //return new WolffunRequestBody("application/x-www-form-urlencoded", bodyRaw);
            return new WolffunRequestBody("application/json", bodyRaw);
        }

        // Should change this one
        // Not good
        public static WolffunRequestBody From<T>(T value)
		{
			var jsonString = JsonConvert.SerializeObject(value);
			CommonLog.Log($"Body raw: {jsonString}");
			var bodyRaw = Encoding.UTF8.GetBytes(jsonString.ToCharArray());

			return new WolffunRequestBody("application/json", bodyRaw);
		}

        public static WolffunRequestBody From(List<IMultipartFormSection> formData)
        {
            // https://answers.unity.com/questions/1354080/unitywebrequestpost-and-multipartform-data-not-for.html

            //List<IMultipartFormSection> formData = form.MultipartForm();

            // generate a boundary then convert the form to byte[]
            byte[] boundary = UnityWebRequest.GenerateBoundary();
            byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
            // my termination string consisting of CRLF--{boundary}--
            byte[] terminate = Encoding.UTF8.GetBytes(String.Concat("\r\n--", Encoding.UTF8.GetString(boundary), "--"));
            // Make complete body from the two byte arrays
            byte[] bodyRaw = new byte[formSections.Length + terminate.Length];
            Buffer.BlockCopy(formSections, 0, bodyRaw, 0, formSections.Length);
            Buffer.BlockCopy(terminate, 0, bodyRaw, formSections.Length, terminate.Length);
            // Set the content type
            string contentType = String.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
            return new WolffunRequestBody(contentType, bodyRaw);
        }

        //[System.Obsolete("WWWForm is obsolete. Use List<IMultipartFormSection> instead")]
        //public static WolffunRequestBody From(WWWForm formData)
        //{
        //	return new WolffunRequestBody("application/x-www-form-urlencoded", formData.data);
        //}

        public string ContentType()
		{
			return contentType;
		}

		public byte[] Body()
		{
			return body;
		}

        public void Dispose()
        {
            contentType = string.Empty;
            body = null;

        }
    }
}
