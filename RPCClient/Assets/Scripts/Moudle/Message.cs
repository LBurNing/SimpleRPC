namespace Game
{
    public class Message : IMoudle
    {
        private ObjectFactory<BuffMessage> _objectFactory;

        public BuffMessage GetBuffMessage()
        {
            lock (_objectFactory)
                return _objectFactory.Get();
        }

        public void PutBuffMessage(BuffMessage msg)
        {
            lock (_objectFactory)
                _objectFactory.Put(msg);
        }

        public void Init()
        {
            _objectFactory = new ObjectFactory<BuffMessage>();
        }

        public void UnInit()
        {
            _objectFactory.Clear();
        }

        public void Update()
        {
        }
    }
}