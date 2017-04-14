using System;

namespace Wedding.Api.Business.Responses
{
    public class ErrorResponse
    {
        public ErrorResponse()
        {
        }
        public ErrorResponse(Exception exception)
        {
            ErrorMessage = exception.Message;
            StackTrace = exception.StackTrace;
        }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }
}