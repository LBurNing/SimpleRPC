using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Game
{
    public class Socket
    {
        private const int BUFFER_SIZE = 100 * 1024;
        private Queue<BuffMessage> _sendMsgs;
        private Queue<BuffMessage> _receiveMsgs;
        private TcpListener? _listener;
        private ObjectFactory<BuffMessage> _msg;
        private byte[] _recvBuff;
        private int _recvOffset;

        public Socket()
        {
            _msg = new ObjectFactory<BuffMessage>();
            _sendMsgs = new Queue<BuffMessage>();
            _receiveMsgs = new Queue<BuffMessage>();
            _recvBuff = new byte[BUFFER_SIZE];
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

        public async void RunServer()
        {
            _listener = new TcpListener(IPAddress.Any, 8888);
            _listener.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected...");
                await Task.WhenAll(ReceiveAsync(client), SendThread(client));
            }
        }

        private async Task SendThread(TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    lock (_sendMsgs)
                    {
                        if (_sendMsgs.Count == 0)
                            continue;
                    }

                    try
                    {
                        BuffMessage msg = _sendMsgs.Dequeue();
                        var cts = new CancellationTokenSource();
                        cts.CancelAfter(TimeSpan.FromMilliseconds(10));

                        await client.GetStream().WriteAsync(msg.bytes, 0, msg.length, cts.Token);
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
                client?.Close();
            }
        }

        private async Task ReceiveAsync(TcpClient client)
        {
            try
            {
                while (client.Connected)
                {
                    try
                    {
                        int length = client.GetStream().Socket.Receive(_recvBuff, _recvOffset, _recvBuff.Length - _recvOffset, SocketFlags.None);
                        if (length == 0)
                        {
                            Console.WriteLine("client disconnected..");
                            client.Close();
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
                client?.Close();
            }
    
        }

        public void Send(BuffMessage message)
        {
            if (message.length > 0)
            {
                byte[] lengthBytes = BitConverter.GetBytes(message.length);
                Buffer.BlockCopy(message.bytes, 0, message.bytes, lengthBytes.Length, message.length);
                Buffer.BlockCopy(lengthBytes, 0, message.bytes, 0, lengthBytes.Length);
                message.length += lengthBytes.Length;

                lock (_sendMsgs)
                    _sendMsgs.Enqueue(message);
            }
            else
            {
                _msg.Put(message);
            }
        }

        public void Destroy()
        {
            _listener?.Dispose();
            _listener = null;
        }
    }
}