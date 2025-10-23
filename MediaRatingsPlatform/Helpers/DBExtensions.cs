using System;
using System.Data;

namespace MediaRatingsPlatform.Helpers;

public static class DBExtensions
{
    public static void AddParameterWithValue(this IDbCommand command, string parameterName, DbType type, object value)
    {
        var parameter = command.CreateParameter();
        parameter.DbType = type;
        parameter.ParameterName = parameterName;
        parameter.Value = value ?? DBNull.Value;

        command.Parameters.Add(parameter);
    }

    public static int? GetNullableInt32(this IDataRecord record, int ordinal)
    {
        int? value = null;
        if (!record.IsDBNull(ordinal))
        {
            value = record.GetInt32(ordinal);
        }
        return value;
    }

    public static string? GetNullableString(this IDataRecord record, int ordinal)
    {
        string? value = null;
        if (!record.IsDBNull(ordinal))
        {
            value = record.GetString(ordinal);
        }
        return value;
    }

    public static DateTime? GetNullableDateTime(this IDataRecord record, int ordinal)
    {
        DateTime? value = null;
        if (!record.IsDBNull(ordinal))
        {
            value = record.GetDateTime(ordinal);
        }
        return value;
    }
}
