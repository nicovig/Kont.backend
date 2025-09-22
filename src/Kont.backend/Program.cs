using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models;
using Kont.backend.Tools;
using Kont.backend.Services;
using Kont.backend;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

#if DEBUG
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestHeaders;
});
#else
builder.Services.AddHttpLogging(logging => { });
#endif

// Add services to the container.
builder.Services
    .AddConfig(builder.Configuration)
    .AddControllersWithViews(options =>
    {
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "The field is required.");
    })
    .AddControllersAsServices()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .AddRazorRuntimeCompilation();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>() ?? new AppSettings();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(appSettings.BaseUrl,
                           appSettings.FrontUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_key_that_should_be_in_config")),
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"]
                    };
                });

var connectionString = builder.Configuration.GetConnectionString("Default");
string database = "Postgres";
Action<DbContextOptionsBuilder> databaseSetup = options => options.UseNpgsql(connectionString,
                                                                             o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

builder.Services.AddDbContext<IDatabaseContext, DatabaseContext>(databaseSetup);

// Register services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IScoringService, ScoringService>();
builder.Services.AddScoped<IScoreCalculationService, ScoreCalculationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IActivitiesService, ActivitiesService>();
builder.Services.AddScoped<IGroupsService, GroupsService>();
builder.Services.AddScoped<IReferentsService, ReferentsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISitesService, SitesService>();
builder.Services.AddScoped<IAdministratorsService, AdministratorsService>();
builder.Services.AddScoped<ISubscriptionsService, SubscriptionsService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IGameSessionsService, GameSessionsService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEventInvitationService, EventInvitationService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<DatabaseContext>()
    .AddCheck<UrlHealthChecker>("External dependecies");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Kont.backend.Services.IUserContextService, Kont.backend.Services.UserContextService>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Razor view engine for email templates is configured via AddControllersWithViews().AddRazorRuntimeCompilation()

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

appSettings.Check(app.Logger);

app.Logger.LogInformation("Using database : {Database}", database);

app.UseHttpLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseForwardedHeaders();

app.UseExceptionLoggerMiddleware();

app.UseAuthentication();
app.UseAuthorization();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".mjml"] = "text/html";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
#if DEBUG
    IdentityModelEventSource.ShowPII = true;
#endif
}

app.MapHealthChecks("/healthz");

app.Run();