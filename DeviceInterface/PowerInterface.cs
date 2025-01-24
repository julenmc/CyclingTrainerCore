namespace DeviceInterface
{
    public interface PowerInterface
    {
        public class CallbackData
        {
            public float Power { private set; get; }
            public float Cadence { private set; get; }
            public CallbackData(float power, float cadence)
            {
                Power = power;
                Cadence = cadence;
            }
        }

        public delegate void DataCallback(object sender, CallbackData data);

        public void SetDataCallback(DataCallback callback);

        public Task Connect();

        public Task Disconnect();
    }
}
