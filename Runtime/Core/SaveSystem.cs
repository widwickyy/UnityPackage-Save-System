namespace Widwickyy.SaveSystem
{
    public class SaveSystem : ISaveSystem
    {
        private readonly ISerializer serializer;
        private readonly IStorage storage;
        private readonly int version;
        private readonly IStringCipher cipher;

        public SaveSystem(ISerializer serializer, IStorage storage, int version = 1, IStringCipher cipher = null)
        {
            this.serializer = serializer;
            this.storage = storage;
            this.version = version;
            this.cipher = cipher;
        }

        public void Save<T>(string key, T data)
        {
            var wrapper = new SaveWrapper<T>(version, data);
            var json = serializer.Serialize(wrapper);
            if (cipher != null)
                json = cipher.Encrypt(json);

            storage.Write(key, json);
        }

        public T Load<T>(string key)
        {
            if (!storage.Exists(key))
                return default;

            var json = storage.Read(key);
            if (cipher != null)
                json = cipher.Decrypt(json);

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
}
