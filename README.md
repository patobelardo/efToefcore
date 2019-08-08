# Steps

Here are the requirements for converting from EF to EF.CORE POC:

## Database First.

To implement Database first approach, you should add the **Microsoft.EntityFrameworkCore.SqlServer** nuget package.

After that, you need to execute scaffold. Here are the steps:

````powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools

Scaffold-DbContext "Server=tcp:efpocpb.database.windows.net,1433;Initial Catalog=EFPoc;Persist Security Info=False;User ID=user;Password=pass;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -UseDatabaseNames -Force
````

## Using custom context (AtlasDbContext) class which is derived from EF context (DbContext)



````csharp
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
}
````

## Capturing the time to perform EF Core operation 

**Notes from the original requirement:** *"We will send to plugin to store the data to analyze later on like WCF telemetry we talked about."*

In this case we are sending information to different destinations:

- Console
- Application Insights
- Custom Logging Provider 

````csharp
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
    serviceCollection.AddLogging(builder =>
            builder .AddConsole()
                    .AddApplicationInsights(ConfigurationManager.AppSettings["instrumentationKey"])
                    .AddProvider(new CustomLoggingFileProvider("c:\\Logs\\logs.txt"))
                    .AddFilter(DbLoggerCategory.Database.Command.Name, 
                            LogLevel.Debug)); 
    return serviceCollection.BuildServiceProvider()
            .GetService<ILoggerFactory>();
}
````
**Log information**

````text
Information :: Microsoft.EntityFrameworkCore.Database.Command :: Executed DbCommand (20ms) [Parameters=[@p0='?' (DbType = DateTime2), @p1='?' (DbType = Int64), @p2='?' (Size = 4000), @p3='?' (Size = 4000), @p4='?' (DbType = DateTime2)], CommandType='Text', CommandTimeout='30']
SET NOCOUNT ON;
INSERT INTO [Spans] ([EndDate], [MemberId], [SpanType], [SpanValue], [StartDate])
VALUES (@p0, @p1, @p2, @p3, @p4);
SELECT [Id]
FROM [Spans]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity(); :: username :: 8/8/2019 3:38:30 PM
````

**AddConsole**

>In order to use the AddConsole() method, you need to include **Microsoft.Extensions.Logging.Console** package from Nuget

**Application Insights**


To use the AddApplicationInsights extension method, you should include **using Microsoft.Extensions.Logging.ApplicationInsights;**

**Custom Logging Provider**

Here is an example of a custom logging provider implementation:

````csharp
public class CustomLoggingFileProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    public CustomLoggingFileProvider(string logFilePath)
    {
        _logFilePath = logFilePath;
    }
    public ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger(categoryName, _logFilePath);
    }

    public void Dispose()
    {
    }

    public class CustomLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _path;

        public CustomLogger(string categoryName, string logFilePath)
        {
            _path = logFilePath;
            _categoryName = categoryName;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                RecordMsg(logLevel, eventId, state, exception, formatter);
            }
            catch (Exception ex)
            {
                //this is being used in case of error 'the process cannot access the file because it is being used by another process', could not find a better way to resolve the issue
                RecordMsg(logLevel, eventId, state, exception, formatter);
            }
        }

        private void RecordMsg<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string msg = $"{logLevel} :: {_categoryName} :: {formatter(state, exception)} :: username :: {DateTime.Now}";

            using (var writer = File.AppendText(_path))
            {
                writer.WriteLine(msg);
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
````


References and examples:

- [Custom Logging Providers](https://stackoverflow.com/questions/38616771/asp-net-core-iloggerprovider-for-database)
- [Use of Logging Factory](http://thedatafarm.com/data-access/logging-in-ef-core-2-2-has-a-simpler-syntax-more-like-asp-net-core/)
- [Application Insights Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging.ApplicationInsights)


## Covers Bulk Insert and Bulk Update on top of SaveChanges API

Already implemented at the received example. (or you want to cover a different scenario?)

## Transaction support

This was implemented in the same way included at the sample project.

````csharp
using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
{
    using (var transaction = context.Database.BeginTransaction(IsolationLevel.RepeatableRead)) 
    {
        try
        {
            var member = new Members
            {
                FirstName = "FirstName",
                LastName = "LastName",
                HIC = "HIC0001",
                PlanID = "Plan",
                PBP = "PBP",
                SegmentID = "SEG",
                CurrentEffDate = DateTime.Now
            };
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var span = new Models.Spans
            {
                MemberId = member.Id,
                SpanType = "SECD",
                SpanValue = "123",
                StartDate = DateTime.Now
            };
            context.Spans.Add(span);
            await context.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            transaction.Rollback();
        }
    }
}
````

## There is way to know if developers re not using the Async API, ex: If they use ToList() instead of ToListAsync()

- [AsyncFixer](https://marketplace.visualstudio.com/items?itemName=SemihOkur.AsyncFixer)
- [VSTHRD103 Call async methods when in an async method](https://github.com/Microsoft/vs-threading/blob/master/doc/analyzers/VSTHRD103.md)
