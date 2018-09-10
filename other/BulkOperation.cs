using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/*
 * https://www.mssqltips.com/sqlservertip/4523/sql-server-performance-of-select-into-vs-insert-into-for-temporary-tables/
 * https://www.mssqltips.com/sqlservertip/1704/using-merge-in-sql-server-to-insert-update-and-delete-at-the-same-time/
 * https://gist.github.com/ondravondra/4001192
 * https://gist.github.com/ciarancolgan/e6b6124fc12bec4d352450f10dba7fe5
 * https://github.com/mcshaz/PicuCalendars/blob/master/PicuCalendars/DataAccess/EFExtensions.cs
 *
 */

namespace Insumos.Core.Data.Bulk
{
    /// <summary>
    /// <para>
    /// BulkOperation copia as Entidades contidas no "Enumerable" em batch utilizando "BatchSize" para a tabela "TableName"
    /// utilizando o mapeamento definido em "ColumnsMap".
    /// </para>
    /// </summary>
    /// <typeparam name="T">Entidade</typeparam>
    public class BulkOperation<T>
    {
        #region "private"
        private int    _batchSize = 10_000;
        private string _command;
        private string _connectionString;
        private bool   _enableStreaming = true;
        private int    _timeOut = 300;

        private string _tableName;
        private string _createTempTableSql;

        private IDictionary<string, string> _columnsMap;
        private IEnumerable<T> _enumerable;

        private CancellationToken _cancellationToken = default(CancellationToken);
        private SqlTransaction _transaction;

        private void ConfigureBulkCopy(SqlBulkCopy sqlBulkCopy)
        {
            sqlBulkCopy.EnableStreaming = _enableStreaming;
            sqlBulkCopy.BatchSize = _batchSize;

            if (_tableName == null) _tableName = typeof(T).Name;
            sqlBulkCopy.DestinationTableName = _tableName;

            if (_columnsMap == null) ColumnsMap();
            sqlBulkCopy.ColumnMappings.Clear();
            foreach (var columnMap in _columnsMap)
            {
                sqlBulkCopy.ColumnMappings.Add(columnMap.Key, columnMap.Value);
            }
        }

        private string BuildCreateTempTableSql()
        {
            return $"select * into #{_tableName} from {_tableName} where 0=1";
        }

        private string BuildUpdateSql()
        {
            var updateColumns = string.Join(",", _columnsMap.Select(c => $"t.{c.Value}=s.{c.Value}").ToArray());
            return $"update {_tableName} as t set {updateColumns} from #{_tableName} as s";
        }
        #endregion

        public BulkOperation<T> BatchSize(int batchSize)
        {
            _batchSize = batchSize;
            return this;
        }

        public BulkOperation<T> Command(string command)
        {
            _command = command;
            return this;
        }

        public BulkOperation<T> CommandTimeOut(int timeOut)
        {
            _timeOut = timeOut;
            return this;
        }

        public BulkOperation<T> ConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            return this;
        }

        public BulkOperation<T> CreateTempTableSql(string createTempTableSql)
        {
            _createTempTableSql = createTempTableSql;
            return this;
        }

        public BulkOperation<T> EnableStreaming(bool enableStreaming)
        {
            _enableStreaming = enableStreaming;
            return this;
        }

        public BulkOperation<T> Enumerable(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
            return this;
        }

