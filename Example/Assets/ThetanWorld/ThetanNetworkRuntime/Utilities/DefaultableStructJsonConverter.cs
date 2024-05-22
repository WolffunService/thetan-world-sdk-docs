using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.Log;

namespace Wolffun.RestAPI
{
    /// <summary>
    /// This is a Json Converter that restores legacy ASP.Net MVC compatible behavior for handling values that cannot be assigned Null.
    /// Historically (before .NET Core) Json values of null would result in errors when setting them into values that cannot be assigned null,
    ///     however these errors treated as warnings and were skipped, leaving the original default values set on the Model. Now in 
    ///     Asp .NET Core, these failures result in Exceptions being thrown.
    /// While previously the result was that these fields were simply left their default values; either the default of the Value type 
    ///     or the default set in the Property Initializer.
    /// Therefore this Json Converter restores this behavior by assigning the Default value safely without any errors being thrown.
    /// </summary>
    public class DefaultableStructJsonConverter : JsonConverter
    {
        public static bool CanTypeBeAssignedNull(Type type)
            => !type.IsValueType || (Nullable.GetUnderlyingType(type) != null);

        //Only Handle Fields that would fail a Null assignment and requires resolving of a non-null existing/default value for the Type!
        public override bool CanConvert(Type objectType) => !CanTypeBeAssignedNull(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingOrDefaultValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Null)
            {
                return existingOrDefaultValue;
            }

            CommonLog.Log("ReadJson: " + token.Type + "__" + token.HasValues + "__" + token.ToString());

            //try
            //{

                return token.ToObject(objectType);
            //}
            //catch
            //{
            //    //return existingOrDefaultValue;
            //    return Activator.CreateInstance(objectType);
            //}
        }

        // Return false; we want normal Json.NET behavior when serializing...
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}