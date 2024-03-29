﻿using AhpilyServer.Concurrent;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Fight
{
    /// <summary>
    /// 战斗的缓存层
    /// </summary>
    public class FightCache
    {
        /// <summary>
        /// 用户id 对应 房间id
        /// </summary>
        private Dictionary<int, int> uidRoomIdDict = new Dictionary<int, int>();

        /// <summary>
        /// 房间id  对应 房间模型对象
        /// </summary>
        private Dictionary<int, FightRoom> idRoomDict = new Dictionary<int, FightRoom>();

        /// <summary>
        /// 重用房间队列
        /// </summary>
        private Queue<FightRoom> roomQueue = new Queue<FightRoom>();

        /// <summary>
        /// 房间id
        /// </summary>
        private ConcurrentInt id = new ConcurrentInt(-1);

        /// <summary>
        /// 创建战斗房间
        /// </summary>
        /// <returns></returns>
        public FightRoom Create(List<int> uidList)
        {
            FightRoom room = null;
            if (roomQueue.Count > 0)
            {
                room = roomQueue.Dequeue();
                room.Init(uidList);
            }
            else
                room = new FightRoom(id.Add_Get(), uidList);

            //绑定映射关系
            foreach (int uid in uidList)
            {
                uidRoomIdDict.Add(uid, room.Id);
            }
            idRoomDict.Add(room.Id, room);
            return room;
        }

        public bool IsFighting(int userId)
        {
            return uidRoomIdDict.ContainsKey(userId);
        }

        /// <summary>
        /// 获取房间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FightRoom GetRoom(int id)
        {
            if(idRoomDict.ContainsKey(id)==false)
            {
                throw new Exception("不存在此房间");
            }
            return idRoomDict[id];
        }

        /// <summary>
        /// 根据用户id获取所在的房间
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public FightRoom GetRoomByUId(int uid)
        {
            if(uidRoomIdDict.ContainsKey(uid)==false)
            {
                throw new Exception("当前用户不在房间");
            }
            int roomId = uidRoomIdDict[uid];
            return GetRoom(roomId);
        }

        /// <summary>
        /// 摧毁房间
        /// </summary>
        /// <param name="room"></param>
        public void Destroy(FightRoom room)
        {
            //移除映射关系
            idRoomDict.Remove(room.Id);
            foreach (PlayerDto player in room.PlayerList)
            {
                uidRoomIdDict.Remove(player.UserId);
            }
            //初始化房间数据
            room.PlayerList.Clear();
            room.LeaveUIdList.Clear();
            room.TableCardList.Clear();
            room.LibraryModel.Init();
            room.Multiple = 1;
            room.RoundModel.Init();

            roomQueue.Enqueue(room);
        }
    }
}
