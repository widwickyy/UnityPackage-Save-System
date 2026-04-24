using System.IO;
using UnityEngine;

public class FileStorage : IStorage
{
    private string GetPath(string key)
    {
        return Path.Combine(Application.persistentDataPath, key + ".json");
    }

    public void Write(string key, string data)
    {
        File.WriteAllText(GetPath(key), data);
    }

    public string Read(string key)
    {
        return File.ReadAllText(GetPath(key));
    }

    public bool Exists(string key)
    {
        return File.Exists(GetPath(key));
    }

    public void Delete(string key)
    {
        var path = GetPath(key);
        if (File.Exists(path))
            File.Delete(path);
    }
}