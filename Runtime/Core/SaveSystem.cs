public class SaveSystem : ISaveSystem
{
    private readonly ISerializer serializer;
    private readonly IStorage storage;
    private readonly int version;

    public SaveSystem(ISerializer serializer, IStorage storage, int version = 1)
    {
        this.serializer = serializer;
        this.storage = storage;
        this.version = version;
    }

    public void Save<T>(string key, T data)
    {
        var wrapper = new SaveWrapper<T>(version, data);
        var json = serializer.Serialize(wrapper);
        storage.Write(key, json);
    }

    public T Load<T>(string key)
    {
        if (!storage.Exists(key))
            return default;

        var json = storage.Read(key);
        var wrapper = serializer.Deserialize<SaveWrapper<T>>(json);

        return wrapper.data;
    }

    public bool Exists(string key)
    {
        return storage.Exists(key);
    }

    public void Delete(string key)
    {
        storage.Delete(key);
    }
}