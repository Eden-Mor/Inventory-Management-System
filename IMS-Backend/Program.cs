using IMS_Backend;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                      build => build.ConfigureDataSource(x => x.EnableDynamicJson()));
});

builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));

await StartupTasks.StartupArgsHandler(args, builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
