namespace RosSharp.RosBridgeClient
{
    public class FloatPublisher : UnityPublisher<MessageTypes.Std.Float32>
    {
        public float messageData;

        private MessageTypes.Std.Float32 message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Float32
            {
                data = messageData
            };
        }

        private void Update()
        {
            message.data = messageData;
            Publish(message);
        }
    }
}