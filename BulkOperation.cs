using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace BulkOperations
{
    public static class BulkOperation
    {
        public static async Task InsertAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection())
            {
                connection.Configure();
                await connection.OpenAsync(cancellationToken).ConfigureAwait(true);

                const int recordsToGenerate = 100_000;
                ConsoleLog.Write($"Gerando {recordsToGenerate:n0} registros clientes");
                var customers = Customer.Generate(recordsToGenerate);

                ConsoleLog.Write("Inserindo registros no banco de dados");
                using (var insertBulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    using (var customerReader = new ObjectDataReader<Customer>(customers.GetEnumerator()))
                    {
                        insertBulk.Configure("Customer");

                        await insertBulk.WriteToServerAsync(customerReader, cancellationToken).ConfigureAwait(false);
                    }
                }

                using (var command = new SqlCommand("", connection))
                {
                    try
                    {
                        ConsoleLog.Write("Criando tabela temporária e copiando registros");
                        command.CommandText = BulkUpdate.CreateTempTable;
                        command.ExecuteNonQuery();
                        try
                        {
                            ConsoleLog.Write("Atualizando registros: Tabela temporária -> Customers");
                            command.CommandTimeout = 300;
                            command.CommandText = BulkUpdate.UpdateTable;
                            command.ExecuteNonQuery();
                        }
                        finally
                        {
                            ConsoleLog.Write("Excluindo tabela temporária");
                            command.CommandText = BulkUpdate.DropTempTable;
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleLog.Write(e.Message);
                    }
                }
            }
        }

        private static void Configure(this SqlBulkCopy bulk, string tableName)
        {
            bulk.MapEntity(tableName);
            bulk.ConfigureOptions();
            bulk.ConfigureLog();
        }

        private static void MapEntity(this SqlBulkCopy bulk, string tableName)
        {
            bulk.DestinationTableName = tableName;
            bulk.ColumnMappings.Add(nameof(Customer.Id), "Id");
            bulk.ColumnMappings.Add(nameof(Customer.FirstName), "FirstName");
            bulk.ColumnMappings.Add(nameof(Customer.LastName), "LastName");
            bulk.ColumnMappings.Add(nameof(Customer.DateOfBirth), "DateOfBirth");
        }

        private static void ConfigureOptions(this SqlBulkCopy bulk)
        {
            bulk.EnableStreaming = true;
            bulk.BatchSize   = 10_000;
            bulk.NotifyAfter =  1_000;
        }

        private static void ConfigureLog(this SqlBulkCopy bulk)
        {
            bulk.SqlRowsCopied += (sender, e) => ConsoleLog.Write($"{DateTime.Now} RowsCopied: {e.RowsCopied:n0}");
        }
    }
}