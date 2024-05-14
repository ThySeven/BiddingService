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

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

var builder = WebApplication.CreateBuilder(args);

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

// Extracting JWT configuration
string mySecret = Convert.ToString(jwtSecret) ?? "none";
string myIssuer = Convert.ToString(jwtIssuer) ?? "none";

// Adding JWT authentication to the service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret))
        };
    });

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IBiddingRepo, BidService>();

// Clearing default logging providers and using NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

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
