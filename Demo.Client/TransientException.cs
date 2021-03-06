public class TransientException : Exception
{
    public TransientException()
    {
    }

    public TransientException(string? message) : base(message)
    {
    }

    public TransientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}