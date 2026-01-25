using System.Net;

namespace SmartParking.Application.Common.Exceptions;

public abstract class BusinessException : Exception
{
    protected BusinessException(HttpStatusCode statusCode, string message, IEnumerable<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors?.ToArray();
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyList<string>? Errors { get; }
}

