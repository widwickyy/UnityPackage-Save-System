namespace Widwickyy.SaveSystem
{
    [System.Serializable]
    public class SaveWrapper<T>
    {
        public int version;
        public T data;

        public SaveWrapper(int version, T data)
        {
            this.version = version;
            this.data = data;
        }
    }
}
