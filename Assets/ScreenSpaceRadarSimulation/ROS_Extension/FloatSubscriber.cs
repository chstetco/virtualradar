namespace RosSharp.RosBridgeClient
{
    public class FloatSubscriber : UnitySubscriber<MessageTypes.Std.Float32>
    {
        public float messageData;

        protected override void Start()
        {
            base.Start();
        }

        protected override void ReceiveMessage(MessageTypes.Std.Float32 message)
        {
            messageData = message.data;
        }
    }
}