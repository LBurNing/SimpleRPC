using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using System;
using System.Threading;
using UnityEngine.Profiling;
using System.IO;
using Unity.VisualScripting;
using System.Collections.Concurrent;

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
        private ConcurrentQueue<BuffMessage> _sendMsgs;
        private ConcurrentQueue<BuffMessage> _receiveMsgs;
        private TcpClient _tcpClient;

        private SocketState _socketState;
        private byte[] _recvBuff;
        private int _recvOffset;
        private int _delay = 10;
        private CancellationTokenSource _recvCancel;
        private CancellationTokenSource _sendCancelToken;

        public SocketState State { get { return _socketState; } }
        public string IP { get; set; }
        public int Port { get; set; }

        public NetworkStream Stream
        {
            get { return _tcpClient.GetStream(); }
        }

        public Tcp()
        {
            _sendMsgs = new ConcurrentQueue<BuffMessage>();
            _receiveMsgs = new ConcurrentQueue<BuffMessage>();
            _recvBuff = new byte[Globals.BUFFER_SIZE];
        }

        private void InitTcpClient()
        {
            _tcpClient = new TcpClient();
            _recvCancel = new CancellationTokenSource();
            _sendCancelToken = new CancellationTokenSource();
        }

        public void Update()
        {
            Profiler.BeginSample("on tcp rpc");
            if (_receiveMsgs.TryDequeue(out BuffMessage msg))
            {
                RPCMoudle.OnRPC(msg);
                GameFrame.message.PutBuffMessage(msg);
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
                InitTcpClient();
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
                    StartAsyncTasks();
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

        private void StartAsyncTasks()
        {
            _ = SendThread();
            _ = RecvThread();
        }

        private async UniTask SendThread()
        {
            await UniTask.SwitchToThreadPool();
            while (_socketState == SocketState.Connected)
            {
                while (true)
                {
                    if (!_sendMsgs.TryDequeue(out BuffMessage msg))
                        break;

                    var timeoutToken = new CancellationTokenSource();
                    timeoutToken.CancelAfterSlim(TimeSpan.FromMilliseconds(msg.TimeoutMillisecond));
                    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_sendCancelToken.Token, timeoutToken.Token);

                    try
                    {
                        await Stream.WriteAsync(msg.bytes, 0, msg.length, linkedCts.Token);
                        LogHelper.Log($"发送完成: {msg.length} byte");
                        GameFrame.message.PutBuffMessage(msg);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (timeoutToken.IsCancellationRequested)
                        {
                            _sendMsgs.Enqueue(msg);
                            LogHelper.LogWarning("消息发送超时, 添加到队列末尾, 等待发送...");
                            await UniTask.Delay(10);
                            continue;
                        }

                        LogHelper.LogWarning("发送操作被终止..." + ex.Message);
                        break;
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionAborted)
                    {
                        LogHelper.Log("发送操作被终止...");
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError("发送错误: " + ex.Message);
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
                    int length = await Stream.ReadAsync(_recvBuff, _recvOffset, _recvBuff.Length - _recvOffset, _recvCancel.Token);
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
                        _receiveMsgs.Enqueue(msg);

                        // 移动偏移量到下一个消息
                        offset += sizeof(int) + dataLength;
                    }

                    // 将未处理的数据移到缓冲区开头
                    if (_recvOffset - offset > 0)
                        Buffer.BlockCopy(_recvBuff, offset, _recvBuff, 0, _recvOffset - offset);

                    _recvOffset -= offset;
                }
                catch(OperationCanceledException ex)
                {
                    LogHelper.Log("读取操作被终止: " + ex.Message);
                    break;
                }
                catch (IOException ex) when (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.OperationAborted)
                {
                    LogHelper.Log("读取操作被终止...");
                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError("读取错误: " + ex.ToString());
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
                    _recvCancel.Dispose();
                    _sendCancelToken.Dispose();
                    _tcpClient.Close();
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
        
            if (_tcpClient != null)
            {
                _tcpClient.Dispose();
                _tcpClient = null;
            }

            if (_sendMsgs != null)
            {
                _sendMsgs.Clear();
                _sendMsgs = null;
            }

            if (_receiveMsgs != null)
            {
                _receiveMsgs.Clear();
                _receiveMsgs = null;
            }

            SetSocketState(SocketState.Dispose);
        }
    }
}