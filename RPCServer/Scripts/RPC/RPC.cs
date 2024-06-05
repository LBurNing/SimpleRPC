using Google.Protobuf;
using System.Reflection;

namespace Game
{
    public class RPC : RPCBase
    {
        private MethodInfo _method;
        private List<DateType> _types;
        private Dictionary<int, IMessage> _param;
        private int _paramIndex;

        public RPC(MethodInfo method)
        {
            this._method = method;
            _types = new List<DateType>();
            _param = new Dictionary<int, IMessage>();
        }

        public void AddParamType(DateType type)
        {
            _types?.Add(type!);
        }

        public void AddParam(int index, IMessage message)
        {
            _param[index] = message;
        }

        public override void Decode(Role role, byte[] buffer)
        {
            base.buffer = buffer;
            base.Decode(role, buffer);
            _paramIndex = 1;
            List<object> objs = new List<object>();
            objs.Add(role);

            for (int index = 1; index < _types.Count; index++)
            {
                DateType type = _types[index];
                DateType dateType = (DateType)buffer[offset++];
                if (dateType != type)
                {
                    LogHelper.Log($"dateType bo equals, recv: {Enum.GetName(typeof(DateType), type)} != local: {Enum.GetName(typeof(DateType), dateType)}");
                }

                object obj = ToObject(dateType, ref offset);
                objs.Add(obj!);
                _paramIndex++;
            }

            _method?.Invoke(null, objs.ToArray());
        }

        private object ToObject(DateType type, ref int offset)
        {
            switch (type)
            {
                case DateType.Message:
                    IMessage message = null;
                    if (!_param!.TryGetValue(_paramIndex, out message))
                    {
                        LogHelper.Log("no find message");
                        return null;
                    }
                        
                    return ToMessage(ref offset, message);
                case DateType.Boolean:
                    return ToBoolean(ref offset);
                case DateType.Char:
                    return ToChar(ref offset);
                case DateType.SByte:
                case DateType.Byte:
                    return ToByte(ref offset);
                case DateType.Int16:
                    return ToInt16(ref offset);
                case DateType.UInt16:
                    return ToUInt16(ref offset);
                case DateType.Int32:
                    return ToInt32(ref offset);
                case DateType.UInt32:
                    return ToUInt32(ref offset);
                case DateType.Int64:
                    return ToInt64(ref offset);
                case DateType.UInt64:
                    return ToUInt64(ref offset);
                case DateType.Single:
                    return ToSingle(ref offset);
                case DateType.Double:
                    return ToDouble(ref offset);
                case DateType.String:
                    return ToString(ref offset);
                default:
                    LogHelper.Log("no find dateType: " + type);
                    break;
            }

            return null;
        }


        public override void Dispose()
        {
            _method = null;
            _types = null;
        }
    }
}