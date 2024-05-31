using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Google.Protobuf;
using System.Data;
using System.Net.Sockets;

namespace Game
{
    class RPCMouble
    {
        private static Dictionary<int, IRPC> _msg = new Dictionary<int, IRPC>();
        private static ObjectFactory<BuffMessage> _objectFactory = new ObjectFactory<BuffMessage>();

        public static void Init()
        {
            Type type = typeof(RPCMsgHandles);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (MethodInfo methodInfo in methods)
            {
                RPC method = new RPC(methodInfo);

                int index = 0;
                ParameterInfo[] infos = methodInfo.GetParameters();
                foreach (var info in infos)
                {
                    if (typeof(IMessage).IsAssignableFrom(info.ParameterType))
                    {
                        IMessage message = Activator.CreateInstance(info.ParameterType) as IMessage;
                        method.AddParamType(DateType.Message);
                        method.AddParam(index, message);
                    }
                    else
                    {
                        DateType dateType = GetDateType(info.ParameterType);
                        method.AddParamType(dateType);
                    }

                    index++;
                }

                int hash = Globals.Hash(methodInfo.Name);
                if (_msg.ContainsKey(hash))
                    throw new Exception("register rpc methodInfo hash conflict: " + methodInfo.Name);

                _msg.Add(hash, method);
            }
        }

        public static void Register<T>(string methodName, Action<Role, T> action) where T : class, IMessage, new()
        {
            int id = Globals.Hash(methodName);
            RPCStatic<T> method = new RPCStatic<T>();
            method.Register(action, new T());

            if (_msg.ContainsKey(id))
            {
                LogHelper.Log($"repeat id, id = {id}");
            }

            _msg[id] = method;
        }

        public static void Unregister(string methodName)
        {
            int id = Globals.Hash(methodName);
            if (_msg.ContainsKey(id))
            {
                _msg.Remove(id);
            }
            else
            {
                LogHelper.Log($"no find method, id = {id}");
            }
        }

        public static BuffMessage GetBuffMessage()
        {
            lock (_objectFactory)
                return _objectFactory.Get();
        }

        public static void PutBuffMessage(BuffMessage msg)
        {
            lock (_objectFactory)
                _objectFactory.Put(msg);
        }

        public static void Call(Role role, string methodName, params object[] args)
        {
            int hash = Globals.Hash(methodName);
            BuffMessage msg = PackAll(hash, args);
            role.Send(msg);
        }

        public static void Call(Role role, string methodName, IMessage message)
        {
            if (message == null)
                return;

            try
            {
                int offset = 0;
                int id = Globals.Hash(methodName);
                BuffMessage msg = GetBuffMessage();
                BitConverter.TryWriteBytes(msg.bytes.AsSpan(offset), id);
                offset += sizeof(int);

                BitConverterHelper.WriteMessage(msg.bytes, ref offset, message);
                msg.length = offset;

                Buffer.BlockCopy(msg.bytes, 0, msg.bytes, sizeof(int), msg.length);
                BitConverter.TryWriteBytes(msg.bytes.AsSpan(0), msg.length);
                msg.length += sizeof(int);
                role.Send(msg);
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.ToString());
            }
        }

