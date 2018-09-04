using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace BulkOperations
{
    /*
     * https://docs.microsoft.com/pt-br/dotnet/framework/data/adonet/sql/single-bulk-copy-operations
     * https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/bulk-copy-example-setup?view=netframework-4.7.2
     * TODO https://github.com/mgravell/fast-member
     */

    public static class BulkOperation
    {
        private const string ConnectionString = "Data Source=NT-03087;Initial Catalog=Insumos;Integrated Security=False;User ID=sa;Password=sa;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=true;";


        public static async Task InsertAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionString;
                await connection.OpenAsync(cancellationToken).ConfigureAwait(true);

                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    var customers = Customer.Generate(1_000_000);

                    using (var customerReader = new ObjectDataReader<Customer>(customers.GetEnumerator()))
                    {
                        bulk.MapEntity();
                        bulk.ConfigureOptions();
                        bulk.ConfigureLog();

                        await bulk.WriteToServerAsync(customerReader, cancellationToken).ConfigureAwait(true);
                    }
                }
            }
        }

        private static void MapEntity(this SqlBulkCopy bulk)
        {
            bulk.DestinationTableName = "Customer";
            bulk.ColumnMappings.Add(nameof(Customer.Id), "Id");
            bulk.ColumnMappings.Add(nameof(Customer.FirstName), "FirstName");
            bulk.ColumnMappings.Add(nameof(Customer.LastName), "LastName");
            bulk.ColumnMappings.Add(nameof(Customer.DateOfBirth), "DateOfBirth");
        }

        private static void ConfigureOptions(this SqlBulkCopy bulk)
        {
            bulk.EnableStreaming = true;
            bulk.BatchSize       = 10000;
            bulk.NotifyAfter     = 1000;
        }

        private static void ConfigureLog(this SqlBulkCopy bulk)
        {
            bulk.SqlRowsCopied += (sender, e) => Console.WriteLine($"{DateTime.Now} RowsCopied: {e.RowsCopied:n0}");
        }

    }
}
