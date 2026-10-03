using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using gdb.Logging;
using Microsoft.Extensions.Logging;

namespace GdbWebApi.Infrastructure.Repositories
{
    public class DataBaseConnectionManager
    {
        private static readonly ILogger _logger = AppLogger.CreateLogger<DataBaseConnectionManager>();

        public static DbConnection GetConnection()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = config.GetConnectionString("GDBConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("Connection string 'GDBConnection' is missing from configuration");
                throw new InvalidOperationException("Connection string 'GDBConnection' not found.");
            }

            // Try to get provider name; prefer explicit ProviderName setting, otherwise fall back to ProviderFactory assembly name
            string providerName = config["ProviderName"];
            string providerFactory = config["ProviderFactory"];

            if (string.IsNullOrWhiteSpace(providerFactory) && string.IsNullOrWhiteSpace(providerName))
            {
                _logger.LogError("Neither ProviderFactory nor ProviderName configured");
                throw new InvalidOperationException("Database provider configuration missing.");
            }

            try
            {
                DbProviderFactory factory = null;

                if (!string.IsNullOrWhiteSpace(providerFactory))
                {
                    Type factoryType = ResolveFactoryType(providerFactory);
                    if (factoryType == null)
                    {
                        _logger.LogError("Provider factory type {FactoryTypeName} could not be loaded", providerFactory);
                        throw new InvalidOperationException("Provider factory type not found: " + providerFactory);
                    }

                    var instanceField = factoryType.GetField(
                        "Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (instanceField == null)
                    {
                        _logger.LogError("Provider factory type {FactoryTypeName} does not expose a public static Instance field", providerFactory);
                        throw new InvalidOperationException("Provider factory type does not expose Instance field: " + providerFactory);
                    }

                    factory = (DbProviderFactory)instanceField.GetValue(null);
                }
                else
                {
                    // If only a providerName is supplied, try to get factory via registered factories
                    factory = DbProviderFactories.GetFactory(providerName);
                }

                DbConnection connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;

                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create DB connection for provider {ProviderName}", providerName ?? providerFactory);
                throw;
            }
        }

        private static Type ResolveFactoryType(string factoryTypeName)
        {
            // Direct attempt
            Type type = Type.GetType(factoryTypeName);
            if (type != null) return type;

            // Known legacy mapping: System.Data.SqlClient -> Microsoft.Data.SqlClient
            if (factoryTypeName.Contains("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
            {
                string mapped = factoryTypeName.Replace("System.Data.SqlClient", "Microsoft.Data.SqlClient");
                type = Type.GetType(mapped);
                if (type != null) return type;

                // Try with assembly name explicitly set to Microsoft.Data.SqlClient
                var parts = factoryTypeName.Split(',');
                if (parts.Length > 0)
                {
                    string typeName = parts[0].Trim();
                    string alt = typeName + ", Microsoft.Data.SqlClient";
                    type = Type.GetType(alt);
                    if (type != null) return type;
                }
            }

            // Fallback: try to load assembly and get the type by name
            var p = factoryTypeName.Split(',');
            if (p.Length > 1)
            {
                string typeName = p[0].Trim();
                string assemblyName = p[1].Trim();
                try
                {
                    Assembly asm = Assembly.Load(new AssemblyName(assemblyName));
                    if (asm != null)
                    {
                        type = asm.GetType(typeName, throwOnError: false, ignoreCase: true);
                        if (type != null) return type;
                    }
                }
                catch
                {
                    // ignore and return null below
                }
            }

            return null;
        }
    }
}
