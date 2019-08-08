using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.Linq;

namespace EfPoc.Infra
{
    public class AtlasDbContext<T> : DbContext
    {
        protected AtlasDbContext(DatabaseAccessRequest request)
            : base(GetDbConnection(request), true)
        {
            // ReSharper disable once UnusedVariable - hack to include assembly in output folder
            var hack = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
        }

        protected AtlasDbContext(string request)
            : base(request)
        {
        }

        private static EntityConnection GetDbConnection(DatabaseAccessRequest request)
        {
            request.ProviderName = DatabaseProviderType.EntityClient;
            var assembly = typeof(T).Assembly;
            var manifestResourceNames = assembly.GetManifestResourceNames();
            var metadata = new List<string>();
            manifestResourceNames.ToList().ForEach(resource =>
            {
                metadata.Add($@"res://*/{resource}");
            });
            var entityFrameworkDatabaseAccessRequest = new EntityFrameworkDatabaseAccessRequest(request)
            {
                Metadata = metadata.ToArray(),
                Assembly = assembly
            };
            var metadataWorkspace = new MetadataWorkspace(entityFrameworkDatabaseAccessRequest.Metadata, new[] { entityFrameworkDatabaseAccessRequest.Assembly });
            var connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            var connectionStringInfo = new ConnectionStringInfo { ConnectionString = connectionString };
            var builder = new SqlConnectionStringBuilder(connectionStringInfo.ConnectionString)
            {
                MultipleActiveResultSets = true,
                WorkstationID = request.WorkStationId

            };
            var sqlConnection = new SqlConnection(builder.ConnectionString);
            if (!string.IsNullOrEmpty(connectionStringInfo.UserName))
            {
                sqlConnection.Credential = new SqlCredential(connectionStringInfo.UserName, connectionStringInfo.SecurePassword);
            }
            return new EntityConnection(metadataWorkspace, sqlConnection);
        }
    }
}
