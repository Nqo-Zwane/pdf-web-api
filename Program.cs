using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.utils;

var builder = WebApplication.CreateBuilder(args);
ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = false,
            ValidIssuer = "WebApi",
            ValidAudience = "WebApi",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12345@manqobazwane897@nqo@nqo@nqo"))
        };
    });

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowOrigin",
                builder =>
                {
                    builder.WithOrigins("https://localhost:7032") // Replace with your frontend URL
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

// Configure MongoDB connection
var connectionString = "mongodb+srv://manqobazwane22:rt53jA7p3b6lRaQL@cluster0.wzhhvn0.mongodb.net/";
var databaseName = "Kounta";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
builder.Services.AddSingleton<PdfGenerator>();

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

// Register Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web Api", Version = "v1" });
});





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
   {
       c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wen Api");
       c.RoutePrefix = "swagger";
   });
}

app.UseRouting();

// Use the CORS policy
app.UseCors("AllowOrigin");

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Or endpoints.MapControllerRoute(...) if you use attribute routing with a specific route prefix.
});


app.Run(async context =>
{
    await context.Response.WriteAsync("Hello World!");
});

app.Run("http://localhost:5000");

