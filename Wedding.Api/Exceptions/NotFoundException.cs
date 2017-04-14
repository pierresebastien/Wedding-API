using System;

namespace Wedding.Api.Exceptions
{
	public class NotFoundException : WeddingException
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="T:NotFoundException" /> class
		/// </summary>
		public NotFoundException()
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:NotFoundException" /> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String" /> that describes the exception. </param>
		public NotFoundException(string message) : base(message)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="T:NotFoundException" /> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String" /> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public NotFoundException(string message, Exception inner) : base(message, inner)
		{

		}
	}
}
