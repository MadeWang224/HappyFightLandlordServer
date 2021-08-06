using AhpilyServer;
using AhpilyServer.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Match
{
    /// <summary>
    /// 匹配的缓存层
    /// </summary>
    public class MatchCache
    {
        /// <summary>
        /// 正在等待的用户id 和 房间id
        /// </summary>
        private Dictionary<int, int> uIdRoomIdDict = new Dictionary<int, int>();

        /// <summary>
        /// 正在等待的房间id 和房间数据模型
        /// </summary>
        private Dictionary<int, MatchRoom> idModelDict = new Dictionary<int, MatchRoom>();

        /// <summary>
        /// 房间重用队列
        /// </summary>
        Queue<MatchRoom> roomQueue = new Queue<MatchRoom>();

        /// <summary>
        /// 房间id
        /// </summary>
        private ConcurrentInt id = new ConcurrentInt(-1);

        /// <summary>
        /// 进入匹配/进入房间
        /// </summary>
        /// <returns></returns>
        public MatchRoom Enter(int userId,ClientPeer client)
        {
            //遍历等待的房间,有没有正在等待的,如果有,加入
            foreach (MatchRoom mr in idModelDict.Values)
            {
                if (mr.IsFull())
                    continue;
                mr.Enter(userId,client);
                uIdRoomIdDict.Add(userId, mr.Id);
                return mr;
            }

            //如果没有正在等待的房间,新开一个房
            MatchRoom room = null;
            if (roomQueue.Count > 0)
                room = roomQueue.Dequeue();
            else
                room = new MatchRoom(id.Add_Get());
            room.Enter(userId,client);
            idModelDict.Add(room.Id, room);
            uIdRoomIdDict.Add(userId, room.Id);
            return room;
        }

        /// <summary>
        /// 离开匹配/房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MatchRoom Leave(int userId)
        {
            int roomId = uIdRoomIdDict[userId];
            MatchRoom room = idModelDict[roomId];
            room.Leave(userId);

            uIdRoomIdDict.Remove(userId);
            if(room.IsEmpty())
            {
                idModelDict.Remove(roomId);
                roomQueue.Enqueue(room);
            }
            return room;
        }

        /// <summary>
        /// 判断用户是否正在匹配
        /// </summary>
        /// <returns></returns>
        public bool IsMatching(int userId)
        {
            return uIdRoomIdDict.ContainsKey(userId);
        }

        /// <summary>
        /// 获取玩家所在的等待房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MatchRoom GetRoom(int userId)
        {
            int roomId = uIdRoomIdDict[userId];
            MatchRoom room = idModelDict[roomId];
            return room;
        }

        /// <summary>
        /// 摧毁房间
        /// </summary>
        public void Destroy(MatchRoom room)
        {
            idModelDict.Remove(room.Id);
            foreach (var userId in room.UIdClientDict.Keys)
            {
                uIdRoomIdDict.Remove(userId);
            }
            //清空数据
            room.UIdClientDict.Clear();
            room.ReadyUIdList.Clear();

            roomQueue.Enqueue(room);
        }
    }
}
