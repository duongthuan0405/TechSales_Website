using Microsoft.Extensions.Configuration;

namespace CommonModule.Infrastructure.Configuration;

public static class DesignTimeConfiguration
{
    public static IConfigurationRoot Create(string[] args)
    {
        var arguments = new ConfigurationBuilder().AddCommandLine(args).Build();
        EnvironmentConfiguration.Load(
            arguments["env-file"], EnvironmentConfiguration.GetEnvironmentName(args));

        return new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
    }
}
