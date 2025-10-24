using System.Net.Http;
using System.Net.Http.Headers;

namespace DigitalSignClient.Services.Handlers
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly ApiManager _apiManager;

        public AuthTokenHandler(ApiManager apiManager)
        {
            _apiManager = apiManager;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _apiManager.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
