using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    /// <summary>
    /// 封装的客户端de连接对象
    /// </summary>
    public class ClientPeer
    {
        public Socket ClientSocket { get; set; }

        public ClientPeer()
        {
            this.ReceiveArgs = new SocketAsyncEventArgs();
            this.ReceiveArgs.UserToken = this;
            this.ReceiveArgs.SetBuffer(new byte[1024], 0, 1024);
            this.SendArgs = new SocketAsyncEventArgs();
            this.SendArgs.Completed += SendArgs_Completed;
        }

        #region 接收数据

        public delegate void ReceiveCompleted(ClientPeer client, SocketMsg msg);

        /// <summary>
        /// 一个消息解析完成的回调
        /// </summary>
        public ReceiveCompleted receiveCompleted;

        /// <summary>
        /// 一旦接收到数据,就存到缓存区
        /// </summary>
        private List<byte> dataCache = new List<byte>();

        /// <summary>
        /// 接收的异步套接字请求
        /// </summary>
        public SocketAsyncEventArgs ReceiveArgs { get; set; }

        /// <summary>
        /// 是否正在处理接收的数据
        /// </summary>
        private bool IsReceiveProcess = false;

        /// <summary>
        /// 自身处理数据包
        /// </summary>
        /// <param name="packet"></param>
        public void StartReceive(byte[] packet)
        {
            dataCache.AddRange(packet);
            if (!IsReceiveProcess)
                ProcessReceive();
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private void ProcessReceive()
        {
            IsReceiveProcess = true;

            //解析数据包
            byte[] data = EncodeTool.DecodePacket(ref dataCache);

            if(data==null)
            {
                IsReceiveProcess = false;
                return;
            }

            SocketMsg msg = EncodeTool.DecodeMsg(data);
            //回调给上层
            receiveCompleted?.Invoke(this, msg);

            //尾递归
            ProcessReceive();
        }

        //粘包和拆包问题:解决决策:1.消息头和消息尾;2.添加特殊字符进行分隔
        //头:信息的长度
        //尾:具体的信息

        #endregion

        #region 断开连接

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                //清空数据
                dataCache.Clear();
                IsReceiveProcess = false;
                sendQueue.Clear();
                isSendProcess = false;

                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
                ClientSocket = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion

        #region 发送数据

        /// <summary>
        /// 发送的消息的一个队列
        /// </summary>
        private Queue<byte[]> sendQueue = new Queue<byte[]>();

        private bool isSendProcess = false;

        /// <summary>
        /// 发送的异步套接字操作
        /// </summary>
        private SocketAsyncEventArgs SendArgs;

        /// <summary>
        /// 发送的时候,断开连接的回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reason"></param>
        public delegate void SendDisconnect(ClientPeer client, string reason);

        public SendDisconnect sendDisconnect;

        /// <summary>
        /// 发送网络消息
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作</param>
        /// <param name="value">参数</param>
        public void Send(int opCode,int subCode,object value)
        {
            //消息处理
            SocketMsg msg = new SocketMsg(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);

            Send(packet);
        }

        public void Send(byte[] packet)
        {
            //存入消息队列
            sendQueue.Enqueue(packet);
            if (!isSendProcess)
                ProcessSend();
        }


        /// <summary>
        /// 处理发送操作
        /// </summary>
        private void ProcessSend()
        {
            isSendProcess = true;
            
            //如果数据的条数为0,就停止
            if(sendQueue.Count==0)
            {
                isSendProcess = false;
                return;
            }

            //取出一条数据
            byte[] packet = sendQueue.Dequeue();
            //设置消息 发送异步套接字操作 的发送数据缓冲区
            SendArgs.SetBuffer(packet, 0, packet.Length);
            bool result = ClientSocket.SendAsync(SendArgs);
            if(result==false)
            {
                SendCompleted();
            }
        }

        private void SendArgs_Completed(object send,SocketAsyncEventArgs e)
        {
            SendCompleted();
        }

        /// <summary>
        /// 当异步发送请求完成的时候调用
        /// </summary>
        private void SendCompleted()
        {
            //发送的有没有错误
            if(SendArgs.SocketError!=SocketError.Success)
            {
                //发送出错了:客户端断开连接了
                sendDisconnect(this, SendArgs.SocketError.ToString());
            }
            else
            {
                ProcessSend();
            }
        }

        #endregion
    }
}
