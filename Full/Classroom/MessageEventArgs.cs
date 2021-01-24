using GCRBot.Data;

namespace Full
{
    internal class MessageEventArgs
    {
        public Message Message { get; }
        public MessageEventArgs(Message msg)
        {
            Message = msg;
        }
    }
}