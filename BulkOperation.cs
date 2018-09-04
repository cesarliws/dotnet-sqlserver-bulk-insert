using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace BulkOperations
{
    /* TODO 
     * https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy?redirectedfrom=MSDN&view=netframework-4.7.2
     * https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy.writetoserver?view=netframework-4.7.2&viewFallbackFrom=netcore-2.1
     * https://docs.microsoft.com/pt-br/dotnet/framework/data/adonet/sql/single-bulk-copy-operations
     * https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/bulk-copy-example-setup?view=netframework-4.7.2
     * https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(System.Data.SqlClient.SqlBulkCopy);k(SolutionItemsProject);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.7);k(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.2
     * https://github.com/mgravell/fast-member
     *
     * Update if exist https://stackoverflow.com/questions/12521692/c-sharp-bulk-insert-sqlbulkcopy-update-if-exists/12535726
     * https://www.databasejournal.com/features/mssql/article.php/3739131/UPSERT-Functionality-in-SQL-Server-2008.htm
     * https://stackoverflow.com/questions/4889123/any-way-to-sqlbulkcopy-insert-or-update-if-exists
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
            // https://stackoverflow.com/questions/40470357/importing-a-csv-using-sqlbulkcopy-with-asp-net-core
            bulk.DestinationTableName = "Customer";
            bulk.ColumnMappings.Add(nameof(Customer.Id), "Id");
            bulk.ColumnMappings.Add(nameof(Customer.FirstName), "FirstName");
            bulk.ColumnMappings.Add(nameof(Customer.LastName), "LastName");
            bulk.ColumnMappings.Add(nameof(Customer.DateOfBirth), "DateOfBirth");
        }

        private static void ConfigureOptions(this SqlBulkCopy bulk)
        {
            bulk.EnableStreaming = true;
            bulk.BatchSize       = 10_000;
            bulk.NotifyAfter     =  1_000;
        }

        private static void ConfigureLog(this SqlBulkCopy bulk)
        {
            bulk.SqlRowsCopied += (sender, e) => Console.WriteLine($"{DateTime.Now} RowsCopied: {e.RowsCopied:n0}");
        }

    }
}
