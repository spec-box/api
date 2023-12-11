using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using SpecBox.Domain;
using SpecBox.WebApi.Lib.Logging;
using SpecBox.WebApi.Model;

var builder = WebApplication.CreateBuilder(args);

string? cstring = builder.Configuration.GetConnectionString("default");
builder.Services.AddDbContext<SpecBoxDbContext>(cfg => cfg.UseNpgsql(cstring));

builder.Services.AddControllers()
    .AddJsonOptions(option =>
    {
        option.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        option.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic);
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<ProjectProfile>());

builder.Services.AddSwaggerGen(opts =>
{
    opts.CustomOperationIds(a => a.RelativePath);
    opts.CustomSchemaIds(a => a.FullName);
    opts.SupportNonNullableReferenceTypes();
});

builder.Logging
    .ClearProviders()
    .AddConsole()
    .AddConsoleFormatter<ConsoleJsonFormatter, ConsoleFormatterOptions>();

var app = builder.Build();

app.UsePathBase(app.Configuration["pathBase"]);
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
