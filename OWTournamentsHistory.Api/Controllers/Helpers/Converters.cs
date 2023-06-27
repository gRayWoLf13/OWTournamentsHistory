using Microsoft.AspNetCore.Mvc;
using OWTournamentsHistory.Api.Services.Contract.Exceptions;

namespace OWTournamentsHistory.Api.Controllers.Helpers
{
    internal static class Converters
    {
        public static ActionResult ToActionResult(this ServiceException exception) => exception switch
        {
            NotFoundException => new NotFoundResult(),
            InvalidParametersException => new BadRequestResult(),

            _ => exception.ToGenericActionResult(),
        };

        public static ActionResult ToGenericActionResult(this Exception exception) => 
            new ObjectResult(exception.Message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

        public static async Task<ActionResult<T>> WrapApiCall<T>(Func<Task<T>> action)
        {
            try
            {
                return new ActionResult<T>(await action());
            }
            catch (ServiceException ex)
            {
                return ex.ToActionResult();
            }
            catch (Exception ex)
            {
                return ex.ToGenericActionResult();
            }
        }

        public static async Task<ActionResult> WrapApiCall(Func<Task> action, ActionResult? successResult = null)
        {
            try
            {
                await action();
                return successResult ?? new OkResult();
            }
            catch (ServiceException ex)
            {
                return ex.ToActionResult();
            }
            catch (Exception ex)
            {
                return ex.ToGenericActionResult();
            }
        }
    }
}
