using GCRBot.Data;

namespace Full
{
    internal class DataEventArgs<T>
    {
        public T Data { get; }
        public DataEventArgs(T data)
        {
            Data = data;
        }
    }
}