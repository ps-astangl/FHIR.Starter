using FHIR.Starter;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.InputFormatters.Insert(0, new FhirJsonInputFormatter());
    options.OutputFormatters.Insert(0, new FhirJsonOutputFormatter());
});

builder.Services.AddSingleton<ISyntheaWrapper, SyntheaWrapper>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FHIR Proxy", Version = "v1" });
    c.EnableAnnotations();
});
var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();