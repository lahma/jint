using BenchmarkDotNet.Attributes;
using Jint.Native;
using Jint.Native.Json;

namespace Jint.Benchmark;

[MemoryDiagnoser]
public class JsonBenchmark
{
    private Engine _engine;
    private JsValue _parsedInstance;
    private string _json;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var filePath = Path.Combine(Path.GetTempPath(), "bestbuy_dataset.json");
        if (!File.Exists(filePath))
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync("https://github.com/algolia/examples/raw/master/instant-search/instantsearch.js/dataset_import/bestbuy_dataset.json");
            await using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
            await using var streamToWriteTo = File.OpenWrite(filePath);
            await streamToReadFrom.CopyToAsync(streamToWriteTo);
        }

        _json = await File.ReadAllTextAsync(filePath);

        _engine = new Engine();

        var parser = new JsonParser(_engine);
        _parsedInstance = parser.Parse(_json);
    }

    [Benchmark]
    public JsValue Parse()
    {
        var parser = new JsonParser(_engine);
        return parser.Parse(_json);
    }

    [Benchmark]
    public JsValue Stringify()
    {
        var serializer = new JsonSerializer(_engine);
        return serializer.Serialize(_parsedInstance);
    }
}
