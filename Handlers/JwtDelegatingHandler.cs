using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GestionPlazasVacantes.Handlers
{
    public class JwtDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;

        public JwtDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory clientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientFactory = clientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;

            // 🟢 1. Adjuntar JWT si existe
            var token = context?.Session.GetString("JWToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // 🟢 2. Ejecutar request
            var response = await base.SendAsync(request, cancellationToken);

            // 🟡 3. Si NO es 401 → salir
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            // 🔄 4. Intentar refresh
            var refreshToken = context?.Session.GetString("RefreshToken");

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await Logout(context);
                return response;
            }

            var api = _clientFactory.CreateClient("Api");

            var refreshResponse = await api.PostAsJsonAsync(
                "api/auth/refresh",
                new { RefreshToken = refreshToken },
                cancellationToken);

            // ⛔ 5. Refresh falló → logout global
            if (!refreshResponse.IsSuccessStatusCode)
            {
                await Logout(context);
                return response;
            }

            // 🔐 6. Guardar nuevos tokens
            var tokens = await refreshResponse.Content
                .ReadFromJsonAsync<RefreshTokenResponse>();

            context!.Session.SetString("JWToken", tokens!.Token);
            context.Session.SetString("RefreshToken", tokens.RefreshToken);

            // 🔁 7. Reintentar request original con nuevo token
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokens.Token);

            return await base.SendAsync(request, cancellationToken);
        }

        private static async Task Logout(HttpContext? context)
        {
            if (context == null) return;

            context.Session.Clear();

            await context.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            context.Response.Redirect("/Account/Login");
        }

        private class RefreshTokenResponse
        {
            public string Token { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
}
