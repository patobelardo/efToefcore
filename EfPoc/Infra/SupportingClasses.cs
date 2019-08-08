using System.Reflection;
using System.Security;

namespace EfPoc.Infra
{
    public class ConnectionStringInfo
    {
        public string ConnectionString { get; set; }
        public string UserName { get; set; }
        public SecureString SecurePassword { get; set; }
    }

    public class DatabaseAccessRequest
    {
        public string WorkStationId { get; set; }
        public DatabaseProviderType ProviderName { get; set; } = DatabaseProviderType.SqlClient;
        public DatabaseIdentifier DatabaseIdentifier { get; set; }
    }

    public enum DatabaseProviderType
    {
        EntityClient,
        Odbc,
        OleDb,
        OracleClient,
        SqlClient,
        Sybase,
    }

    public class DatabaseIdentifier
    {
        public int TenantId { get; set; }
        public string UserName { get; set; }
        public int ConnectionStringId { get; set; }
        public DatabaseIdentifierType DatabaseIdentifierType { get; set; } = DatabaseIdentifierType.Internal;
    }

    public enum DatabaseIdentifierType
    {
        Metadata,
        Internal,
        External,
    }

    internal class EntityFrameworkDatabaseAccessRequest : DatabaseAccessRequest
    {
        public EntityFrameworkDatabaseAccessRequest(DatabaseAccessRequest databaseAccessRequest)
        {
            DatabaseIdentifier = databaseAccessRequest.DatabaseIdentifier;
            ProviderName = databaseAccessRequest.ProviderName;
            WorkStationId = databaseAccessRequest.WorkStationId;
        }
        internal string[] Metadata { get; set; }
        internal Assembly Assembly { get; set; }
    }
}
