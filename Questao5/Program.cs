using FluentAssertions.Common;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;

using MediatR;
using Microsoft.OpenApi.Models;
using Questao5.Infrastructure.Sqlite;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

// sqlite
builder.Services.AddSingleton(new DatabaseConfig { Name = builder.Configuration.GetValue<string>("DatabaseName", "Data Source=database.sqlite") });
builder.Services.AddSingleton<IDatabaseBootstrap, DatabaseBootstrap>();





// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.ToString());
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Conta Corrente", Version = "v1" });

    c.AddSecurityDefinition("IdempotencyKey", new OpenApiSecurityScheme()
    {
        Name = "IdempotencyKey",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "IdempotencyKey",     
        In = ParameterLocation.Header,
        Description = "Informe a Chave de Idempotency. \r\n\r\n Informe o sua key no campo abaixo.\r\n\r\nPor Examplo: \"12345abcdef\"",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                              new OpenApiSecurityScheme
                              {
                                  Reference = new OpenApiReference
                                  {
                                      Type = ReferenceType.SecurityScheme,
                                      Id = "IdempotencyKey"
                                  }
                              },
                             new string[] {}
                        }
                    });
});

// Idempontent functions
//builder.Services.AddIdempotentAPI();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddIdempotentAPIUsingDistributedCache();

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

// sqlite
#pragma warning disable CS8602 // Dereference of a possibly null reference.
app.Services.GetService<IDatabaseBootstrap>().Setup();
#pragma warning restore CS8602 // Dereference of a possibly null reference.



app.Run();

// Informações úteis:
// Tipos do Sqlite - https://www.sqlite.org/datatype3.html


