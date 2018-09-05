using System.Data.SqlClient;

namespace BulkOperations
{
    public static class SqlConnectionExtension
    {
        // TODO : load from configuration file
        private const string ConnectionString = "Data Source=NT-03087;Initial Catalog=Insumos;Integrated Security=False;User ID=sa;Password=sa;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=true;";

        public static void Configure(this SqlConnection connection)
        {
            connection.ConnectionString = ConnectionString;
        }
    }
}