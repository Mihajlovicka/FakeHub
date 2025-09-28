using System.Text.RegularExpressions;
using FakeHubApi.ElasticSearch;

public static class LogicParser
{
    private static readonly Regex TermRegex = new Regex(@"(?<field>\w+):(""(?<value>[^""]+)""|(?<value>[^\s()]+))", RegexOptions.Compiled);

    public static bool IsLogicExpression(string query)
    {
        return query.Contains("AND") || query.Contains("OR") || query.Contains("NOT") || query.Contains("(");
    }

    public static LogicNode Parse(string query)
    {
        query = query.Trim();

        // Ukloni spoljne zagrade ako postoje
        if (query.StartsWith("(") && query.EndsWith(")"))
            query = query[1..^1].Trim();

        int parenLevel = 0;
        for (int i = 0; i < query.Length; i++)
        {
            if (query[i] == '(') parenLevel++;
            if (query[i] == ')') parenLevel--;
            if (parenLevel == 0)
            {
                // AND
                if (i + 3 <= query.Length && query.Substring(i, 3) == "AND")
                {
                    return new LogicNode
                    {
                        Type = NodeType.And,
                        Children = new List<LogicNode>
                        {
                            Parse(query.Substring(0, i).Trim()),
                            Parse(query.Substring(i + 3).Trim())
                        }
                    };
                }
                // OR
                if (i + 2 <= query.Length && query.Substring(i, 2) == "OR")
                {
                    return new LogicNode
                    {
                        Type = NodeType.Or,
                        Children = new List<LogicNode>
                        {
                            Parse(query.Substring(0, i).Trim()),
                            Parse(query.Substring(i + 2).Trim())
                        }
                    };
                }
            }
        }

        // NOT
        if (query.StartsWith("NOT "))
        {
            return new LogicNode
            {
                Type = NodeType.Not,
                Children = new List<LogicNode> { Parse(query[4..].Trim()) }
            };
        }

        // Term
        var match = TermRegex.Match(query);
        if (match.Success)
        {
            return new LogicNode
            {
                Type = NodeType.Term,
                Field = match.Groups["field"].Value,
                Value = match.Groups["value"].Value
            };
        }

        throw new Exception($"Cannot parse logic term: {query}");
    }
}
