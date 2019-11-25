using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //创建服务器对象
            ServerControl server = new ServerControl();
            //启动服务器
            server.StartServer();
            Console.ReadKey();//让这个服务器保持运行状态,不然客户端连不上
        }
    }
    // 服务器类
    class ServerControl
    {
        private Socket serverSocket; //声明服务器
        private List<Socket> clients; //存取所有已连接客户端

        public ServerControl() //构造函数
        {
            // 创建服务器对象需要初始化 IP协议 和 传输格式 以及 通信协议
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clients = new List<Socket>(); //初始化服务器集合
        }
        public void StartServer() //启动服务器
        {
            //创建一个端口对象,需要初始化IP地址(0.0.0.0)和端口号
            //Any表示自动侦测本机IP
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 16383);
            serverSocket.Bind(endPoint); //绑定后,确定本机为服务器
            serverSocket.Listen(10); //开始监听,挂起数10
            Console.WriteLine("服务器成功启动          ---" + DateTime.Now.ToString());
            Thread th = new Thread(AcceptFun); //开个线程挂起等待连接方法
            th.IsBackground = true;
            th.Start();
        }
        void AcceptFun() //等待连接
        {
            while (true)
            {   //挂起当前线程等待客户端的连接
                Socket client = serverSocket.Accept();
                //如果有远程客户端连接,接收过来
                IPEndPoint point = client.RemoteEndPoint as IPEndPoint;

                //将连接到的客户端IP地址和端口号显示出来 
                Console.WriteLine(point.Address + "[" + point.Port + "]连接成功          ---" + DateTime.Now.ToString());
                clients.Add(client); //新客户端添加进服务器集合
                Thread th = new Thread(ReceiveFun); //开启一个线程来挂起接收方法
                th.IsBackground = true;
                th.Start(client); //参数是object类型,根据里氏替换原则,可以传入它的派生类
            }
        }
        private void ReceiveFun(object obj) //接收方法,实时接收客户端的位置旋转信息
        {
            Socket client = obj as Socket; //将参数强转为Socket类型,
            IPEndPoint point = client.RemoteEndPoint as IPEndPoint; //基类强转派生类,获取发消息的远程客户端,
            try //如果该端口客户端掉线会发生异常
            {
                byte[] br = new byte[1024]; //用来存取接收来的消息
                int msgLeng = client.Receive(br); //将消息读进数组里
                string data = Encoding.UTF8.GetString(br, 0, msgLeng); //将字节数组转为字符串
                Broadcast(client, data); //将该客户端的消息加工一下广播给其他客户端
                ReceiveFun(client); //使用尾递归,循环调用此方法
            }
            catch
            {
                 Console.WriteLine(point.Address + "[" + point.Port + "]已断开          ---" + DateTime.Now.ToString()); //显示已断开的客户端                
                 clients.Remove(client); //客户端集合会移除当前客户端         
            }
        }
        private void Broadcast(Socket targetClinet, string data) //广播数据
        {
            foreach (var item in clients) //遍历所有客户端
            {
                if (item != targetClinet) //如果是其它客户端
                    item.Send(Encoding.UTF8.GetBytes(data)); //将消息广播给它       
            }
        }
    }
}