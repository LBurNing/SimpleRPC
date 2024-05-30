using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using System;
using System.Threading;
using UnityEngine.Profiling;

namespace Game
{
    public enum SocketState
    {
        None = 0,
        Connected = 1,
        Disconnected = 2,
        Connecting = 3,
        ConnectFailed = 4,
        Close = 5,
        Dispose = 6,
    }

    public class Tcp
    {
        private Queue<BuffMessage> _sendMsgs;
        private Queue<BuffMessage> _receiveMsgs;
        private TcpClient _tcpClient;

        private SocketState _socketState;
        private byte[] _recvBuff;
        private int _recvOffset;
        private int _delay = 10;
        public SocketState State { get { return _socketState; } }
        public string IP { get; set; }
        public int Port { get; set; }

        public NetworkStream Stream
        {
            get { return _tcpClient.GetStream(); }
        }

        public Tcp()
        {
            _sendMsgs = new Queue<BuffMessage>();
            _receiveMsgs = new Queue<BuffMessage>();
            _tcpClient = new TcpClient();
            _recvBuff = new byte[Globals.BUFFER_SIZE];
        }

        public void Update()
        {
            Profiler.BeginSample("on tcp rpc");
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

        public async void Connect()
        {
            try
            {
                Close();
                SetSocketState(SocketState.Connecting);
                await _tcpClient.ConnectAsync(IP, Port);
                OnConnect();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.ToString());
            }
        }

        private void OnConnect()
        {
            try
            {
                if (_tcpClient.Connected)
                {
                    LogHelper.Log("connected...");
                    SetSocketState(SocketState.Connected);
                    _ = UniTask.Create(() => SendThread());
                    _ = UniTask.Create(() => RecvThread());
                }
                else
                {
                    SetSocketState(SocketState.ConnectFailed);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError("连接或通信发生错误：{0}" + ex.Message);
                SetSocketState(SocketState.ConnectFailed);
            }
        }

        private async UniTask SendThread()
        {
            await UniTask.SwitchToThreadPool();
            while (_socketState == SocketState.Connected)
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
                        await Stream.WriteAsync(msg.bytes, 0, msg.length);
                        LogHelper.Log($"发送完成: {msg.length} byte");
                        GameFrame.message.PutBuffMessage(msg);
                    }
                    catch (OperationCanceledException ex)
                    {
                        LogHelper.LogError("Time out: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Close();
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
            while (_socketState == SocketState.Connected)
            {
                try
                {
                    int length = await Stream.ReadAsync(_recvBuff, _recvOffset, _recvBuff.Length - _recvOffset);
                    if (length == 0)
                    {
                        LogHelper.Log("connect failed...");
                        break;
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
                    Close();
                    LogHelper.LogError(ex.ToString());
                    break;
                }

                await UniTask.Delay(_delay);
            }
        }

        private void SetSocketState(SocketState state)
        {
            _socketState = state;
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
            if (_tcpClient == null)
                return;

            try
            {
                if (_tcpClient.Connected)
                {
                    _tcpClient.Dispose();
                    SetSocketState(SocketState.Close);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.ToString());
            }
        }

        public void Dispose()
        {
            Close();
            _tcpClient = null;
            _sendMsgs = null;
            _receiveMsgs = null;
            SetSocketState(SocketState.Dispose);
        }
    }
}