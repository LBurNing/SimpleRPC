using Google.Protobuf;
using System.Data;

namespace Game
{
    public class RPCStaticMethod<T> : RPCMethodAbstract
    {
        private Action<Role, T> _action;
        private IMessage _message;

        public RPCStaticMethod()
        {
        }

        public virtual void Register(Action<Role, T> action, IMessage message)
        {
            this._message = message;
            this._action = action;
        }

        public override void Invoke(byte[] buffer)
        {
            base.Invoke(buffer);
            int offset = 1;
            object obj = ToString(ref offset);
            Role role = Game.gateway.GetRole((string)obj);
            DateType dateType = (DateType)buffer[offset++];

            try
            {
                if (dateType == DateType.Message)
                {
                    IMessage arg = ToMessage(ref offset, _message);
                    _action?.Invoke(role, (T)arg);
                }
                else
                {
                    LogHelper.Log($"invoke error, type != DateType.Message, type = {dateType}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.ToString());
            }
 
        }

        public override void Dispose()
        {
        }
    }
}