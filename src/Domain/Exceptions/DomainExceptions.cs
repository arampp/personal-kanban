namespace PersonalKanban.Domain.Exceptions;

public class RequestFailed : Exception
{
    public RequestFailed(string? message) : base(message)
    {
    }

    public RequestFailed(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public class NotFound : Exception
{
    public NotFound(string? message) : base(message)
    {
    }

    public NotFound(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}