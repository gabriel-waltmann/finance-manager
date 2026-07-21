using Microsoft.EntityFrameworkCore;
using api.Services.Transaction;
using api.Services.Dashboard;
using api.Services.File;
using api.Services.FileProcessing;
using api.Services.Job;
using api.Services.Person;
using api.Services.TransactionImport;
using api.Services.TransactionPerson;
using api.Settings;
using api.Helpers.Database;
using api.Exceptions.Database;
using api.Models.Database;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Database
var databaseSettingsSection = builder.Configuration.GetSection("Database");
var databaseSettings = databaseSettingsSection.Get<DatabaseSettings>() ?? throw new NotFoundSettingsDatabaseException();
var databaseUrl = BuildDatabaseHelper.BuildUrlString(databaseSettings);
builder.Services.Configure<DatabaseSettings>(databaseSettingsSection);
builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(databaseUrl));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Redis
var redisSettingsSection = builder.Configuration.GetSection("Redis");
var redisSettings = redisSettingsSection.Get<RedisSettings>()
  ?? throw new InvalidOperationException("Redis settings not found.");
builder.Services.Configure<RedisSettings>(redisSettingsSection);
builder.Services.AddSingleton<IConnectionMultiplexer>(
  ConnectionMultiplexer.Connect($"{redisSettings.Host}:{redisSettings.Port}")
);

// RabbitMQ
var rabbitMqSettingsSection = builder.Configuration.GetSection("RabbitMq");
_ = rabbitMqSettingsSection.Get<RabbitMqSettings>()
  ?? throw new InvalidOperationException("RabbitMQ settings not found.");
builder.Services.Configure<RabbitMqSettings>(rabbitMqSettingsSection);
builder.Services.AddSingleton<RabbitMqConnection>();

// Services
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<TransactionPersonService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<FileProcessingService>();
builder.Services.AddScoped<TransactionImportService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddHostedService<TransactionImportJob>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.CustomSchemaIds(type => type.ToString()));
builder.Services.AddControllers();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
