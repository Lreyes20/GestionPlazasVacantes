using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace GestionPlazasVacantes.Services
{
    public class JwtDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;
        private readonly IHttpClientFactory _factory;

        public JwtDelegatingHandler(IHttpContextAccessor http, IHttpClientFactory factory)
        {
            _http = http;
            _factory = factory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var ctx = _http.HttpContext;

            // Si por alguna razón no hay contexto (tareas en background, etc.)
            if (ctx == null)
                return await base.SendAsync(request, cancellationToken);

            // 1) Adjuntar JWT si existe
            var jwt = ctx.Session.GetString("JWToken");
            if (!string.IsNullOrWhiteSpace(jwt))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await base.SendAsync(request, cancellationToken);

            // 2) Si NO es 401, retornamos normal
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            // 3) Si es 401, intentamos refresh
            var refreshToken = ctx.Session.GetString("RefreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await ForceLogout(ctx);
                return response;
            }

            var refreshed = await TryRefresh(ctx, refreshToken, cancellationToken);
            if (!refreshed)
            {
                await ForceLogout(ctx);
                return response;
            }

            // 4) Reintentar request original con nuevo JWT
            var newJwt = ctx.Session.GetString("JWToken");
            if (string.IsNullOrWhiteSpace(newJwt))
            {
                await ForceLogout(ctx);
                return response;
            }

            // IMPORTANTE: clonar request, porque ya fue enviado
            var retryRequest = await CloneHttpRequestMessageAsync(request);
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newJwt);

            return await base.SendAsync(retryRequest, cancellationToken);
        }

        private async Task<bool> TryRefresh(HttpContext ctx, string refreshToken, CancellationToken ct)
        {
            // Cliente "Api" SIN este handler (para evitar loop infinito)
            var apiNoAuth = _factory.CreateClient("ApiNoAuth");

            var res = await apiNoAuth.PostAsJsonAsync("api/auth/refresh", new
            {
                refreshToken = refreshToken
            }, ct);

            if (!res.IsSuccessStatusCode)
                return false;

            var payload = await res.Content.ReadFromJsonAsync<RefreshResponse>(cancellationToken: ct);
            if (payload == null || string.IsNullOrWhiteSpace(payload.Token) || string.IsNullOrWhiteSpace(payload.RefreshToken))
                return false;

            ctx.Session.SetString("JWToken", payload.Token);
            ctx.Session.SetString("RefreshToken", payload.RefreshToken);

            return true;
        }

        private async Task ForceLogout(HttpContext ctx)
        {
            // opcional: llamar API logout global si quieres
            // var apiNoAuth = _factory.CreateClient("ApiNoAuth");
            // await apiNoAuth.PostAsJsonAsync("api/auth/logout", new { refreshToken = ctx.Session.GetString("RefreshToken") });

            ctx.Session.Remove("JWToken");
            ctx.Session.Remove("RefreshToken");

            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            // Copiar headers
            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Copiar contenido si existe
            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;

                clone.Content = new StreamContent(ms);

                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        private sealed class RefreshResponse
        {
            public string Token { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
}
