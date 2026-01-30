using Microsoft.Extensions.Configuration;

namespace GenericReportGenerator.Shared;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration config, string key)
    {
        IConfigurationSection section = config.GetRequiredSection(key);
        if (string.IsNullOrEmpty(section.Value))
        {
            throw new InvalidOperationException($"Invalid configuration value: '{key}'");
        }

        return section.Value;
    }

    public static string[] GetRequiredArray(this IConfiguration config, string key)
    {
        IConfigurationSection section = config.GetRequiredSection(key);
        string[]? value = section.Get<string[]>();
        if (value is null || value.Length == 0)
        {
            throw new InvalidOperationException($"Invalid configuration value: '{key}'");
        }

        return value;
    }
}
