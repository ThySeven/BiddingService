using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;
using BiddingService.Repository;
using BiddingService.Service;
using System.Security.Claims;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.UseNLog();
// Fetching necessary environment variables
var EndPoint = Environment.GetEnvironmentVariable("VAULT_IP");
var vaultSecret = Environment.GetEnvironmentVariable("VAULT_SECRET");

var httpClientHandler = new HttpClientHandler();
httpClientHandler.ServerCertificateCustomValidationCallback =
    (message, cert, chain, sslPolicyErrors) => { return true; };

// Setting up Vault authentication
IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultSecret);
var vaultClientSettings = new VaultClientSettings(EndPoint, authMethod)
{
    Namespace = "",
    MyHttpClientProviderFunc = handler =>
        new HttpClient(httpClientHandler)
        {
            BaseAddress = new Uri(EndPoint)
        }
};
IVaultClient vaultClient = new VaultClient(vaultClientSettings);

// Fetching JWT configuration from Vault
Secret<SecretData> kv2Secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "jwt", mountPoint: "secret");
var jwtSecret = kv2Secret.Data.Data["secret"];
var jwtIssuer = kv2Secret.Data.Data["issuer"];
var vaultjwtInternalApiKey = kv2Secret.Data.Data["internalApiKey"];

string mySecret = Convert.ToString(jwtSecret) ?? "none";
string myIssuer = Convert.ToString(jwtIssuer) ?? "none";
string vaultInternalApiKey = Convert.ToString(vaultjwtInternalApiKey) ?? "none";
WebManager.GetInstance.HttpClient.DefaultRequestHeaders.Add("X-Internal-ApiKey", vaultInternalApiKey);
builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = myIssuer,
        ValidAudience = "http://localhost",
        IssuerSigningKey =
    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            //AuctionCoreLogger.Logger.Info($"Received API Call from {context.Request.Headers.Origin}");

            if (context.Request.Headers.TryGetValue("X-Internal-ApiKey", out var extractedApiKey))
            {
                var internalApiKey = vaultInternalApiKey; // or fetch from configuration
                if (internalApiKey.Equals(extractedApiKey))
                {
                    //AuctionCoreLogger.Logger.Info($"JWTBypass {context.Request.Headers.Origin}");
                    context.HttpContext.Items["InternalRequest"] = true;

                    // Skip JWT token processing
                    context.Token = null;

                    context.Response.Headers.Add("X-Auth-Skipped", "true");
                }
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            if (context.HttpContext.Items.ContainsKey("InternalRequest"))
            {
                context.Success();
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            if (context.HttpContext.Items.ContainsKey("InternalRequest"))
            {
                context.HandleResponse(); // This suppresses the default 401
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization(options =>
{
    // Policy that checks for the internal request item
    options.AddPolicy("InternalRequestPolicy", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.Identity.IsAuthenticated ||
            (context.Resource as HttpContext)?.Items?.ContainsKey("InternalRequest") == true);
    });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IBiddingRepo, BidService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Items.ContainsKey("InternalRequest"))
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "InternalUser")
        }, "InternalAuthScheme");

        context.User = new ClaimsPrincipal(identity);
    }

    await next();
});

// Clearing default logging providers and using NLog


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Adding authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Shutdown NLog at the end
NLog.LogManager.Shutdown();
