using Core.Entities;
using Core.Interfaces;
using Core.Settings;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.Json.Serialization;
using Infra.Email;
using Core.Dtos;
using Infra.Data.MongoDb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

string? authenticationKey = builder.Configuration["API_AUTHENTICATION_KEY"];

if (string.IsNullOrEmpty(authenticationKey))
    throw new KeyNotFoundException("Chave 'API_AUTHENTICATION_KEY' não encontrada.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationKey))
    };
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fast Food Challenge API - Order microservice (V1.0)",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Carlos Henrique Bezerra Gonçalves",
            Email = "carlos_henrique97@outlook.com.br",
            Url = new Uri("https://www.linkedin.com/in/carlos-henrique-b-goncalves/")
        }
    });

    #region Recursos para mostrar enum no Swagger

    c.SchemaGeneratorOptions = new SchemaGeneratorOptions
    {
        UseAllOfForInheritance = true,
        UseAllOfToExtendReferenceSchemas = true
    };

    c.UseInlineDefinitionsForEnums();

    #endregion

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorização JWT utilizando o padrão Bearer. Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IDbConnection>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var rawConnectionString = config["DB_CONNECTION_STRING"];
    var databaseName = config["DB_NAME"];

    if (string.IsNullOrWhiteSpace(rawConnectionString))
        throw new KeyNotFoundException("Chave 'DB_CONNECTION_STRING' não encontrada.");

    if (string.IsNullOrEmpty(databaseName))
        throw new KeyNotFoundException("Chave 'DB_NAME' não encontrada.");

    // Keep existing initializer call for compatibility.
    DatabaseInitializer.EnsureDatabaseExists(rawConnectionString, databaseName);

    //SqlMapper.SetTypeMap(typeof(Pedido), new SnakeCaseTypeMapper<Pedido>());
    //SqlMapper.SetTypeMap(typeof(StatusPedido), new SnakeCaseTypeMapper<StatusPedido>());

    //var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(rawConnectionString)
    //{
    //    InitialCatalog = databaseName
    //};

    //var finalConnectionString = builder.ToString();

    //return new SqlServerConnection(finalConnectionString);

    return new MongoDbConnection(rawConnectionString, databaseName);
});

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<EmailSettingsDto>(builder.Configuration.GetSection("EMAIL_SETTINGS"));

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("APP_SETTINGS"));

builder.Services.AddScoped<IEmailService>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<EmailSettingsDto>>().Value;
    Console.WriteLine(JsonConvert.SerializeObject(settings, Formatting.Indented));
    return new EmailService(settings);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
