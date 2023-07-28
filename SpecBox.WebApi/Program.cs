using Microsoft.EntityFrameworkCore;
using SpecBox.Domain;

const string cstring = "host=localhost;port=5432;database=tms;user name=postgres;password=123";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<SpecBoxDbContext>(cfg => cfg.UseNpgsql(cstring));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.CustomOperationIds(a => a.RelativePath);
    opts.SupportNonNullableReferenceTypes();
});

builder.Logging.ClearProviders().AddConsole();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
