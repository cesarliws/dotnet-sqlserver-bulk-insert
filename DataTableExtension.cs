using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace BulkOperations
{
    public static class DataTableExtension
    {
        public static DataTable FromList<T>(this DataTable dataTable, IList<T> list)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));

            foreach (PropertyDescriptor prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in list)
            {
                var row = dataTable.NewRow();

                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}