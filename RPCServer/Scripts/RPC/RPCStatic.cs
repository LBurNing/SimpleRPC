using Google.Protobuf;
using System.Data;
using System.Net.Sockets;

namespace Game
{
    public class RPCStatic<T> : RPCBase
    {
        private Action<Role, T> _action;
        private IMessage _message;

        public RPCStatic()
        {
        }

        public virtual void Register(Action<Role, T> action, IMessage message)
        {
            this._message = message;
            this._action = action;
        }

        public override void Decode(byte[] buffer)
        {
            base.buffer = buffer;
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