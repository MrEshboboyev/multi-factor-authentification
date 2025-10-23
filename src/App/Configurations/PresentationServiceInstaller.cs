using Presentation;

namespace App.Configurations;

public class PresentationServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers and application part
        services
            .AddControllers()
            .AddApplicationPart(AssemblyReference.Assembly);

        // Add NSwag OpenAPI support
        services.AddOpenApiDocument(config =>
            config.PostProcess = (settings) =>
            {
                settings.Info.Title = "Multi-Factor Authentication API";
                settings.Info.Version = "v1";
                settings.Info.Description = "Advanced Multi-Factor Authentication API with TOTP, Backup Codes, and Device Management";
            });
    }
}