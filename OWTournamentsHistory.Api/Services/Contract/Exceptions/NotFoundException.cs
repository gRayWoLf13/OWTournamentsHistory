namespace OWTournamentsHistory.Api.Services.Contract.Exceptions
{
    internal sealed class NotFoundException : ServiceException
    {
        public NotFoundException(string? message) : base(message)
        {
        }

        public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}
