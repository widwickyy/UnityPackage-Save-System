public interface IStorage
{
    void Write(string key, string data);
    string Read(string key);
    bool Exists(string key);
    void Delete(string key);
}