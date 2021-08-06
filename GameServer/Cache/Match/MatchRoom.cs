using AhpilyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Match
{
    /// <summary>
    /// 匹配房间
    /// </summary>
    public class MatchRoom
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 在房间内的用户id 和 对应的连接对象
        /// </summary>
        public Dictionary<int,ClientPeer> UIdClientDict { get; private set; }

        /// <summary>
        /// 已准备的玩家id列表
        /// </summary>
        public List<int> ReadyUIdList { get; private set; }

        public MatchRoom(int id)
        {
            this.Id = id;
            this.UIdClientDict = new Dictionary<int, ClientPeer>();
            this.ReadyUIdList = new List<int>();
        }

        /// <summary>
        /// 房间是否满了
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return UIdClientDict.Count == 3;
        }

        /// <summary>
        /// 房间是否空了
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return UIdClientDict.Count == 0;
        }

        /// <summary>
        /// 是否所有人都准备了
        /// </summary>
        /// <returns></returns>
        public bool IsAllReady()
        {
            return ReadyUIdList.Count == 3;
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="userId"></param>
        public void Enter(int userId,ClientPeer client)
        {
            UIdClientDict.Add(userId, client);
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="userId"></param>
        public void Leave(int userId)
        {
            UIdClientDict.Remove(userId);
        }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="userId"></param>
        public void Ready(int userId)
        {
            ReadyUIdList.Add(userId);
        }

        /// <summary>
        /// 广播房间内的所有玩家信息
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="subCode"></param>
        /// <param name="value"></param>
        public void Brocast(int opCode,int subCode,object value,ClientPeer exClient=null)
        {
            SocketMsg msg = new SocketMsg(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);

            foreach (var client in UIdClientDict.Values)
            {
                if (client == exClient)
                    continue;
                client.Send(packet);
            }
        }

        public List<int> GetUIdList()
        {
            return UIdClientDict.Keys.ToList();
        }
    }
}
