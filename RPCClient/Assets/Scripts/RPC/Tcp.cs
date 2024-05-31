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
                LogHelper.LogError("���ӻ�ͨ�ŷ�������{0}" + ex.Message);
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
                        LogHelper.Log($"�������: {msg.length} byte");
                        GameFrame.message.PutBuffMessage(msg);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (timeoutToken.IsCancellationRequested)
                        {
                            _sendMsgs.Enqueue(msg);
                            LogHelper.LogWarning("��Ϣ���ͳ�ʱ, ��ӵ�����ĩβ, �ȴ�����...");
                            await UniTask.Delay(10);
                            continue;
                        }

                        LogHelper.LogWarning("���Ͳ�������ֹ..." + ex.Message);
                        break;
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionAborted)
                    {
                        LogHelper.Log("���Ͳ�������ֹ...");
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError("���ʹ���: " + ex.Message);
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
                            // û���㹻�����ݶ�ȡ��һ����Ϣ�ĳ���
                            break;

                        int dataLength = BitConverter.ToInt32(_recvBuff, offset);
                        if (_recvOffset - offset < dataLength + sizeof(int))
                            // û���㹻�����ݶ�ȡ��������Ϣ
                            break;

                        // ��ȡ������Ϣ
                        BuffMessage msg = GameFrame.message.GetBuffMessage();
                        Buffer.BlockCopy(_recvBuff, offset + sizeof(int), msg.bytes, 0, dataLength);
                        _receiveMsgs.Enqueue(msg);

                        // �ƶ�ƫ��������һ����Ϣ
                        offset += sizeof(int) + dataLength;
                    }

                    // ��δ����������Ƶ���������ͷ
                    if (_recvOffset - offset > 0)
                        Buffer.BlockCopy(_recvBuff, offset, _recvBuff, 0, _recvOffset - offset);

                    _recvOffset -= offset;
                }
                catch(OperationCanceledException ex)
                {
                    LogHelper.Log("��ȡ��������ֹ: " + ex.Message);
                    break;
                }
                catch (IOException ex) when (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.OperationAborted)
                {
                    LogHelper.Log("��ȡ��������ֹ...");
                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError("��ȡ����: " + ex.ToString());
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