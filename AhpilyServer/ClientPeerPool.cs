﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    /// <summary>
    /// 客户端的连接池:重用客户端连接对象
    /// </summary>
    public class ClientPeerPool
    {
        private Queue<ClientPeer> clientPeerQueue;

        public ClientPeerPool(int capacity)
        {
            clientPeerQueue = new Queue<ClientPeer>(capacity);
        }

        /// <summary>
        /// 添
        /// </summary>
        /// <param name="client"></param>
        public void Enqueue(ClientPeer client)
        {
            clientPeerQueue.Enqueue(client);
        }

        /// <summary>
        /// 取
        /// </summary>
        /// <returns></returns>
        public ClientPeer Dequeue()
        {
            return clientPeerQueue.Dequeue();
        }
    }
}
