using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SensorSever;

public class ServerConnect
{
    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private bool isClientConnected = false;
    private bool isRunning = true;
    private DateTime lastHeartbeat;
    private const int timeoutThreshold = 10000; // 10秒

    public void StartServer()
    {
        try
        {
            // 启动服务器
            server = new TcpListener(IPAddress.Any, 8000);
            server.Start();
            Console.WriteLine("Server started. Waiting for clients...");

            // 开始监听客户端连接
            BeginAcceptClient();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to start server: " + ex.Message);
        }

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            // 在新线程中处理客户端通信
            new System.Threading.Thread(() => HandleClient(client)).Start();
        }
    }

    private void BeginAcceptClient()
    {
        // 异步等待客户端连接
        server.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
        ////临时测试用
        //SendResponse("1");
    }

    void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        while (true)
        {
            // 模拟传感器数据到达
            string command = "SomeMethod"; // 需要调用的方法名
            int messageType = 1; // 自定义消息类型，比如1表示调用方法

            byte[] bodyBuffer = Encoding.UTF8.GetBytes(command);
            int bodyLength = bodyBuffer.Length;

            // 构造包头（8字节：前4字节为包体长度，后4字节为消息类型）
            byte[] headerBuffer = new byte[8];
            BitConverter.GetBytes(bodyLength).CopyTo(headerBuffer, 0);
            BitConverter.GetBytes(messageType).CopyTo(headerBuffer, 4);

            // 发送包头
            stream.Write(headerBuffer, 0, headerBuffer.Length);

            // 发送包体
            stream.Write(bodyBuffer, 0, bodyBuffer.Length);

            // 模拟延时
            System.Threading.Thread.Sleep(5000);
        }
    }

    private void OnClientConnect(IAsyncResult ar)
    {
        try
        {
            client = server.EndAcceptTcpClient(ar); // 接受客户端连接
            stream = client.GetStream();
            Console.WriteLine("Client connected!");
            isClientConnected = true;

            // 启动接收消息的逻辑
            StartReceiving();

            // 准备下一个客户端连接
            BeginAcceptClient();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error accepting client: " + ex.Message);

            // 如果有异常，确保继续监听新的连接
            BeginAcceptClient();
        }
    }

    /// <summary>
    /// 服务器接收客户端消息
    /// </summary>
    private async void StartReceiving()
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (client != null && client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received from client: " + message);

                    // 处理接收到的消息...
                    if (message == "PING")
                    {
                        lastHeartbeat = DateTime.Now; // 重置心跳计时器
                        SendResponse("PONG");
                        
                        //SensorReceiveData("1");
                        //SensorReceiveData("2");
                    }
                }
                else
                {
                    Console.WriteLine("Client disconnected.");
                    isClientConnected = false;
                    client.Close();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error receiving data: " + ex.Message);
        }
        finally
        {
            // 当客户端断开时，确保释放资源
            stream?.Close();
            client?.Close();
        }
    }

    private void StartHeartbeatTimeoutCheck()
    {
        lastHeartbeat = DateTime.Now;

        Task.Run(async () =>
        {
            while (isClientConnected)
            {
                await Task.Delay(1000); // 每秒检测一次

                if ((DateTime.Now - lastHeartbeat).TotalMilliseconds > timeoutThreshold)
                {
                    Console.WriteLine("Connection timed out, closing connection.");
                    isClientConnected = false;
                    StopServer();
                    break;
                }
            }
        });
    }

    public void StopServer()
    {
        isRunning = false;
        client?.Close();
        server?.Stop();
        Console.WriteLine("Server stopped.");
    }

    /// <summary>
    /// 服务端向客户端发消息
    /// </summary>
    /// <param name="responseMessage"></param>
    //private void SendResponse(string responseMessage)
    //{
    //    string response = responseMessage;
    //    byte[] data = Encoding.ASCII.GetBytes(response);
    //    stream.Write(data, 0, data.Length);
    //    Console.WriteLine("Sent heartbeat response: " + response);
    //}

    private void SendResponse(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);

            // 获取消息的长度，并转换成 4 字节的头部（大端字节序）
            byte[] messageLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));

            // 先发送消息头部，再发送消息体
            stream.Write(messageLength, 0, messageLength.Length);
            stream.Write(data, 0, data.Length);

            Console.WriteLine("Sent message: " + message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message: " + ex.Message);
        }
    }

    #region 接收并处理传感器消息

    private int artificialRespirationNum = 0;
    private int pressNum = 0;
    private int artificialRespirationAccNum = 2;
    private int pressAccNum = 30;
    private int round = 0;

    /// <summary>
    /// 服务端接收传感器消息
    /// </summary>
    /// <param name="str">1人工呼吸，2按压</param>
    public void SensorReceiveData(string str)
    {
        if (!isClientConnected)
        {
            return;
        }

        if (round == 5)
        {
            return;
        }

        if (str == "1")
        {
            if (artificialRespirationNum < artificialRespirationAccNum)
            {
                artificialRespirationNum++;
            }
        }
        else if (str == "2")
        {
            if (pressNum < pressAccNum)
            {
                pressNum++;
            }
        }

        if (artificialRespirationNum == artificialRespirationAccNum && pressNum == pressAccNum)
        {
            round++;
            artificialRespirationNum = 0;
            pressNum = 0;
            //if (round == 5)
            //{
            //    //5轮结束触发事件
            //}
        }
        
        Message message = new Message()
        {
            Code = 200,
            Data = new MessageData()
            {
                Type = str,
                ArtificialRespirationNum = artificialRespirationNum,
                PressNum = pressNum,
                ArtificialRespirationAccNum = artificialRespirationAccNum,
                PressAccNum = pressAccNum,
                Round = round,
            }
        };

        string messageStr = JsonConvert.SerializeObject(message);
        SendResponse(messageStr);
    }

    public struct Message
    {
        public int Code;
        public MessageData Data;
    }

    public struct MessageData
    {
        public string Type;
        public int ArtificialRespirationNum;
        public int PressNum;
        public int ArtificialRespirationAccNum;
        public int PressAccNum;
        public int Round;
    }

    //public struct BaseMsg
    //{
    //    public int Header;//2
    //    public int Header;//2
    //}

    #endregion
}