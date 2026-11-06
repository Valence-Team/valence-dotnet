using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Valence.Observatorio.Application.Configs;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.API.Extensions
{
    public static class AutenticacaoExtensions
    {
        public const string PoliticaGestor = "Gestor";

        public static IServiceCollection AddAutenticacaoJwt(this IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                        NameClaimType = JwtRegisteredClaimNames.Sub,
                        RoleClaimType = JwtSettings.ClaimPapel
                    };
                });

            services.AddAuthorization(options =>
                options.AddPolicy(PoliticaGestor, politica => politica.RequireRole(nameof(Papel.GESTOR))));

            return services;
        }
    }
}