        public BulkOperation<T> TableName(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public BulkOperation<T> ColumnsMap(IDictionary<string, string> columnsMap)
        {
            _columnsMap = columnsMap;
            return this;
        }

        public BulkOperation<T> ColumnsMap()
        {
            _columnsMap = typeof(T)
                .GetProperties()
                .ToDictionary(prop => prop.Name, prop  => prop.Name);
            return this;
        }

        public BulkOperation<T> SetCancelationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        public BulkOperation<T> SetSqlTransaction(SqlTransaction transaction)
        {
            _transaction = transaction;
            return this;
        }

        public async Task CopyAsync(SqlConnection connection)
        {
            using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, _transaction))
            {
                using (var dataReader = new ObjectDataReader<T>(_enumerable.GetEnumerator()))
                {
                    ConfigureBulkCopy(sqlBulkCopy);
                    await sqlBulkCopy.WriteToServerAsync(dataReader, _cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task InsertAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                if (_connectionString == null) connection.Configure();
                await connection.OpenAsync(_cancellationToken).ConfigureAwait(true);
                await CopyAsync(connection).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Execute um comando SQL"Update, Delete, Merge, etc.." a partir da tabela temporária '#TableName"' criada pelo BulkCopy
        /// </summary>
        /// <code>
        ///     merge into {tableName} as destino
        ///     using {#tableName} as origem
        ///         -- condição para verificar se o registro existe
        ///         on destino.Numero = origem.Numero and destino.NumeroItem = origem.NumeroItem
        ///
        ///         -- se o registro existe, definir os campos a atualizar atualizar
        ///         when matched [and destino.Fornecedor &lt;&gt; origem.Fornecedor] then
        ///         update set 
        ///             destino.Fornecedor = origem.Fornecedor ...
        ///
        ///         -- se o registro não existe incluir o novo registro
        ///         when not matched [by target] then
        ///             insert (Numero, NumeroItem, Fornecedor... ) 
        ///             values (origem.Numero, origem.NumeroItem, origem.Fornecedor...)
        ///
        ///         -- quando a condição traz um registro que não existe não origem
        ///         when not matched [by source] then
        ///             DELETE ...;
        /// </code>
        public async Task ExecuteAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                if (_connectionString == null) connection.Configure();
                await connection.OpenAsync(_cancellationToken).ConfigureAwait(true);
                await ExecuteAsync(connection).ConfigureAwait(true);
                connection.Close();
            }
        }

        public async Task ExecuteAsync(SqlConnection connection)
        {
            using (var command = new SqlCommand(string.Empty, connection))
            {
                command.CommandText = _createTempTableSql ?? BuildCreateTempTableSql();
                command.ExecuteNonQuery();

                await CopyAsync(connection).ConfigureAwait(true);

                command.CommandTimeout = _timeOut;
                command.CommandText = _command;
                await command.ExecuteNonQueryAsync(_cancellationToken).ConfigureAwait(true);
            }
        }

        public Task UpdateAsync()
        {
            _command = BuildUpdateSql();
            return ExecuteAsync();
        }

        public Task UpdateAsync(SqlConnection connection)
        {
            _command = BuildUpdateSql();
            return ExecuteAsync(connection);
        }
    }

    #region "Testes -> Remover"    
    public class AR
    {
    }

    public static class BulkOperation
    {
        public static void Test()
        {
            IEnumerable<AR> avisos = new List<AR>();

            new BulkOperation<AR>()
                .Enumerable(avisos)
                .InsertAsync()
                .Wait();


            new BulkOperation<AR>()
                .Enumerable(avisos)
                .UpdateAsync()
                .Wait();

            const string mergeAvisosSql =
                @"  
                     merge into AR as destino
                     using #AR as origem
                         on destino.Numero = origem.Numero and destino.NumeroItem = origem.NumeroItem
                
                         -- se o registro existe, definir os campos a atualizar atualizar
                         when matched and destino.Fornecedor <> origem.Fornecedor then
                         update set 
                             destino.Fornecedor = origem.Fornecedor
                
                        when not matched by target then
                             insert (Numero, NumeroItem, Fornecedor ...) 
                             values (origem.Numero, origem.NumeroItem, origem.Fornecedor ...)
                 ";

            new BulkOperation<AR>()
                .Enumerable(avisos)
                .Command(mergeAvisosSql)
                .ExecuteAsync()
                .Wait();
        }
    }
    #endregion
}
