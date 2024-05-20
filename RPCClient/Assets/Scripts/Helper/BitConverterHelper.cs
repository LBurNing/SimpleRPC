using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Game
{
    public static class BitConverterHelper
    {
        private static readonly int BUFFER_SIZE = 1024 * 1024;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private static CodedOutputStream _stream;
        private static Stopwatch _watch;

        public static void Init()
        {
            CreateStream();
            _watch = new Stopwatch();
            _watch.Start();
        }

        private static void CreateStream()
        {
            if (_stream != null)
                _stream.Dispose();

            if (_watch != null)
            {
                _watch.Stop();
                LogHelper.LogWarning($"create stream interval time: {_watch.ElapsedMilliseconds / 1000.0f} s");
                _watch.Restart();
            }

            _stream = new CodedOutputStream(buffer);
        }

        private static Span<byte> ToByteArray(IMessage message)
        {
            if (message == null)
                return new byte[0];

            int length = message.CalculateSize();
            if (length == 0)
                return new byte[0];

            if (length >= BUFFER_SIZE)
            {
                throw new Exception($"overflow: message length >= {BUFFER_SIZE}");
            }

            if (_stream.Position + length >= BUFFER_SIZE)
                CreateStream();

            int position = (int)_stream.Position;
            message.WriteTo(_stream);
            return buffer.AsSpan(position, length);
        }

        public static void WriteInt16(byte[] buffer, ref int offset, Int16 arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Int16;
            Check(buffer, offset + sizeof(Int16));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(Int16);
        }

        public static void WriteInt32(byte[] buffer, ref int offset, Int32 arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Int32;
            Check(buffer, offset + sizeof(Int32));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(Int32);
        }

        public static void WriteInt64(byte[] buffer, ref int offset, Int64 arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Int64;
            Check(buffer, offset + sizeof(Int64));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(Int64);
        }

        public static void WriteUInt16(byte[] buffer, ref int offset, UInt16 arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.UInt16;
            Check(buffer, offset + sizeof(UInt16));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(UInt16);
        }

        public static void WriteUInt32(byte[] buffer, ref int offset, UInt32 arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.UInt32;
            Check(buffer, offset + sizeof(UInt32));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(UInt32);
        }

        public static void WriteUInt64(byte[] buffer, ref int offset, UInt64 arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.UInt64;
            Check(buffer, offset + sizeof(UInt64));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(UInt64);
        }

        public static void WriteBool(byte[] buffer, ref int offset, bool arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Boolean;
            Check(buffer, offset + sizeof(bool));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(bool);
        }

        public static void WriteByte(byte[] buffer, ref int offset, byte arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Byte;
            Check(buffer, offset + 1);
            buffer[offset++] = arg;
        }

        public static void WriteChar(byte[] buffer, ref int offset, Char arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Char;
            Check(buffer, offset + sizeof(Char));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(Char);
        }

        public static void WriteSingle(byte[] buffer, ref int offset, Single arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Single;
            Check(buffer, offset + sizeof(Single));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(Single);
        }

        public static void WriteDouble(byte[] buffer, ref int offset, Double arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Double;
            Check(buffer, offset + sizeof(Double));
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), arg);
            offset += sizeof(Double);
        }

        public static void WriteString(byte[] buffer, ref int offset, string arg)
        {
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.String;
            byte[] bytes = Encoding.UTF8.GetBytes(arg);
            Check(buffer, offset + bytes.Length);
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), bytes.Length);
            offset += sizeof(int);
            Span<byte> target = new Span<byte>(buffer, offset, buffer.Length - offset);
            bytes.CopyTo(target);
            offset += bytes.Length;
        }

        public static void WriteMessage(byte[] buffer, ref int offset, IMessage arg)
        {
            IMessage message = arg;
            Span<byte> bytes = ToByteArray(message);
            Check(buffer, offset + 1);
            buffer[offset++] = (byte)DateType.Message;
            Check(buffer, offset + bytes.Length);
            BitConverter.TryWriteBytes(buffer.AsSpan(offset), bytes.Length);
            offset += sizeof(int);
            Span<byte> target = new Span<byte>(buffer, offset, bytes.Length);
            bytes.CopyTo(target);
            offset += bytes.Length;
        }

        private static void Check(byte[] buffer, int offset)
        {
            if (offset >= buffer.Length)
                throw new Exception($"date length: {offset} > {Globals.DATA_SZIE}, Invalid data!!");
        }

        public static void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}