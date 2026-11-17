using DotNetEnv;
using Microsoft.Extensions.Configuration;

namespace CommonModule.Infrastructure.Configuration;
public static class EnvironmentConfiguration
{
    public static string EnvFilePath
    {
        get
        {
            foreach (var start in new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            }

            )
            {
                for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
                {
                    if (File.Exists(Path.Combine(directory.FullName, "TechSalesBE.WebAPI.csproj")))
                    {
                        return Path.Combine(directory.FullName, ".env");
                    }

                    if (File.Exists(Path.Combine(directory.FullName, "TechSalesBE.slnx")))
                    {
                        return Path.Combine(directory.FullName, "TechSalesBE.WebAPI", ".env");
                    }
                }
            }

            return Path.Combine(AppContext.BaseDirectory, ".env");
        }
    }

    public static string GetEnvironmentName(string[] args)
    {
        var arguments = new ConfigurationBuilder().AddCommandLine(args).Build();
        return arguments["environment"] ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    }

    public static void Load(string? envFile = null, string? environmentName = null)
    {
        environmentName ??= GetEnvironmentName(Array.Empty<string>());
        if (!string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var explicitFile = !string.IsNullOrWhiteSpace(envFile);
        var path = explicitFile ? Path.GetFullPath(envFile!) : EnvFilePath;
        if (File.Exists(path))
        {
            Env.NoClobber().Load(path);
        }
        else if (explicitFile)
        {
            throw new FileNotFoundException("The requested .env file does not exist.", path);
        }
    }
}
