using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game
{
    public class RPCStatic<T> : RPCBase
    {
        private Action<T> _action;
        private IMessage _message;

        public RPCStatic()
        {
        }

        public virtual void Register(Action<T> action, IMessage message)
        {
            this._message = message;
            this._action = action;
        }

        public override void Decode(byte[] buffer)
        {
            base.buffer = buffer;
            int offset = 0;
            DateType dateType = (DateType)buffer[offset++];

            try
            {
                if (dateType == DateType.Message)
                {
                    IMessage arg = ToMessage(ref offset, _message);
                    _action?.Invoke((T)arg);
                }
                else
                {
                    LogHelper.LogError($"invoke error, type != DateType.Message, type = {dateType}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.ToString());
            }
        }

        public override void Dispose()
        {
        }
    }
}