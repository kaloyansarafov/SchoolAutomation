using GCRBot.Data;

namespace Full
{
    public class DataEventArgs<T>
    {
        public T Data { get; }
        public T PreviousData { get; }
        public DataEventArgs(T data, T prevData = default(T))
        {
            Data = data;
            PreviousData = prevData;
        }
    }
}