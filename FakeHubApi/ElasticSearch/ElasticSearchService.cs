using Nest;

namespace FakeHubApi.ElasticSearch;

public class ElasticSearchService
{
    private readonly ElasticClient _client;

    public ElasticSearchService()
    {
        var settings = new ConnectionSettings(new Uri("http://elasticsearch:9200"))
            .DefaultIndex("fakehubapi-logs-*")
            .BasicAuthentication("elastic", "changeme")
            .DisableDirectStreaming()
            .EnableDebugMode()
            .ThrowExceptions()
            .ServerCertificateValidationCallback((o, certificate, chain, errors) => true);

        _client = new ElasticClient(settings);

        EnsureIndexExists();
    }

    public async Task<List<LogEntry>> GetAllLogsAsync(int size = 100)
    {
        var response = await _client.SearchAsync<ElasticSearchClientLog>(s => s
            .Query(q => q.MatchAll())
            .Size(Math.Min(size, 1000))
            .Sort(sort => sort.Ascending(f => f.Timestamp))
        );

        if (!response.IsValid)
            throw new Exception(response.DebugInformation);

        return response.Documents.Select(d => new LogEntry(
            timestamp: d.Timestamp.ToString(),
            level: d.Level ?? "Information",
            message: d.Message ?? "",
            exception: "", 
            application: d.Fields.Application ?? "FakeHubApi"
        )).ToList();
    }

    
    public async Task<List<LogEntry>> SearchLogsAsync(
        string? query = null,
        string? level = null,
        DateTime? from = null,
        DateTime? to = null,
        int size = 100)
    {
        QueryContainer boolQuery;

        if (!string.IsNullOrWhiteSpace(query) && LogicParser.IsLogicExpression(query))
        {
            // Ako je logički izraz, parsiraj ga
            var rootNode = LogicParser.Parse(query);
            boolQuery = BuildQueryFromLogicNode(rootNode);

            // Dodaj filtere po level i timestamp ako postoje
            var filters = new List<QueryContainer>();
            if (from.HasValue || to.HasValue)
                filters.Add(new DateRangeQuery { Field = "@timestamp", GreaterThanOrEqualTo = from, LessThanOrEqualTo = to });
            if (!string.IsNullOrWhiteSpace(level))
                filters.Add(new TermsQuery { Field = "level.keyword", Terms = level.Split(',') });

            if (filters.Count > 0)
                boolQuery = new BoolQuery { Must = new List<QueryContainer> { boolQuery }.Concat(filters).ToList() };
        }
        else
        {
            // Stara logika za jednostavan query
            boolQuery = BuildBoolQuery(query, level, from, to);
        }

        var response = await _client.SearchAsync<ElasticSearchClientLog>(s => s
            .Query(q => boolQuery)
            .Size(size)
            .Sort(sort => sort.Descending(f => f.Timestamp))
        );

        if (!response.IsValid)
            throw new Exception(response.DebugInformation);

        return response.Documents.Select(d => new LogEntry(
            timestamp: d.Timestamp.ToString(),
            level: d.Level ?? "Information",
            message: d.Message ?? "",
            exception: "",
            application: d.Fields.Application ?? "FakeHubApi"
        )).ToList();
    }

    
    
    private QueryContainer BuildBoolQuery(string query, string? level, DateTime? from, DateTime? to)
    {
        var must = new List<QueryContainer>();

        if (from.HasValue || to.HasValue)
        {
            must.Add(new DateRangeQuery
            {
                Field = "@timestamp",
                GreaterThanOrEqualTo = from,
                LessThanOrEqualTo = to
            });
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            must.Add(new TermQuery
            {
                Field = "level.keyword",
                Value = level
            });
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            must.Add(new MatchPhrasePrefixQuery
            {
                Field = "message",
                Query = query
            });
        }

        return new BoolQuery
        {
            Must = must
        };
    }

    private QueryContainer BuildBoolQuery(string query, DateTime? from, DateTime? to)
    {
        var must = new List<QueryContainer>();
        var should = new List<QueryContainer>();
        var mustNot = new List<QueryContainer>();

        if (from.HasValue || to.HasValue)
        {
            must.Add(new DateRangeQuery
            {
                Field = "@timestamp",
                GreaterThanOrEqualTo = from,
                LessThanOrEqualTo = to
            });
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            must.Add(new MatchPhrasePrefixQuery
            {
                Field = "message",
                Query = query
            });
        }

        return new BoolQuery
        {
            Must = must,
            Should = should,
            MustNot = mustNot
        };
    }
    
    private QueryContainer BuildQueryFromLogicNode(LogicNode node)
    {
        switch (node.Type)
        {
            case NodeType.And:
                return new BoolQuery { Must = node.Children.Select(BuildQueryFromLogicNode).ToList() };
            case NodeType.Or:
                return new BoolQuery { Should = node.Children.Select(BuildQueryFromLogicNode).ToList() };
            case NodeType.Not:
                return new BoolQuery { MustNot = new List<QueryContainer> { BuildQueryFromLogicNode(node.Children[0]) } };
            case NodeType.Term:
                if (node.Field.Equals("level", StringComparison.OrdinalIgnoreCase))
                    return new TermQuery { Field = "level.keyword", Value = node.Value };
                if (node.Field.Equals("message", StringComparison.OrdinalIgnoreCase))
                    return new MatchPhraseQuery { Field = "message", Query = node.Value };
                break;
        }
        return null;
    }
    
    private void EnsureIndexExists()
    {
        var indexName = "fakehubapi-logs";

        var existsResponse = _client.Indices.Exists(indexName);
        if (existsResponse.Exists) return;
        var createResponse = _client.Indices.Create(indexName, c => c
                .Map(m => m.AutoMap())
        );

        if (!createResponse.IsValid)
            throw new Exception($"Failed to create index {indexName}: {createResponse.ServerError}");
    }
}
