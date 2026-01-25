using System.Net;
using SmartParking.Domain.Constants;

namespace SmartParking.Application.Common.Exceptions;

public sealed class ForbiddenException : BusinessException
{
    public ForbiddenException(string message = Messages.Common.Forbidden)
        : base(HttpStatusCode.Forbidden, message)
    {
    }
}

