using Microsoft.EntityFrameworkCore;
using tms.Domain;

const string cstring = "host=localhost;port=5432;database=tms;user name=postgres;password=123";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<TmsDbContext>(cfg => cfg.UseNpgsql(cstring));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
