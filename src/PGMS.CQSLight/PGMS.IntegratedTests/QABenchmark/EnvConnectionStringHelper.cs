using System;
using System.IO;

namespace PGMS.IntegratedTests.QABenchmark;

/// <summary>
/// Reads the QA connection string from a .env file or environment variable.
/// The .env file must be at the repository root and is excluded from source control.
/// Alternatively, set the QA_CONNECTION_STRING environment variable.
/// </summary>
public static class EnvConnectionStringHelper
{
    private const string EnvVarName = "QA_CONNECTION_STRING";

    public static string GetConnectionString()
    {
        // 1. Try environment variable first
        var connStr = Environment.GetEnvironmentVariable(EnvVarName);
        if (!string.IsNullOrWhiteSpace(connStr))
            return connStr;

        // 2. Try .env file walking up from the test assembly directory
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        while (dir != null)
        {
            var envFile = Path.Combine(dir, ".env");
            if (File.Exists(envFile))
            {
                foreach (var line in File.ReadAllLines(envFile))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed))
                        continue;

                    var eqIndex = trimmed.IndexOf('=');
                    if (eqIndex <= 0) continue;

                    var key = trimmed.Substring(0, eqIndex).Trim();
                    var value = trimmed.Substring(eqIndex + 1).Trim();

                    if (key == EnvVarName)
                        return value;
                }
            }
            dir = Directory.GetParent(dir)?.FullName;
        }

        return null;
    }

    /// <summary>
    /// Returns true if a QA connection string is available (for conditional test skipping).
    /// </summary>
    public static bool IsAvailable() => !string.IsNullOrWhiteSpace(GetConnectionString());
}

