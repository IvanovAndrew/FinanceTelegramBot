using Microsoft.Extensions.Logging;
using Refit;

namespace Infrastructure;

public class RefitMessageHandler(ILogger<RefitMessageHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            string? requestBody = null;
            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync(); // garantees the read
                requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            logger.LogInformation(
                "[HttpClient] Request to {Uri}\nMethod: {Method}\nBody:\n{Body}",
                request.RequestUri,
                request.Method,
                requestBody ?? "<null>"
            );
            
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                logger.LogError(
                    "HTTP error {StatusCode} {ReasonPhrase} for request {Url}.\nResponse: {Content}",
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    request.RequestUri,
                    content
                );
            }

            return response;
        }
        catch (ApiException ex)
        {
            logger.LogError(
                ex,
                "Refit ApiException for {Url}: Status={StatusCode}, Reason={ReasonPhrase}, Content={Content}",
                request.RequestUri,
                ex.StatusCode,
                ex.ReasonPhrase,
                ex.Content
            );

            throw; 
        }
    }
}
