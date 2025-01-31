﻿namespace StorageService.ErrorHandling
{
    public static class ResultExtensions
    {
        public static IResult ToProblemDetails<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException();
            }

            return Results.Problem(
                statusCode: GetStatusCode(result.Error.Type),
                title: GetTitle(result.Error.Type),
                type: GetType(result.Error.Type),
                extensions: new Dictionary<string, object?>
                {
                    {"errors", new[] {result.Error } },
                });
        }
        static int GetStatusCode(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
        }
        static string GetTitle(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Validation => "Bad Request",
                ErrorType.NotFound => "Not Found",
                ErrorType.Conflict => "Conflict",
                ErrorType.Forbidden => "Forbidden",
                _ => "Server Failure"
            };
        }
        static string GetType(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Validation => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                ErrorType.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
                ErrorType.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
            };
        }
    }
}
