using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ClientControl
{
    public event Action<string> msg; //同步接收事件
    public Socket clientSocket;
    public ClientControl() //构造函数
    {
        // 创建客户端对象需要初始化 IP协议 和 传输格式 以及 通信协议
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }
    public void ConnectionServer(string ip, int port, string data) //连接服务器
    {
        //根据IP和端口号连接相应服务器
        clientSocket.Connect(ip, port);
        Debug.Log("--------------- 您已成功连上服务器 ---------------");

        Thread th = new Thread(ReceiveData); //开个线程运行接收方法
        th.IsBackground = true;
        th.Start();
    }

    public void SendData(string data) //发送数据
    {
        clientSocket.Send(Encoding.UTF8.GetBytes(data));
    }

    private void ReceiveData() //接收数据
    {
        try //服务器掉线会报错
        {
            byte[] data = new byte[1024]; //存放服务器消息的数组
            int dataLength = clientSocket.Receive(data); //将服务器消息存进数组中
            string serverData = Encoding.UTF8.GetString(data, 0, dataLength); //将服务器的消息转成字符串
            msg(serverData);
            ReceiveData(); //尾递归,循环调用此方法
        }
        catch
        {
            Debug.Log("服务器停止运行");
        }
    }
}
