using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FlowBlox.Core.Util;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Exceptions;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Enums;

namespace FlowBlox.Core.Util.Fields
{
    public static class FlowBloxFieldHelper
    {
        public static List<FieldElement> GetFieldElementsFromString(string value)
        {
            List<FieldElement> referencedFields = new List<FieldElement>();

            if (value == null)
                return referencedFields;

            Regex regex_GetFields = new Regex(BaseFlowBlock.Regex_FullyQualifiedFieldNames);
            MatchCollection matchCollection = regex_GetFields.Matches(value);
            foreach (Match match_Field in matchCollection)
            {
                FieldElement referencedField = FlowBloxRegistryProvider.GetRegistry().GetFieldElementOrNull(match_Field.Value);
                if (referencedField != null)
                {
                    referencedFields.Add(referencedField);
                }
            }
            return referencedFields;
        }

        public static Dictionary<string, string> ReplaceFieldsInDictionary(Dictionary<string, string> inputDictionary)
        {
            return inputDictionary.ToDictionary(
                x => ReplaceFieldsInString(x.Key),
                y => ReplaceFieldsInString(y.Value));
        }

        public static string ReplaceFieldsInString(string value)
        {
            if (value == null)
                return null;

            foreach (FieldElement fieldElement in FlowBloxRegistryProvider.GetRegistry().GetAllFields())
            {
                if (value.Contains(fieldElement.FullyQualifiedName))
                    value = value.Replace(fieldElement.FullyQualifiedName, fieldElement.StringValue ?? "");
            }
            return value;
        }

        private static string GetParameterPrefixForDbType(DbTypes dbType)
        {
            switch (dbType)
            {
                case DbTypes.Oracle:
                    return ":";
                case DbTypes.MSSQL:
                case DbTypes.MySQL:
                case DbTypes.SQLite:
                    return "@";
                default:
                    throw new NotSupportedException($"DbType {dbType} is not supported.");
            }
        }

        public static string ReplaceFieldsInSQL(string sqlStatement, DbTypes dbType, out Dictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();
            int index = 0;
            foreach (FieldElement fieldElement in FlowBloxRegistryProvider.GetRegistry().GetAllFields())
            {
                if (sqlStatement.Contains(fieldElement.FullyQualifiedName))
                {
                    string parameterKey = GetParameterPrefixForDbType(dbType) + index.ToString();
                    parameters[parameterKey] = fieldElement.Value ?? DBNull.Value;
                    sqlStatement = sqlStatement.Replace(fieldElement.FullyQualifiedName, parameterKey);
                    index++;
                }
            }
            return sqlStatement;
        }

        public static string ReplaceFQName(string value, string fqOld, string fqNew)
        {
            if (!string.IsNullOrEmpty(value) && value.Contains(fqOld))
                value = value.Replace(fqOld, fqNew);
            return value;
        }
    }
}
