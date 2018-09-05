/* TODO
*
* https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy?redirectedfrom=MSDN&view=netframework-4.7.2
* https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy.writetoserver?view=netframework-4.7.2&viewFallbackFrom=netcore-2.1
* https://docs.microsoft.com/pt-br/dotnet/framework/data/adonet/sql/single-bulk-copy-operations
* https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/bulk-copy-example-setup?view=netframework-4.7.2
* https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(System.Data.SqlClient.SqlBulkCopy);k(SolutionItemsProject);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.7);k(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.2
* https://github.com/mgravell/fast-member
*
* Update if exist
* https://stackoverflow.com/questions/12521692/c-sharp-bulk-insert-sqlbulkcopy-update-if-exists/12535726
*
* https://stackoverflow.com/questions/33027246/insert-object-or-update-if-it-already-exists-using-bulkcopy-c-sql
* https://www.databasejournal.com/features/mssql/article.php/3739131/UPSERT-Functionality-in-SQL-Server-2008.htm
* https://stackoverflow.com/questions/4889123/any-way-to-sqlbulkcopy-insert-or-update-if-exists
*
* https://github.com/Microsoft/referencesource/blob/master/System.Data/System/Data/SqlClient/SqlBulkCopy.cs
*
* https://stackoverflow.com/questions/40470357/importing-a-csv-using-sqlbulkcopy-with-asp-net-core
*
*/

/*
use [Database]
go

merge into {Employee} as Target
using {#EmployeeTemp} as Source
   on Target.id = Source.id

when matched then
    update set 
      Target.name   = Source.name,
      Target.Salary = Source.Salary

when not matched then
    insert (id, name, salary) 
    values (Source.id, Source.name, Source.Salary);

*/
