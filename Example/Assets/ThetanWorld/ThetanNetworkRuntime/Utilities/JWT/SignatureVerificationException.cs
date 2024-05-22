using System;

namespace Wolffun.RestAPI.JWT
{
    public class SignatureVerificationException : Exception
	{
		public SignatureVerificationException(string message)
			: base(message)
		{
		}
	}
}