using EfPoc.EFCore;
using EfPoc.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Logging.ApplicationInsights;
namespace EfPoc.Infra
{
    public class AtlasDbContextEFCore<T> : DbContext
    {
        protected AtlasDbContextEFCore(DatabaseAccessRequest request)
            : base(GetDbConnection(request))
        {
            // ReSharper disable once UnusedVariable - hack to include assembly in output folder
            var hack = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
        }

        private static DbContextOptions GetDbConnection(DatabaseAccessRequest request)
        {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();

            var connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            optionsBuilder.UseSqlServer(connectionString);
            optionsBuilder.UseLoggerFactory(GetLoggerFactory());
            return optionsBuilder.Options;
        }

        private static ILoggerFactory GetLoggerFactory()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            //serviceCollection.AddLogging(builder =>
            //        builder.AddConsole()
            //                .AddProvider(new CustomLoggingFileProvider("c:\\Logs\\logs.txt"))
            //                .AddApplicationInsights(ConfigurationManager.AppSettings["instrumentationKey"])
            //                .AddFilter(DbLoggerCategory.Database.Command.Name,
            //                        LogLevel.Debug));
            return serviceCollection.BuildServiceProvider()
                    .GetService<ILoggerFactory>();
        }
    }
}
