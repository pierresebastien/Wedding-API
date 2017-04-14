using System;

namespace Wedding.Api.Exceptions
{
	public class WeddingException : Exception
	{
		public WeddingException(string message)
			: base(message)
		{
		}

		public WeddingException()
		{
		}

		public WeddingException(string message, Exception exception)
			: base(message, exception)
		{
		}

		public WeddingException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}
	}
}
