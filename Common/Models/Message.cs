namespace Common.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string MessageContent { get; set; }

        public Message(int id, string type, string message)
        {
            this.Id = id;
            this.Type = type;
            this.MessageContent = message;
        }

        public Message()
        {
            this.Id = 0;
            this.Type = string.Empty;
            this.MessageContent = string.Empty;
        }

        public string MessageText
        {
            get { return this.MessageContent; }
            set { this.MessageContent = value; }
        }
    }
}
