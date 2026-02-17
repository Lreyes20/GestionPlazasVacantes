using System.Net.Http.Headers;

namespace GestionPlazasVacantes.Data
{
    public class JwtHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;

        public JwtHandler(IHttpContextAccessor http)
        {
            _http = http;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _http.HttpContext?.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

}
