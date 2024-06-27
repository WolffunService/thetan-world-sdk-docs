using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITokenErrorAPIHandle
{
    /// <summary>
    /// When receive error code WSErrorCode.TokenInvalid
    /// </summary>
    public void HandleTokenError();
    
    /// <summary>
    /// When receive error code WSErrorCode.TokenExpired
    /// </summary>
    public void HandleTokenExpire();

    /// <summary>
    /// When receive error code WSErrorCode.UserBanned
    /// </summary>
    public void HandleAccountBanned();
}
