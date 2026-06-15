using api.Models;
using Microsoft.EntityFrameworkCore;
using api.Services;
using api.Exceptions;
using api.Settings;
using api.Helpers.Database;
using api.Mappers;

var builder = WebApplication.CreateBuilder(args);

// Database
var databaseSettingsSection = builder.Configuration.GetSection("Database");
var databaseSettings = databaseSettingsSection.Get<DatabaseSettings>() ?? throw new NotFoundSettingsDatabase();
var databaseUrl = BuildDatabaseHelper.BuildUrlString(databaseSettings);
builder.Services.Configure<DatabaseSettings>(databaseSettingsSection);
builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(databaseUrl));

// Services
builder.Services.AddScoped<CreateTransactionService>();
builder.Services.AddScoped<GetTransactionService>();
builder.Services.AddScoped<ListTransactionService>();
builder.Services.AddScoped<ListTransactionMapper>();
builder.Services.AddScoped<UpdateTransactionService>();
builder.Services.AddScoped<UploadTransactionMapper>();

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
