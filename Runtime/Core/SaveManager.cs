public static class SaveManager
{
    private static ISaveSystem _instance;
    
    public static bool IsInitialized => _instance != null;

    public static void Initialize(int version = 1)
    {
        _instance = new SaveSystem(
            new NewtonsoftSerializer(),
            new FileStorage(),
            version
        );
    }

    public static void Save<T>(string key, T data)
    {
        _instance?.Save(key, data);
    }

    public static T Load<T>(string key)
    {
        return _instance != null ? _instance.Load<T>(key) : default;
    }

    public static bool Exists(string key)
    {
        return _instance?.Exists(key) ?? false;
    }

    public static void Delete(string key)
    {
        _instance?.Delete(key);
    }
}