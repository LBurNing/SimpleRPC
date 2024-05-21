using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class SocketClient : IDisposable
    {
        private const int BUFFER_SIZE = 100 * 1024;
        private Queue<BuffMessage> _sendMsgs;
        private Queue<BuffMessage> _receiveMsgs;
        private ObjectFactory<BuffMessage> _msg;
        private byte[] _recvBuff;
        private int _recvOffset;
        private TcpClient _client;

        public SocketClient(TcpClient client)
        {
            this._client = client;
            _sendMsgs = new Queue<BuffMessage>();
            _receiveMsgs = new Queue<BuffMessage>();
            _msg = new ObjectFactory<BuffMessage>();
            _recvBuff = new byte[BUFFER_SIZE];
            Task.Run(() => SendThread());
            Task.Run(() => RecvThread());
        }

        public void Update()
        {
            lock (_receiveMsgs)
            {
                if (_receiveMsgs.Count > 0)
                {
                    BuffMessage msg = _receiveMsgs.Dequeue();
                    RPC.OnRPC(msg);
                }
            }
        }

        public bool Connected
        {
            get { return _client.Connected; }
        }

        private async Task SendThread()
        {
            try
            {
                while (_client.Connected)
                {
                    lock (_sendMsgs)
                    {
                        if (_sendMsgs.Count == 0)
                            continue;
                    }

                    try
                    {
                        Console.WriteLine("send thread ID: {0}", Thread.CurrentThread.ManagedThreadId);
                        BuffMessage msg = _sendMsgs.Dequeue();
                        var cts = new CancellationTokenSource();
                        cts.CancelAfter(TimeSpan.FromMilliseconds(10));

                        await _client.GetStream().WriteAsync(msg.bytes, 0, msg.length, cts.Token);
                        Console.WriteLine($"数据发送完成: {msg.length}");
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
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
            try
            {
                while (_client.Connected)
                {
                    try
                    {
                        int length = _client.GetStream().Socket.Receive(_recvBuff, _recvOffset, _recvBuff.Length - _recvOffset, SocketFlags.None);
                        if (length == 0)
                        {
                            Console.WriteLine("client disconnected..");
                            _client.Close();
                            break;
                        }

                        Console.WriteLine("recv thread ID: {0}", Thread.CurrentThread.ManagedThreadId);
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

                            lock (_receiveMsgs)
                                _receiveMsgs.Enqueue(msg);

                            // 移动偏移量到下一个消息
                            offset += sizeof(int) + dataLength;
                        }

                        // 将未处理的数据移到缓冲区开头
                        if (offset > 0)
                            Buffer.BlockCopy(_recvBuff, offset, _recvBuff, 0, _recvOffset - offset);

                        _recvOffset -= offset;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to receive {ex.Message}");
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
                lock (_sendMsgs)
                    _sendMsgs.Enqueue(message);
            }
            else
            {
                _msg.Put(message);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _sendMsgs?.Clear();
            _receiveMsgs?.Clear();
            _msg?.Clear();
        }
    }
}
