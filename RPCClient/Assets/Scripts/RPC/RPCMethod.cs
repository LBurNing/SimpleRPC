using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class RPCMethod : RPCMethodAbstract
    {
        private MethodInfo _method;
        private List<DateType> _types;
        private Dictionary<int, IMessage> _param;
        private int _paramIndex;

        public RPCMethod(MethodInfo method)
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

        public override void Invoke(byte[] buffer)
        {
            base.Invoke(buffer);
            _paramIndex = 0;
            int offset = 0;
            List<object> objs = new List<object>();
            foreach (DateType type in _types)
            {
                DateType dateType = (DateType)buffer[offset++];
                if (dateType != type)
                {
                    Console.WriteLine($"dateType bo equals, recv: {Enum.GetName(typeof(DateType), type)} != local: {Enum.GetName(typeof(DateType), dateType)}");
                }

                object obj = UnpackValue(dateType, ref offset);
                objs.Add(obj!);
                _paramIndex++;
            }

            _method?.Invoke(null, objs.ToArray());
        }

        private object UnpackValue(DateType type, ref int offset)
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
                    Console.WriteLine("no find dateType: " + type);
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