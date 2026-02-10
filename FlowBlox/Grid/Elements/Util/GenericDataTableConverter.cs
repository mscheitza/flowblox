using FlowBlox.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;

namespace FlowBlox.Grid.Elements.Util
{
    public class GenericDataTableConverter
    {
        public static DataTable ConvertToDataTable(IList list, out Dictionary<object, DataRow> assignments)
        {
            var dt = new DataTable();

            var properties = GetOrderedProperties(list.GetType().GetGenericArguments()[0].GetProperties());

            foreach (var prop in properties)
            {
                var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                if (underlyingType != null)
                    dt.Columns.Add(prop.Name, underlyingType);
                else
                    dt.Columns.Add(prop.Name, prop.PropertyType);
            }

            assignments = new Dictionary<object, DataRow>();
            if (list == null || list.Count == 0)
                return dt;

            foreach (var item in list)
            {
                var values = new object[properties.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }
                dt.Rows.Add(values);
                assignments[item] = dt.Rows[dt.Rows.Count - 1];
            }

            return dt;
        }

        private static PropertyInfo[] GetOrderedProperties(PropertyInfo[] propertyInfos)
        {
            return propertyInfos
                .Select(p => new
                {
                    PropertyInfo = p,
                    Order = p.GetCustomAttribute<DisplayAttribute>()?.GetOrder() ?? int.MaxValue
                })
                .OrderBy(o => o.Order)
                .Select(o => o.PropertyInfo)
                .ToArray();
        }

        public static IList ConvertToList(DataTable dt, Type listType, Dictionary<object, DataRow> originAssignments) 
        {
            return ConvertToList(dt, listType, out _, originAssignments);
        }

        public static IList ConvertToList(DataTable dt, Type listType, out Dictionary<DataRow, object> assignments, Dictionary<object, DataRow> originAssignments = null)
        {
            assignments = new Dictionary<DataRow, object>();

            var genericType = typeof(List<>).MakeGenericType(new[] { listType });
            var list = Activator.CreateInstance(genericType);
            var properties = listType.GetProperties();

            var originAssignmentsReversed = originAssignments?.ReverseDictionary();

            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState == DataRowState.Detached)
                    continue;

                object item;
                if (originAssignmentsReversed?.TryGetValue(row, out object sourceObject) == true)
                    item = sourceObject;
                else
                    item = Activator.CreateInstance(listType);

                assignments[row] = item;
                foreach (var prop in properties)
                {
                    if (dt.Columns.Contains(prop.Name))
                    {
                        if (row[prop.Name] != DBNull.Value && prop.CanWrite)
                        {
                            prop.SetValue(item, row[prop.Name]);
                        }
                    }
                }
                genericType.GetMethod(nameof(IList.Add)).Invoke(list, new object[] {item} );
            }

            return (IList)list;
        }
    }
}
