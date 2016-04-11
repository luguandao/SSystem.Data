using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSystem.Data
{
    public static class DataReaderExtension
    {
        public static bool GetBoolean(this IDataReader reader, string columnName) =>
            reader.GetBoolean(reader.GetOrdinal(columnName));
        public static byte GetByte(this IDataReader reader, string columnName) =>
            reader.GetByte(reader.GetOrdinal(columnName));
        public static char GetChar(this IDataReader reader, string columnName) =>
            reader.GetChar(reader.GetOrdinal(columnName));
        public static DateTime GetDateTime(this IDataReader reader, string columnName) =>
            reader.GetDateTime(reader.GetOrdinal(columnName));
        public static decimal GetDecimal(this IDataReader reader, string columnName) =>
            reader.GetDecimal(reader.GetOrdinal(columnName));
        public static double GetDouble(this IDataReader reader, string columnName) =>
            reader.GetDouble(reader.GetOrdinal(columnName));
        public static float GetFloat(this IDataReader reader, string columnName) =>
            reader.GetFloat(reader.GetOrdinal(columnName));
        public static Guid GetGuid(this IDataReader reader, string columnName) =>
            reader.GetGuid(reader.GetOrdinal(columnName));
        public static short GetInt16(this IDataReader reader, string columnName) =>
            reader.GetInt16(reader.GetOrdinal(columnName));
        public static int GetInt32(this IDataReader reader, string columnName) =>
            reader.GetInt32(reader.GetOrdinal(columnName));
        public static long GetInt64(this IDataReader reader, string columnName) =>
            reader.GetInt64(reader.GetOrdinal(columnName));
        public static string GetString(this IDataReader reader, string columnName) =>
            reader.GetString(reader.GetOrdinal(columnName));
        public static bool IsDBNull(this IDataReader reader, string columnName) =>
            reader.IsDBNull(reader.GetOrdinal(columnName));
    }
}
