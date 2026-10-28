using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using BlazorShared;
using BlazorShared.Interfaces;
using Dynatrace.OpenKit;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Objects;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using JsonValue = Dynatrace.OpenKit.Util.Json.Objects.JsonValue;

namespace Microsoft.eShopWeb.PublicApi.Extensions;

public static class ServiceCollectionExtensions
{
    // configures the openkit instance
    public static void AddBusinessEventAgentServices(this IServiceCollection services,OpenKitOptions openKitOptions)
    {
        // to-do: get values from settings json
        var endpointURL = "https://DESKTOP-GEQUNFQ:9999/mbeacon/hkh71453";
        var applicationID = openKitOptions.ApplicationId;
        IOpenKit openKit = new DynatraceOpenKitBuilder(endpointURL, applicationID, 123).Build();
        var openKitSession = openKit.CreateSession();
        BizeventAgent bizeventAgent = new BizeventAgent(openKit, openKitSession);
        services.AddSingleton(bizeventAgent);
    }
    
    public static void AddCustomServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
        services.Configure<CatalogSettings>(configuration);

        var catalogSettings = configuration.Get<CatalogSettings>() ?? new CatalogSettings();
        services.AddSingleton<IUriComposer>(new UriComposer(catalogSettings));
        services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
        services.AddScoped<ITokenClaimsService, IdentityTokenClaimService>();
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddJwtSettings(configuration, environment);

        services.AddAuthentication(config =>
        {
            config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((config, jwtSettings) =>
            {
                var settings = jwtSettings.Value;
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = settings.GetSecurityKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true
                };
            });
    }

    public static void AddCorsPolicy(this IServiceCollection services, string policyName, BaseUrlConfiguration baseUrlConfig)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: policyName,
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder.WithOrigins(baseUrlConfig!.WebBase.Replace("host.docker.internal", "localhost").TrimEnd('/'));
                    corsPolicyBuilder.AllowAnyMethod();
                    corsPolicyBuilder.AllowAnyHeader();
                });
        });
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.SwaggerDocument(c =>
        {
            c.DocumentSettings = s =>
            {
                s.Title = "My API";
                s.Version = "v1";
                s.AddAuth("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
            };
            c.EnableJWTBearerAuth = false;
            c.AutoTagPathSegmentIndex = 0;
            c.ShortSchemaNames = true;
        });
    }
}

public class BizeventAgent : IBizeventExporter, IDisposable
{
    private IOpenKit openKitInstance;
    private ISession session;

    public BizeventAgent(IOpenKit openKitInstance, ISession session)
    {
        this.openKitInstance = openKitInstance;
        this.session = session;
    }
    public void sendBizEvent(string eventName, Dictionary<string, JsonValue> data)
    {
        this.session.SendBizEvent(eventName, data);
    }

    public void Dispose()
    {
        Console.WriteLine($"{nameof(BizeventAgent)} disposed");
        this.openKitInstance.Shutdown();
    }
}
