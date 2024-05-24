using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game
{
    public interface IRPCMethod : IDisposable
    {
        public void Invoke(byte[] buffer);
    }

    public class RPCMethodAbstract : IRPCMethod
    {
        private byte[] buffer;

        public virtual void Invoke(byte[] buffer)
        {
            this.buffer = buffer;
        }

        protected ReadOnlySpan<byte> ReadData(DateType type, ref int offset)
        {
            ReadOnlySpan<byte> data = null;
            int length = GetLength(type);
            if (length > 0)
            {
                data = new ReadOnlySpan<byte>(buffer, offset, length);
                offset += length;
            }

            return data;
        }

        protected bool ToBoolean(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Boolean, ref offset);
            return BitConverter.ToBoolean(data);
        }

        protected Byte ToByte(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Char, ref offset);
            return data[0];
        }

        protected char ToChar(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Char, ref offset);
            return BitConverter.ToChar(data);
        }

        protected Int16 ToInt16(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Int16, ref offset);
            return BitConverter.ToInt16(data);
        }

        protected UInt16 ToUInt16(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.UInt16, ref offset);
            return BitConverter.ToUInt16(data);
        }

        protected Int32 ToInt32(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Int32, ref offset);
            return BitConverter.ToInt32(data);
        }

        protected UInt32 ToUInt32(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.UInt32, ref offset);
            return BitConverter.ToUInt32(data);
        }

        protected Int64 ToInt64(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Int64, ref offset);
            return BitConverter.ToInt64(data);
        }

        protected UInt64 ToUInt64(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.UInt64, ref offset);
            return BitConverter.ToUInt64(data);
        }

        protected Single ToSingle(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Single, ref offset);
            return BitConverter.ToSingle(data);
        }

        protected Double ToDouble(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Double, ref offset);
            return BitConverter.ToDouble(data);
        }

        protected string ToString(ref int offset)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.String, ref offset);
            return UnpackString(ref offset);
        }

        protected IMessage ToMessage(ref int offset, IMessage message)
        {
            ReadOnlySpan<byte> data = ReadData(DateType.Message, ref offset);
            return UnpackMessage(ref offset, message);
        }

        private IMessage UnpackMessage(ref int offset, IMessage message)
        {
            int length = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);

            ReadOnlySpan<byte> messageData = new ReadOnlySpan<byte>(buffer, offset, length);
            offset += length;

            return message.Descriptor.Parser.ParseFrom(messageData)!;
        }

        private string UnpackString(ref int offset)
        {
            int length = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);

            ReadOnlySpan<byte> messageData = new ReadOnlySpan<byte>(buffer, offset, length);
            offset += length;

            return Encoding.UTF8.GetString(messageData);
        }

        private static int GetLength(DateType type)
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

        public virtual void Dispose()
        {
        }
    }
}