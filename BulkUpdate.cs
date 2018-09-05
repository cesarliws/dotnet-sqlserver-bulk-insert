using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BulkOperations
{
    public static class BulkUpdate
    {
        public const string TempTableName = "#Customer";
        public const string TableName = "Customer";

        public const string CreateTempTable =
            "select * into #Customer from Customer";

        public const string UpdateTable =
            @"
                update Customer
                set
                    Customer.FirstName   = #Customer.FirstName,
                    Customer.LastName    = #Customer.LastName,
                    Customer.DateOfBirth = #Customer.CreatedAt,
                    Customer.CreatedAt   = #Customer.DateOfBirth
                from #Customer
            ";

        public const string DropTempTable =
            "drop table #Customer;";

        public static void UpdateData<T>(List<T> list)
        {
            var dataTable = new DataTable(TempTableName);
            dataTable.FromList(list);

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

            using (var connection = new SqlConnection())
            {
                connection.Configure();

                using (var command = new SqlCommand("", connection))
                {
                    try
                    {
                        connection.Open();

                        command.CommandText = CreateTempTable;
                        command.ExecuteNonQuery();

                        using (var bulkcopy = new SqlBulkCopy(connection))
                        {
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = TableName;
                            bulkcopy.WriteToServer(dataTable);
                            bulkcopy.Close();
                        }

                        command.CommandTimeout = 300;
                        command.CommandText = $"{UpdateTable}{DropTempTable}";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}