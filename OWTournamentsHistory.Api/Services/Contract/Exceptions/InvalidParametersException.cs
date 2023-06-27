namespace OWTournamentsHistory.Api.Services.Contract.Exceptions
{
    internal sealed class InvalidParametersException : ServiceException
    {
        public InvalidParametersException(string? message) : base(message)
        {
        }

        public InvalidParametersException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        public InvalidParametersException(params string[] parameterNames) : base($"The following method parameters are invalid: {string.Join(", ", parameterNames)}")
        {
        }
    }
}