        public static void Call(string methodName, IMessage message)
        {
            if (message == null)
                return;

            try
            {
                int offset = 0;
                int id = Globals.Hash(methodName);
                BuffMessage msg = GetBuffMessage();
                BitConverter.TryWriteBytes(msg.bytes.AsSpan(offset), id);
                offset += sizeof(int);

                BitConverterHelper.WriteMessage(msg.bytes, ref offset, message);
                msg.length = offset;

                Buffer.BlockCopy(msg.bytes, 0, msg.bytes, sizeof(int), msg.length);
                BitConverter.TryWriteBytes(msg.bytes.AsSpan(0), msg.length);
                msg.length += sizeof(int);
                Game.gateway.Send(msg);
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.ToString());
            }
        }

        public static void Call(string methodName, params object[] args)
        {
            int hash = Globals.Hash(methodName);
            BuffMessage msg = PackAll(hash, args);
            Game.gateway.Send(msg);
        }

        public static void OnRPC(BuffMessage msg)
        {
            if(msg == null)
                return;

            Invoke(msg.bytes);
        }

        public static void Invoke(byte[] buffer)
        {
            if (buffer == null || buffer.Length < sizeof(int))
            {
                LogHelper.LogError("Invalid buffer received");
                return;
            }

            int protoId = BitConverter.ToInt32(buffer, 0);
            if (!_msg.TryGetValue(protoId, out IRPC method))
            {
                LogHelper.LogError($"Method not found for protoId: {protoId}");
                return;
            }

            BuffMessage buffMessage = GetBuffMessage();
            try
            {
                Array.Copy(buffer, sizeof(int), buffMessage.bytes, 0, buffer.Length - sizeof(int));
                method.Decode(buffMessage.bytes);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error invoking method for protoId {protoId}: {ex.Message}");
            }
            finally
            {
                PutBuffMessage(buffMessage);
            };
        }

        private static DateType GetDateType(Type type)
        {
            if (type == typeof(IMessage))
                return DateType.Message;
            else if (type == typeof(Int16))
                return DateType.Int16;
            else if (type == typeof(UInt16))
                return DateType.UInt16;
            else if (type == typeof(Int32))
                return DateType.Int32;
            else if (type == typeof(UInt32))
                return DateType.UInt32;
            else if (type == typeof(Int64))
                return DateType.Int64;
            else if (type == typeof(UInt64))
                return DateType.UInt64;
            else if (type == typeof(bool))
                return DateType.Boolean;
            else if (type == typeof(SByte))
                return DateType.SByte;
            else if (type == typeof(Byte))
                return DateType.Byte;
            else if (type == typeof(Char))
                return DateType.Char;
            else if (type == typeof(Double))
                return DateType.Double;
            else if (type == typeof(Single))
                return DateType.Single;
            else if (type == typeof(string))
                return DateType.String;

            return DateType.Empty;
        }

        private static BuffMessage PackAll(int id, params object[] args)
        {
            int offset = 0;
            BuffMessage msg = GetBuffMessage();
            BitConverter.TryWriteBytes(msg.bytes.AsSpan(offset), id);
            offset += sizeof(int);

            foreach (object arg in args)
            {
                try
                {
                    Type type = arg.GetType();
                    switch (arg)
                    {
                        case IMessage:
                            BitConverterHelper.WriteMessage(msg.bytes, ref offset, (IMessage)arg);
                            break;
                        case Int16:
                            BitConverterHelper.WriteInt16(msg.bytes, ref offset, (Int16)arg);
                            break;
                        case Int32:
                            BitConverterHelper.WriteInt32(msg.bytes, ref offset, (Int32)arg);
                            break;
                        case Int64:
                            BitConverterHelper.WriteInt64(msg.bytes, ref offset, (Int64)arg);
                            break;
                        case UInt16:
                            BitConverterHelper.WriteUInt16(msg.bytes, ref offset, (UInt16)arg);
                            break;
                        case UInt32:
                            BitConverterHelper.WriteUInt32(msg.bytes, ref offset, (UInt32)arg);
                            break;
                        case UInt64:
                            BitConverterHelper.WriteUInt64(msg.bytes, ref offset, (UInt64)arg);
                            break;
                        case bool:
                            BitConverterHelper.WriteBool(msg.bytes, ref offset, (bool)arg);
                            break;
                        case Byte:
                            BitConverterHelper.WriteByte(msg.bytes, ref offset, (byte)arg);
                            break;
                        case SByte:
                            BitConverterHelper.WriteByte(msg.bytes, ref offset, (byte)arg);
                            break;
                        case Char:
                            BitConverterHelper.WriteChar(msg.bytes, ref offset, (Char)arg);
                            break;
                        case Single:
                            BitConverterHelper.WriteSingle(msg.bytes, ref offset, (Single)arg);
                            break;
                        case Double:
                            BitConverterHelper.WriteDouble(msg.bytes, ref offset, (Double)arg);
                            break;
                        case string:
                            BitConverterHelper.WriteString(msg.bytes, ref offset, (string)arg);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"methodName: {id}, " + ex.ToString());
                    msg.Dispose();
                    return msg;
                }
            }

            msg.length = offset;
            Buffer.BlockCopy(msg.bytes, 0, msg.bytes, sizeof(int), msg.length);
            BitConverter.TryWriteBytes(msg.bytes.AsSpan(0), msg.length);
            msg.length += sizeof(int);
            return msg;
        }
    }

    public class BuffMessage : IObject
    {
        private const int MAX_BUFF_SIZE = 8 * 1024;
        public byte[] bytes = new byte[MAX_BUFF_SIZE];
        public int length = 0;

        public int TimeoutMillisecond
        {
            get { return length; }
        }

        public void Reset()
        {
            length = 0;
        }

        public void Dispose()
        {
            Reset();
        }
    }
}
