using Newtonsoft.Json;
using System;

namespace Wolffun.RestAPI
{
    [Serializable]
public struct DataResponse<T>
{
    [JsonProperty("success")]
    public bool Success;

    [JsonProperty("page")]
    public Page Paging;

    [JsonProperty("data")]
    public T Data;
}

[Serializable]
public struct Page
{
    [JsonProperty("total")]
    public int Total;
}

public struct WolffunResponseError
{
    [JsonProperty("code")]
    public int Code;

    [JsonProperty("message")]
    public string Message;

    [JsonProperty("status")]
    public string DebugMessage;

    [JsonProperty("rootCode")]
    public int RootCode;

    [JsonProperty("debug")]
    public string DevDebugMessage;

    [JsonProperty("data")]
    public object Data;
    
    public WolffunResponseError(WolffunResponseError other)
    {
        Code = other.Code;
        Message = other.Message;
        DebugMessage = other.DebugMessage;
        RootCode = other.RootCode;
        Data = other.Data;
        DevDebugMessage = other.DevDebugMessage;
    }
    
    public WolffunResponseError(int code, string message, string debugMessage, int rootCode, object data)
    {
        Code = code;
        Message = message;
        DebugMessage = debugMessage;
        RootCode = rootCode;
        Data = data;
        DevDebugMessage = string.Empty;
    }
    
    public WolffunResponseError(int code, string message, string debugMessage, int rootCode)
    {
        Code = code;
        Message = message;
        DebugMessage = debugMessage;
        RootCode = rootCode;
        Data = null;
        DevDebugMessage = string.Empty;
    }
    
    public WolffunResponseError(int code, string message, string debugMessage)
    {
        Code = code;
        Message = message;
        DebugMessage = debugMessage;
        RootCode = 0;
        Data = null;
        DevDebugMessage = string.Empty;
    }
    
    public WolffunResponseError(int code, string message)
    {
        Code = code;
        Message = message;
        DebugMessage = string.Empty;
        RootCode = 0;
        Data = null;
        DevDebugMessage = string.Empty;
    }
    
    public WolffunResponseError(int code)
    {
        Code = code;
        Message = string.Empty;
        DebugMessage = string.Empty;
        RootCode = 0;
        Data = null;
        DevDebugMessage = string.Empty;
    }
    
    public WolffunResponseError(string message)
    {
        Code = 0;
        Message = message;
        DebugMessage = string.Empty;
        RootCode = 0;
        Data = null;
        DevDebugMessage = string.Empty;
    }

    const string debugString = "Code : {0} -- Mess : {1} -- status : {2} -- data : {3}";
    public override string ToString()
    {
        string result = "Code : " + Code + " -- Mess : " + Message + " -- status : " + DebugMessage + " -- data : ";
        if (Data != null)
        {
            return string.Format(debugString, Code, Message, DebugMessage, Data.ToString());
        }
        else
        {
            return string.Format(debugString, Code, Message, DebugMessage, "is null");
        }
    }
}
}

