using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class FloatArrayPublisher : UnityPublisher<MessageTypes.Std.Float32MultiArray>
    {
        [HideInInspector]
        public float[] messageData;

        private MessageTypes.Std.Float32MultiArray message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Float32MultiArray
            {

                data = messageData
            };
        }

        private void Update()
        {
            InitializeMessage();
            for (int i = 0; i < messageData.Length; i++)
            {
                message.data[i] = messageData[i];
            }

            Publish(message);
        }
    }
}