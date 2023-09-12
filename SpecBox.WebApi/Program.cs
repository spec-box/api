using Microsoft.EntityFrameworkCore;
using SpecBox.Domain;
using SpecBox.WebApi.Model;

var builder = WebApplication.CreateBuilder(args);

string? cstring = builder.Configuration.GetConnectionString("default");
builder.Services.AddDbContext<SpecBoxDbContext>(cfg =>cfg.UseNpgsql(cstring));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<ProjectProfile>());

builder.Services.AddSwaggerGen(opts =>
{
    opts.CustomOperationIds(a => a.RelativePath);
    opts.CustomSchemaIds(a => a.FullName);
    opts.SupportNonNullableReferenceTypes();
});

builder.Logging.ClearProviders().AddConsole();

var app = builder.Build();

app.UsePathBase(app.Configuration["pathBase"]);
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
