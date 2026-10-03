using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using gdb.Logging;
using Microsoft.Extensions.Logging;

namespace GdbWebApi.Infrastructure.Repositories
{
    public class DataBaseProviderRegistration
    {
        private static readonly ILogger _logger = AppLogger.CreateLogger<DataBaseProviderRegistration>();

        public static void Register()
        {
            try
            {
                RegisterProvider();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register DB provider factory");
                throw;
            }
        }

        private static void RegisterProvider()
        {
            // Build configuration from appsettings.json and environment
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            string factoryTypeName = config["ProviderFactory"];

            if (string.IsNullOrWhiteSpace(factoryTypeName))
            {
                _logger.LogError("ProviderFactory configuration is missing");
                throw new InvalidOperationException("ProviderFactory configuration is missing.");
            }

            Type factoryType = ResolveFactoryType(factoryTypeName);

            if (factoryType == null)
            {
                _logger.LogError("Provider factory type {FactoryTypeName} could not be loaded", factoryTypeName);
                throw new InvalidOperationException("Provider factory type not found: " + factoryTypeName);
            }

            var instanceField =
                factoryType.GetField(
                    "Instance",
                    BindingFlags.Public |
                    BindingFlags.Static);

            if (instanceField == null)
            {
                _logger.LogError("Provider factory type {FactoryTypeName} does not expose a public static Instance field", factoryTypeName);
                throw new InvalidOperationException("Provider factory type does not expose Instance field: " + factoryTypeName);
            }

            DbProviderFactory factory =
                (DbProviderFactory)instanceField.GetValue(null);

            // Derive provider name. Prefer explicit configuration key if present, otherwise use the assembly name from the factoryTypeName
            string providerName = config["ProviderName"];

            if (string.IsNullOrWhiteSpace(providerName))
            {
                // factoryTypeName expected in the form "Namespace.FactoryType, AssemblyName"
                var parts = factoryTypeName.Split(',');
                if (parts.Length > 1)
                {
                    providerName = parts[1].Trim();
                }
                else
                {
                    providerName = factoryType.Assembly.GetName().Name;
                }
            }

            try
            {
                DbProviderFactories.RegisterFactory(
                    providerName,
                    factory);

                _logger.LogInformation("Registered DB provider {ProviderName}", providerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register provider factory for {ProviderName}", providerName);
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
