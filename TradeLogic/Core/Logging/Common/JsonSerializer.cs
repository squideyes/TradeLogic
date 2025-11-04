using System;
using System.Collections.Generic;
using System.Text;

namespace TradeLogic
{
    internal static class JsonSerializer
    {
        public static string Serialize(Dictionary<string, object> data)
        {
            var sb = new StringBuilder();

            sb.Append("{");

            var first = true;

            foreach (var kv in data)
            {
                if (!first)
                    sb.Append(",");

                sb.Append("\"");
                sb.Append(kv.Key);
                sb.Append("\":");

                AppendJsonValue(sb, kv.Value);

                first = false;
            }

            sb.Append("}");

            return sb.ToString();
        }

        private static void AppendJsonValue(StringBuilder sb, object value)
        {
            if (value == null)
            {
                sb.Append("null");
            }
            else if (value is string s)
            {
                sb.Append("\"").Append(EscapeJsonString(s)).Append("\"");
            }
            else if (value is DateTime d)
            {
                sb.Append("\"").Append(d.ToString("O")).Append("\"");
            }
            else if (value is bool b)
            {
                sb.Append(b ? "true" : "false");
            }
            else if (value is int
                || value is long
                || value is double
                || value is decimal)
            {
                sb.Append(value);
            }
            else
            {
                sb.Append("\"").Append(EscapeJsonString(
                    value.ToString())).Append("\"");
            }
        }

        private static string EscapeJsonString(string str)
        {
            return str.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}

