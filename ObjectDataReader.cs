using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

namespace BulkOperations
{
    /// <inheritdoc />
    /// <summary>
    /// The available overloads of the SqlBulkCopy.WriteToServerAsync method, you can use an array, a DataTable or DbDataReader.
    /// Only the last one allows to stream the data.
    /// This is very important for the high volumetry of data I want to insert (a few million rows).
    /// https://www.softfluent.com/blog/dev/2017/02/20/Quickly-insert-millions-of-rows-in-SQL-Server-in-NET
    /// </summary>
    public class ObjectDataReader<T> : DbDataReader
    {
        private readonly IEnumerator<T> _iterator;
        private readonly IDictionary<string, int> _propertyNameToOrdinal = new Dictionary<string, int>();
        private readonly IDictionary<int, string> _ordinalToPropertyName = new Dictionary<int, string>();
        private Func<T, object>[] _getPropertyValueFuncs;

        public ObjectDataReader(IEnumerator<T> items)
        {
            _iterator = items ?? throw new ArgumentNullException(nameof(items));
            Initialize();
        }

        private void Initialize()
        {
            var properties = typeof(T).GetProperties();
            _getPropertyValueFuncs = new Func<T, object>[properties.Length];

            var ordinal = 0;
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                _propertyNameToOrdinal.Add(propertyName, ordinal);
                _ordinalToPropertyName.Add(ordinal, propertyName);

                var parameterExpression = Expression.Parameter(typeof(T), "x");
                var func = (Func<T, object>)Expression.Lambda(
                    Expression.Convert(
                        Expression.Property(parameterExpression, propertyName),
                        typeof(object)),
                        parameterExpression)
                    .Compile();

                _getPropertyValueFuncs[ordinal] = func;

                ordinal++;
            }
        }

        // required
        public override bool Read()
        {
            return _iterator.MoveNext();
        }

        public override int GetOrdinal(string name)
        {
            return (_propertyNameToOrdinal.TryGetValue(name, out var ordinal)) ? ordinal : -1;
        }

        public override bool IsDBNull(int ordinal)
        {
            return GetValue(ordinal) == null;
        }

        public override object GetValue(int ordinal)
        {
            var func = _getPropertyValueFuncs[ordinal];
            return func(_iterator.Current);
        }

        // optional
        public override int GetValues(object[] values)
        {
            var max = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < max; i++)
            {
                values[i] = IsDBNull(i) ? DBNull.Value : GetValue(i);
            }

            return max;
        }

        public override object this[int ordinal] => GetValue(ordinal);

        public override object this[string name] => GetValue(GetOrdinal(name));

        public override int Depth => 0;

        public override int FieldCount => _ordinalToPropertyName.Count;

        public override bool HasRows => true;

        public override bool IsClosed => _iterator != null;

        public override int RecordsAffected => throw new NotImplementedException();

        public override bool GetBoolean(int ordinal)
        {
            return (bool)GetValue(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return (byte)GetValue(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            return (char)GetValue(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return (decimal)GetValue(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return (double)GetValue(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int ordinal)
        {
            var value = GetValue(ordinal);
            return value == null ? typeof(object) : value.GetType();
        }

        public override float GetFloat(int ordinal)
        {
            return (float)GetValue(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return (Guid)GetValue(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return (short)GetValue(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return (int)GetValue(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return (long)GetValue(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return _ordinalToPropertyName.TryGetValue(ordinal, out var name) ? name : null;
        }

        public override string GetString(int ordinal)
        {
            return (string)GetValue(ordinal);
        }

        public override bool NextResult()
        {
            return false;
        }
    }
}