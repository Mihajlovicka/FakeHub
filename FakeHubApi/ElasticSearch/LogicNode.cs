namespace FakeHubApi.ElasticSearch;

public enum NodeType { And, Or, Not, Term }

public class LogicNode
{
    public NodeType Type { get; set; }
    public string Field { get; set; } = "";
    public string Value { get; set; } = "";
    public List<LogicNode> Children { get; set; } = new List<LogicNode>();
}