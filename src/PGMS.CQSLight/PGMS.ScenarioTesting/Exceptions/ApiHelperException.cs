using System.Net;

namespace PGMS.ScenarioTesting.Exceptions;

public class ApiHelperException : Exception
{
    public string Resource { get; }
    public string ResponseContent { get; }
    public HttpStatusCode ResultStatusCode { get; }

    public ApiHelperException(string resource, string responseContent) : base($"Error With Resource {resource}. Details: {responseContent}")
    {
        Resource = resource;
        ResponseContent = responseContent;
    }

    public ApiHelperException(string? message, string resource, string responseContent, HttpStatusCode resultStatusCode) : base(message)
    {
        Resource = resource;
        ResponseContent = responseContent;
        ResultStatusCode = resultStatusCode;
    }
}