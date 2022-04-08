using System.Net;

public static class Utility
{
    // Original source: https://docs.microsoft.com/en-us/azure/architecture/patterns/retry
    public static bool IsTransient(Exception ex)
    {
        // Determine if the exception is transient.
        // In some cases this is as simple as checking the exception type, in other
        // cases it might be necessary to inspect other properties of the exception.
        if (ex is TransientException)
        {
            return true;
        }

        var httpRequestException = ex as WebException;
        
        if (httpRequestException != null)
        {
            // If the web exception contains one of the following status values it might be transient.
            return new[] {  WebExceptionStatus.ConnectionClosed,
                            WebExceptionStatus.Timeout,
                            WebExceptionStatus.RequestCanceled }.Contains(httpRequestException.Status);
        }

        // Additional exception checking logic goes here.
        return false;
    }
}