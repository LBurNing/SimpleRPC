using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using System;
using System.Threading;
using UnityEngine.Profiling;
using System.Net;

namespace Game
{
    public class Udp
    {
        private Queue<BuffMessage> _sendMsgs;
        private Queue<BuffMessage> _receiveMsgs;
        private UdpClient _udpClient;
        private byte[] _recvBuff;
        private int _recvOffset;
        private int _delay = 10;
        public string IP { get; set; }
        public int Port { get; set; }

        public Udp()
        {
            _sendMsgs = new Queue<BuffMessage>();
            _receiveMsgs = new Queue<BuffMessage>();
            _udpClient = new UdpClient();
            _recvBuff = new byte[Globals.BUFFER_SIZE];
        }

        public void Update()
        {
            Profiler.BeginSample("on udp rpc");
            lock (_receiveMsgs)
            {
                if (_receiveMsgs.Count > 0)
                {
                    BuffMessage msg = _receiveMsgs.Dequeue();
                    RPCMoudle.OnRPC(msg);
                    GameFrame.message.PutBuffMessage(msg);
                }
            }
            Profiler.EndSample();
        }

        public void Connect(string ip, int port)
        {
            IP = ip;
            Port = port;
            Connect();
        }

        public void Connect()
        {
            try
            {
                _udpClient.Connect(IP, Port);
                _ = UniTask.Create(() => SendThread());
                _ = UniTask.Create(() => RecvThread());
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.ToString());
            }
        }

        private async UniTask SendThread()
        {
            await UniTask.SwitchToThreadPool();
            while (true)
            {
                while (true)
                {
                    lock (_sendMsgs)
                    {
                        if (_sendMsgs.Count == 0)
                            break;
                    }

                    try
                    {
                        BuffMessage msg = _sendMsgs.Dequeue();
                        int length = _udpClient.Send(msg.bytes, msg.length);
                        LogHelper.Log($"发送完成: {length} byte");
                        GameFrame.message.PutBuffMessage(msg);
                    }
                    catch (OperationCanceledException ex)
                    {
                        LogHelper.LogError("Time out: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError(ex.Message);
                        break;
                    }
                }

                await UniTask.Delay(_delay);
            }
        }

        private async UniTask RecvThread()
        {
            await UniTask.SwitchToThreadPool();
            while (true)
            {
                try
                {
                    int length = _udpClient.Client.Receive(_recvBuff, _recvOffset, _recvBuff.Length - _recvOffset, SocketFlags.None);
                    if (length == 0)
                    {
                        await UniTask.Delay(_delay);
                        continue;
                    }

                    _recvOffset += length;
                    int offset = 0;
                    while (true)
                    {
                        if (_recvOffset - offset < sizeof(int))
                            // 没有足够的数据读取下一个消息的长度
                            break;

                        int dataLength = BitConverter.ToInt32(_recvBuff, offset);
                        if (_recvOffset - offset < dataLength + sizeof(int))
                            // 没有足够的数据读取完整的消息
                            break;

                        // 读取完整消息
                        BuffMessage msg = GameFrame.message.GetBuffMessage();
                        Buffer.BlockCopy(_recvBuff, offset + sizeof(int), msg.bytes, 0, dataLength);

                        lock (_receiveMsgs)
                            _receiveMsgs.Enqueue(msg);

                        // 移动偏移量到下一个消息
                        offset += sizeof(int) + dataLength;
                    }

                    // 将未处理的数据移到缓冲区开头
                    if (_recvOffset - offset > 0)
                        Buffer.BlockCopy(_recvBuff, offset, _recvBuff, 0, _recvOffset - offset);

                    _recvOffset -= offset;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex.ToString());
                    break;
                }

                await UniTask.Delay(_delay);
            }
        }

        public void Send(BuffMessage message)
        {
            if (message.length > 0)
            {
                int headLength = sizeof(int);
                Buffer.BlockCopy(message.bytes, 0, message.bytes, headLength, message.length);
                BitConverter.TryWriteBytes(message.bytes.AsSpan(0), message.length);
                message.length += headLength;

                lock (_sendMsgs)
                    _sendMsgs.Enqueue(message);
            }
            else
            {
                GameFrame.message.PutBuffMessage(message);
            }
        }

        public void Close()
        {
            if (_udpClient == null)
                return;

            try
            {
                _udpClient.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.ToString());
            }
        }

        public void Dispose()
        {
            Close();
            _udpClient = null;
            _sendMsgs = null;
            _receiveMsgs = null;
        }
    }
}