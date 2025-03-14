using System;

namespace PubDoomer.Context;

public sealed class PubDoomerEnvironment
{
    internal const string EnvironmentKey = "environment";
    internal const string DefaultEnvironment = "production";

    internal PubDoomerEnvironment(string environmentName, string applicationName)
    {
        EnvironmentName = environmentName;
        ApplicationName = applicationName;
    }

    public string EnvironmentName { get; }
    public string ApplicationName { get; }

    public bool IsDevelopment => EnvironmentName.Equals("development", StringComparison.OrdinalIgnoreCase);
    public bool IsProduction => EnvironmentName.Equals("production", StringComparison.OrdinalIgnoreCase);
}