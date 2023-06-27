namespace OWTournamentsHistory.Api.Services.Contract.Exceptions
{
    internal abstract class ServiceException : Exception
    {
        public ServiceException(string? message) : base(message)
        {
        }

        public ServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
