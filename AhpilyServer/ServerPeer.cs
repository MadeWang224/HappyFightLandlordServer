using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AhpilyServer
{
    /// <summary>
    /// 服务器端
    /// </summary>
    public class ServerPeer
    {
        /// <summary>
        /// 服务器端的socket对象
        /// </summary>
        private Socket serverSocket;

        /// <summary>
        /// 限制客户端连接数量的信号量
        /// </summary>
        private Semaphore acceptSemaphore;

        /// <summary>
        /// 客户端对象的连接池
        /// </summary>
        private ClientPeerPool clientPeerPool;

        /// <summary>
        /// 应用层
        /// </summary>
        private IApplication application;

        /// <summary>
        /// 设置应用层
        /// </summary>
        /// <param name="app"></param>
        public void SetApplication(IApplication app)
        {
            this.application = app;
        }

        /// <summary>
        /// 用来开启服务器
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="maxCount">最大连接数量</param>
        public void Start(int port,int maxCount)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                acceptSemaphore = new Semaphore(maxCount, maxCount);

                //创建出最大数量的连接对象
                clientPeerPool = new ClientPeerPool(maxCount);
                ClientPeer tmpClientPeer = null;
                for (int i = 0; i < maxCount; i++)
                {
                    tmpClientPeer = new ClientPeer();
                    tmpClientPeer.ReceiveArgs.Completed += Receive_Completed;
                    tmpClientPeer.receiveCompleted = ReceiveCompleted;
                    tmpClientPeer.sendDisconnect = Disconnect;
                    clientPeerPool.Enqueue(tmpClientPeer);
                }

                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                serverSocket.Listen(10);

                Console.WriteLine("服务器启动......");

                StartAccept(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #region 接收客户端的连接

        /// <summary>
        /// 开始等待客户端的连接
        /// </summary>
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if(e==null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += Accept_Completed;
            }

            bool result = serverSocket.AcceptAsync(e);
            //返回值判断异步事件是否执行完毕
            //true 代表正在执行,执行完毕后触发事件
            //false 代表已经执行完成,直接处理
            if(result==false)
            {
                ProcessAccept(e);
            }
        }

        /// <summary>
        /// 接受连接请求异步事件完成时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Accept_Completed(object sender,SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        /// <summary>
        /// 处理连接请求
        /// </summary>
        /// <param name="e"></param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            //限制线程的访问
            acceptSemaphore.WaitOne();

            //得到客户端对象
            //Socket clientSocket = e.AcceptSocket;
            ClientPeer client = clientPeerPool.Dequeue();
            client.ClientSocket = e.AcceptSocket;


            //开始接收数据
            StartReceive(client);

            e.AcceptSocket = null;
            StartAccept(e);
        }

        #endregion

        #region 接收数据

        /// <summary>
        /// 开始接收数据
        /// </summary>
        /// <param name="client"></param>
        private void StartReceive(ClientPeer client)
        {
            try
            {
                bool result = client.ClientSocket.ReceiveAsync(client.ReceiveArgs);
                if(result==false)
                {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 处理接收的请求
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            ClientPeer client = e.UserToken as ClientPeer;
            //判断网络消息是否接收成功
            if(client.ReceiveArgs.SocketError==SocketError.Success && client.ReceiveArgs.BytesTransferred>0)
            {
                //拷贝到数组中
                byte[] packet = new byte[client.ReceiveArgs.BytesTransferred];
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, client.ReceiveArgs.BytesTransferred);
                //让客户端自身处理数据包,解析数据包
                client.StartReceive(packet);
                //尾递归
                StartReceive(client);
            }
            //断开连接:如果没有传输的字节数,代表断开连接
            else if (client.ReceiveArgs.BytesTransferred == 0)
            {
                if(client.ReceiveArgs.SocketError==SocketError.Success)
                {
                    //客户端主动断开连接
                    Disconnect(client, "客户端主动断开连接");
                }
                else
                {
                    //由于网络异常导致被动断开连接
                    Disconnect(client, client.ReceiveArgs.SocketError.ToString()); ;
                }
            }
        }

        /// <summary>
        /// 当接收完成时,触发的事件
        /// </summary>
        /// <param name="e"></param>
        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        /// <summary>
        /// 一条数据解析完成的处理
        /// </summary>
        /// <param name="client">对应的连接对象</param>
        /// <param name="value">解析出来的一个具体能使用的类型</param>
        private void ReceiveCompleted(ClientPeer client,SocketMsg msg)
        {
            //给应用层,让其使用
            application.OnReceive(client, msg);
        }

        #endregion

        #region 发送数据



        #endregion

        #region 断开连接

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client">表示断开的客户端连接对象</param>
        /// <param name="reason">断开的原因</param>
        public void Disconnect(ClientPeer client,string reason)
        {
            try
            {
                if (client == null)
                    throw new Exception("当前指定的客户端连接对象为空,无法断开连接");

                Console.WriteLine(client.ClientSocket.RemoteEndPoint.ToString() + "客户端断开连接:" + reason);
                //通知应用层,这个客户端断开连接了
                application.OnDisconnect(client);

                client.Disconnect();
                //回收对象,方便下次使用
                clientPeerPool.Enqueue(client);
                //释放
                acceptSemaphore.Release();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion

    }
}
