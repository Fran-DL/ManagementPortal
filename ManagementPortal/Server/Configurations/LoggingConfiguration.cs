using System.Data;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se encarga de configurar logging.
    /// </summary>
    public static class LoggingConfiguration
    {
        /// <summary>
        /// Metodo que se encarga de setear los parametros de configuracion para logging.
        /// </summary>
        /// <param name="configuration">Interfaz para poder obtener parametros del appsettings.</param>
        public static void ConfigureLogging(IConfiguration configuration)
        {
            var columnOptions = new ColumnOptions
            {
                Store = new List<StandardColumn>
                {
                    StandardColumn.Id,
                    StandardColumn.Message,
                    StandardColumn.Level,
                    StandardColumn.TimeStamp,
                },
                AdditionalColumns = new List<SqlColumn>
                {
                    new ()
                    {
                        ColumnName = "Application",
                        DataType = SqlDbType.NVarChar,
                        DataLength = 100,
                    },
                    new ()
                    {
                        ColumnName = "IpAddress",
                        DataType = SqlDbType.NVarChar,
                        DataLength = 100,
                    },
                    new ()
                    {
                        ColumnName = "UserId",
                        DataType = SqlDbType.NVarChar,
                        DataLength = 100,
                    },
                    new ()
                    {
                        ColumnName = "Action",
                        DataType = SqlDbType.NVarChar,
                        DataLength = 100,
                    },
                },
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Filter.ByExcluding(logEvent =>
                    logEvent.Properties.ContainsKey("SourceContext") &&
                    logEvent.Properties["SourceContext"].ToString().Contains("System.Net.Http.HttpClient") &&
                    !(logEvent.MessageTemplate.Text.Contains("Sending HTTP request") ||
                    logEvent.MessageTemplate.Text.Contains("Received HTTP response"))) // Excluye todo excepto "Sending HTTP request" y "Received HTTP response"
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    sinkOptions: new MSSqlServerSinkOptions { TableName = "Log", AutoCreateSqlTable = true, },
                    columnOptions: columnOptions)
                .CreateLogger();
        }
    }
}