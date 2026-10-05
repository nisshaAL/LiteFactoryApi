using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LiteFactoryApi.Data;

public enum LiteFactoryDatabaseProvider
{
    Sqlite,
    PostgreSql
}

public sealed record LiteFactoryDatabaseSettings(
    LiteFactoryDatabaseProvider Provider,
    string ConnectionString);

public static class DatabaseConfiguration
{
    public static LiteFactoryDatabaseSettings Resolve(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return new LiteFactoryDatabaseSettings(
                LiteFactoryDatabaseProvider.PostgreSql,
                NormalizePostgreSqlConnectionString(databaseUrl));
        }

        var sqliteConnectionString = configuration.GetConnectionString("LiteFactory")
                                     ?? "Data Source=data/litefactory.db";

        return new LiteFactoryDatabaseSettings(
            LiteFactoryDatabaseProvider.Sqlite,
            sqliteConnectionString);
    }

    public static string NormalizePostgreSqlConnectionString(string databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            throw new InvalidOperationException("DATABASE_URL is empty.");
        }

        if (databaseUrl.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            var existingBuilder = new NpgsqlConnectionStringBuilder(databaseUrl);
            EnsureSsl(existingBuilder);
            return existingBuilder.ConnectionString;
        }

        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
            !IsPostgreSqlScheme(uri.Scheme))
        {
            throw new InvalidOperationException("DATABASE_URL must be a PostgreSQL connection URI.");
        }

        if (string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException("DATABASE_URL does not contain a PostgreSQL host.");
        }

        var userInfoParts = uri.UserInfo.Split(':', 2);
        var username = userInfoParts.Length > 0
            ? Uri.UnescapeDataString(userInfoParts[0])
            : "";
        var password = userInfoParts.Length > 1
            ? Uri.UnescapeDataString(userInfoParts[1])
            : "";
        var database = uri.AbsolutePath.TrimStart('/');

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException("DATABASE_URL must contain a username and database name.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Username = username,
            Password = password,
            Database = Uri.UnescapeDataString(database)
        };

        ApplyQueryParameters(builder, uri.Query);
        EnsureSsl(builder);
        return builder.ConnectionString;
    }

    private static bool IsPostgreSqlScheme(string scheme)
    {
        return string.Equals(scheme, "postgresql", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(scheme, "postgres", StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyQueryParameters(NpgsqlConnectionStringBuilder builder, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        var trimmedQuery = query.TrimStart('?');
        foreach (var pair in trimmedQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";

            if (string.Equals(key, "sslmode", StringComparison.OrdinalIgnoreCase))
            {
                builder.SslMode = ParseSslMode(value);
            }
            else if (string.Equals(key, "channel_binding", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "channelbinding", StringComparison.OrdinalIgnoreCase))
            {
                builder.ChannelBinding = ParseChannelBinding(value);
            }
            else if (string.Equals(key, "pooling", StringComparison.OrdinalIgnoreCase) &&
                     bool.TryParse(value, out var pooling))
            {
                builder.Pooling = pooling;
            }
            else if (string.Equals(key, "connect_timeout", StringComparison.OrdinalIgnoreCase) &&
                     int.TryParse(value, out var connectTimeout))
            {
                builder.Timeout = connectTimeout;
            }
        }
    }

    private static SslMode ParseSslMode(string value)
    {
        return value.Replace("-", "", StringComparison.OrdinalIgnoreCase).ToLowerInvariant() switch
        {
            "disable" => SslMode.Disable,
            "allow" => SslMode.Allow,
            "prefer" => SslMode.Prefer,
            "require" => SslMode.Require,
            "verifyca" => SslMode.VerifyCA,
            "verifyfull" => SslMode.VerifyFull,
            _ => SslMode.Require
        };
    }

    private static ChannelBinding ParseChannelBinding(string value)
    {
        return value.Replace("_", "", StringComparison.OrdinalIgnoreCase)
            .Replace("-", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant() switch
            {
                "disable" => ChannelBinding.Disable,
                "prefer" => ChannelBinding.Prefer,
                "require" => ChannelBinding.Require,
                _ => ChannelBinding.Prefer
            };
    }

    private static void EnsureSsl(NpgsqlConnectionStringBuilder builder)
    {
        if (builder.SslMode is SslMode.Disable or SslMode.Allow or SslMode.Prefer)
        {
            builder.SslMode = SslMode.Require;
        }
    }
}
