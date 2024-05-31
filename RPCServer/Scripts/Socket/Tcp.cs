using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Tcp : IDisposable
    {
        private const int BUFFER_SIZE = 100 * 1024;
        private ConcurrentQueue<BuffMessage> _sendMsgs;
        private ConcurrentQueue<BuffMessage> _receiveMsgs;
        private ObjectFactory<BuffMessage> _msg;
        private CancellationTokenSource _recvCancelToken;
        private CancellationTokenSource _sendCancelToken;

        private byte[] _recvBuff;
        private int _recvOffset;
        private TcpClient _client;

        public Tcp(TcpClient client)
        {
            this._client = client;
            _sendMsgs = new ConcurrentQueue<BuffMessage>();
            _receiveMsgs = new ConcurrentQueue<BuffMessage>();
            _msg = new ObjectFactory<BuffMessage>();
            _recvCancelToken = new CancellationTokenSource();
            _sendCancelToken = new CancellationTokenSource();
            _recvBuff = new byte[BUFFER_SIZE];
            _recvOffset = 0;
            _ = SendThread();
            _ =RecvThread();
        }

        public void Update()
        {
            if(_receiveMsgs.TryDequeue(out BuffMessage msg))
            {
                RPCMouble.OnRPC(msg);
            }
        }

        public bool Connected
        {
            get { return _client.Connected; }
        }

        private async Task SendThread()
        {
            await Task.Yield();
            try
            {
                while (_client.Connected)
                {
                    if (!_sendMsgs.TryDequeue(out BuffMessage msg))
                        continue;

                    var timeoutToken = new CancellationTokenSource();
                    timeoutToken.CancelAfter(TimeSpan.FromMilliseconds(msg.TimeoutMillisecond));
                    var linked = CancellationTokenSource.CreateLinkedTokenSource(_sendCancelToken.Token, timeoutToken.Token);

                    try
                    {
                        await _client.GetStream().WriteAsync(msg.bytes, 0, msg.length, linked.Token);
                        LogHelper.Log($"数据发送完成: {msg.length}");
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (timeoutToken.IsCancellationRequested)
                        {
                            _sendMsgs.Enqueue(msg);
                            LogHelper.LogWarning("消息发送超时, 添加到队列末尾, 等待发送...");
                            await Task.Delay(10);
                            continue;
                        }

                        LogHelper.LogWarning("发送操作被终止..." + ex.Message);
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException socketEx)
                    {
                        if(socketEx.SocketErrorCode == SocketError.ConnectionAborted)
                        {
                            LogHelper.Log("发送消息时断开连接...");
                        }
                        else if(socketEx.SocketErrorCode == SocketError.OperationAborted)
                        {
                            LogHelper.Log("发送操作被终止...");
                        }
                        
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError("发送错误: " + ex.Message);
                        break;
                    }

                    await Task.Delay(1);
                }
            }
            catch
            {
                _client?.Close();
            }
        }

        private async Task RecvThread()
        {
            await Task.Yield();
            try
            {
                while (_client.Connected)
                {
                    try
                    {
                        int length = await _client.GetStream().ReadAsync(_recvBuff, _recvOffset, _recvBuff.Length - _recvOffset, _recvCancelToken.Token);
                        if (length == 0)
                        {
                            LogHelper.Log("client disconnected..");
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
                            BuffMessage msg = _msg.Get();
                            Buffer.BlockCopy(_recvBuff, offset + sizeof(int), msg.bytes, 0, dataLength);
                            _receiveMsgs.Enqueue(msg);

                            // 移动偏移量到下一个消息
                            offset += sizeof(int) + dataLength;
                        }

                        // 将未处理的数据移到缓冲区开头
                        if (offset > 0)
                            Buffer.BlockCopy(_recvBuff, offset, _recvBuff, 0, _recvOffset - offset);

                        _recvOffset -= offset;
                    }
                    catch (OperationCanceledException ex)
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

                    await Task.Delay(1);
                }
            }
            catch
            {
                _client?.Close();
            }

        }

        public void Send(BuffMessage message)
        {
            if (message.length > 0)
            {
                _sendMsgs.Enqueue(message);
            }
            else
            {
                _msg.Put(message);
            }
        }

        public void Dispose()
        {
            _recvCancelToken?.Cancel();
            _sendCancelToken?.Cancel();
            _client?.Dispose();
            _sendMsgs?.Clear();
            _receiveMsgs?.Clear();
            _msg?.Clear();
        }
    }
}
