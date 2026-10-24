using System.Text;

namespace FlowBlox.Core.Util
{
    internal static class SqlUnquotedTokensReader
    {
        public static IEnumerable<string> Read(string? sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                yield break;

            for (var index = 0; index < sql.Length;)
            {
                if (index + 1 < sql.Length && sql[index] == '-' && sql[index + 1] == '-')
                {
                    index += 2;
                    while (index < sql.Length && sql[index] != '\r' && sql[index] != '\n')
                        index++;
                    continue;
                }

                if (index + 1 < sql.Length && sql[index] == '/' && sql[index + 1] == '*')
                {
                    index += 2;
                    while (index + 1 < sql.Length && (sql[index] != '*' || sql[index + 1] != '/'))
                        index++;
                    index = Math.Min(sql.Length, index + 2);
                    continue;
                }

                var quote = sql[index];
                if (quote is '\'' or '"' or '`' or '[')
                {
                    var closingQuote = quote == '[' ? ']' : quote;
                    index++;
                    while (index < sql.Length)
                    {
                        if (sql[index] != closingQuote)
                        {
                            index++;
                            continue;
                        }

                        if (quote != '[' && index + 1 < sql.Length && sql[index + 1] == closingQuote)
                        {
                            index += 2;
                            continue;
                        }

                        index++;
                        break;
                    }
                    continue;
                }

                if (!char.IsLetter(sql[index]))
                {
                    index++;
                    continue;
                }

                var token = new StringBuilder();
                while (index < sql.Length && (char.IsLetterOrDigit(sql[index]) || sql[index] == '_'))
                    token.Append(sql[index++]);
                yield return token.ToString();
            }
        }
    }
}
