
namespace RosSharp.RosBridgeClient
{
    public class FloatArraySubscriber : UnitySubscriber<MessageTypes.Std.Float32MultiArray>
    {
        public float[] messageData;

        protected override void Start()
        {
            base.Start();
        }

        protected override void ReceiveMessage(MessageTypes.Std.Float32MultiArray message)
        {
            messageData = new float[message.data.Length];
            for (int i = 0; i < message.data.Length; i++)
            {
                messageData[i] = message.data[i];
            }
           
        }
    }
}