using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.StaticFiles;
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
    });

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
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = static context =>
                        {
                            if (context.Request.Headers.Authorization.Count == 0 && context.Request.Query.TryGetValue("access_token", out var token))
                                context.Token = token;
                            return Task.CompletedTask;
                        }
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
builder.Services.AddScoped<IPoolsService, PoolsService>();
builder.Services.AddScoped<IReferentsService, ReferentsService>();

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<DatabaseContext>()
    .AddCheck<UrlHealthChecker>("External dependecies");

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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