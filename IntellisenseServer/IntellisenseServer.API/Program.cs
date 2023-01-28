using IntellisenseServer.API.gRPC;
using IntellisenseServer.Services;
using IntellisenseServer.Services.Interfaces;
using Microsoft.IdentityModel.Logging;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    builder.Services.AddGrpcReflection();
}

builder.Services.AddControllers();

builder.Services.AddGrpc();

builder.Services.AddSingleton<IIntellisenseCodeService, IntellisenseCodeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<DynamicServiceV1>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.UseStaticFiles();

app.Run();
