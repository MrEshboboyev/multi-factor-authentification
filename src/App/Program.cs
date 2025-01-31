using App.Configurations;
using App.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Install services from assemblies implementing IServiceInstaller
builder.Services
    .InstallServices(
        builder.Configuration,
        typeof(IServiceInstaller).Assembly);

// Configure Serilog for logging
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Register the global exception handling middleware in the request processing pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Map controllers to route endpoints
app.MapControllers();

app.Run();