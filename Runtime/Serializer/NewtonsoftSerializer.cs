using Newtonsoft.Json;

public class NewtonsoftSerializer : ISerializer
{
    private readonly JsonSerializerSettings settings;

    public NewtonsoftSerializer()
    {
        settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    public string Serialize<T>(T data)
    {
        return JsonConvert.SerializeObject(data, settings);
    }

    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, settings);
    }
}