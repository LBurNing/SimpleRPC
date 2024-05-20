using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Google.Protobuf;

namespace Game
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RecvAttribute : Attribute
    {
    }

    class RPC
    {
        private static Dictionary<int, Method> _msg = new Dictionary<int, Method>();
        private static ObjectFactory<BuffMessage> _objectFactory = new ObjectFactory<BuffMessage>();
        private static readonly int TA = 63689;
        private static readonly int TB = 378551;

        public static void Init()
        {
            Type type = typeof(RPCMsgHandles);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (MethodInfo methodInfo in methods)
            {
                if (!methodInfo.IsDefined(typeof(RecvAttribute)))
                    continue;

                Method method = new Method();
                method.methodInfo = methodInfo;

                int index = 0;
                ParameterInfo[] infos = methodInfo.GetParameters();
                foreach (var info in infos)
                {
                    if (typeof(IMessage).IsAssignableFrom(info.ParameterType))
                    {
                        IMessage message = Activator.CreateInstance(info.ParameterType) as IMessage;
                        method.Add(DateType.Message);
                        method.messages![index] = message!;
                    }
                    else
                    {
                        DateType dateType = GetDateType(info.ParameterType);
                        method.Add(dateType);
                    }

                    index++;
                }

                int hash = Hash(methodInfo.Name);
                if (_msg.ContainsKey(hash))
                    throw new Exception("register rpc methodInfo hash conflict: " + methodInfo.Name);

                _msg.Add(hash, method);
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

        public static void Call(string id, params object[] args)
        {
            int hash = Hash(id);
            BuffMessage msg = PackAll(hash, args);
            Game.Socket.Send(msg);
        }

        public static void OnRPC(BuffMessage msg)
        {
            if(msg == null)
                return;

            UnPack(msg.bytes);
        }

        public static void UnPack(byte[] buffer)
        {
            int protoId = ((buffer[3] & 0xFF) << 24) | ((buffer[2] & 0xFF) << 16) | ((buffer[1] & 0xFF) << 8) | (buffer[0] & 0xFF);
            Method method = null;
            if (!_msg.TryGetValue(protoId, out method))
            {
                Console.WriteLine("no find method: " + protoId);
                return;
            }

            BuffMessage buffMessage = GetBuffMessage();
            Array.Copy(buffer, sizeof(int), buffMessage.bytes, 0, buffer.Length - sizeof(int));
            method.Invoke(buffMessage.bytes);
            PutBuffMessage(buffMessage);
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
                    Console.WriteLine($"id: {id}, " + ex.ToString());
                    msg.Dispose();
                    return msg;
                }
            }

            msg.length = offset;
            return msg;
        }

        private static int Hash(string id)
        {
            int seed = TA;
            int hash = 0;
            foreach (char c in id)
            {
                hash = hash * seed + c;
                seed *= TB;
            }

            return hash;
        }
    }

    public class BuffMessage : IObject
    {
        private const int MAX_BUFF_SIZE = 8 * 1024;
        public byte[] bytes = new byte[MAX_BUFF_SIZE];
        public int length = 0;

        public void Reset()
        {
            length = 0;
        }

        public void Dispose()
        {
            Reset();
        }
    }

    class Method
    {
        public MethodInfo methodInfo;
        public List<DateType> types;
        public Dictionary<int, IMessage> messages;

        public Method()
        {
            types = new List<DateType>();
            messages = new Dictionary<int, IMessage>();
        }

        public void Add(DateType type)
        {
            types?.Add(type!);
        }

        public void Invoke(byte[] buffer)
        {
            if (types == null)
                return;

            int offset = 0;
            List<object> objs = new List<object>();
            for (int index = 0; index < types.Count; index++)
            {
                DateType type = types[index];
                DateType dateType = (DateType)buffer[offset++];
                if (dateType != type)
                {
                    Console.WriteLine($"dateType bo equals, recv: {Enum.GetName(typeof(DateType), type)} != local: {Enum.GetName(typeof(DateType), dateType)}");
                }

                object obj = UnpackValue(buffer, dateType, ref offset, index);
                objs.Add(obj!);
            }

            methodInfo?.Invoke(null, objs.ToArray());
        }

        private object UnpackValue(byte[] buffer, DateType type, ref int offset, int index)
        {
            object obj = null;
            ReadOnlySpan<byte> data = null;
            int length = GetLength(type);
            if (length > 0)
            {
                data = new ReadOnlySpan<byte>(buffer, offset, length);
                offset += length;
            }

            switch (type)
            {
                case DateType.Message:
                    IMessage message = null;
                    if (!messages!.TryGetValue(index, out message))
                        Console.WriteLine("no find message");
                    else
                        obj = UnpackMessage(buffer, ref offset, message);

                    break;
                case DateType.Boolean:
                    obj = BitConverter.ToBoolean(data);
                    break;
                case DateType.Char:
                    obj = BitConverter.ToChar(data);
                    break;
                case DateType.SByte:
                case DateType.Byte:
                    obj = data[0];
                    break;
                case DateType.Int16:
                    obj = BitConverter.ToInt16(data);
                    break;
                case DateType.UInt16:
                    obj = BitConverter.ToUInt16(data);
                    break;
                case DateType.Int32:
                    obj = BitConverter.ToInt32(data);
                    break;
                case DateType.UInt32:
                    obj = BitConverter.ToUInt32(data);
                    break;
                case DateType.Int64:
                    obj = BitConverter.ToInt64(data);
                    break;
                case DateType.UInt64:
                    obj = BitConverter.ToUInt64(data);
                    break;
                case DateType.Single:
                    obj = BitConverter.ToSingle(data);
                    break;
                case DateType.Double:
                    obj = BitConverter.ToDouble(data);
                    break;
                case DateType.String:
                    obj = UnpackString(buffer, ref offset);
                    break;
                default:
                    Console.WriteLine("no find dateType: " + type);
                    break;
            }

            return obj!;
        }

        private object UnpackMessage(byte[] buffer, ref int offset, IMessage message)
        {
            int length = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);

            ReadOnlySpan<byte> messageData = new ReadOnlySpan<byte>(buffer, offset, length);
            offset += length;

            return message.Descriptor.Parser.ParseFrom(messageData)!;
        }

        private object UnpackString(byte[] buffer, ref int offset)
        {
            int length = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);

            ReadOnlySpan<byte> messageData = new ReadOnlySpan<byte>(buffer, offset, length);
            offset += length;

            return Encoding.UTF8.GetString(messageData);
        }

        public static int GetLength(DateType type)
        {
            switch (type)
            {
                case DateType.Boolean:
                    return sizeof(bool);
                case DateType.Char:
                    return sizeof(char);
                case DateType.SByte:
                case DateType.Byte:
                    return sizeof(byte);
                case DateType.Int16:
                    return sizeof(Int16);
                case DateType.UInt16:
                    return sizeof(UInt16);
                case DateType.Int32:
                    return sizeof(Int32);
                case DateType.UInt32:
                    return sizeof(UInt32);
                case DateType.Int64:
                    return sizeof(Int64);
                case DateType.UInt64:
                    return sizeof(UInt64);
                case DateType.Single:
                    return sizeof(Single);
                case DateType.Double:
                    return sizeof(double);
            }

            return -1;
        }


        public void Dispose()
        {
            methodInfo = null;
            types = null;
        }
    }
}
