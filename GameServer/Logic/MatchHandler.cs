using AhpilyServer;
using GameServer.Cache;
using GameServer.Cache.Match;
using GameServer.Model;
using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic
{
    public delegate void StartFight(List<int> uidList);

    public class MatchHandler : IHandler
    {
        public StartFight startFight;

        public MatchCache matchCache = Caches.Match;
        private UserCache userCache = Caches.User;

        public void OnDisconnect(ClientPeer client)
        {
            if (!userCache.IsOnline(client))
                return;

            int userId = userCache.GetId(client);
            if (matchCache.IsMatching(userId))
            {
                Leave(client);
            }
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case MatchCode.ENTER_CREQ:
                    Enter(client);
                    break;
                case MatchCode.LEAVE_CREQ:
                    Leave(client);
                    break;
                case MatchCode.READY_CREQ:
                    Ready(client);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 进入匹配房间
        /// </summary>
        /// <param name="client"></param>
        private void Enter(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                int userId = userCache.GetId(client);
                //已在匹配队列中
                if(matchCache.IsMatching(userId))
                {
                    client.Send(OpCode.MATCH, MatchCode.ENTER_SRES, -1);
                    return;
                }
                //进入匹配房间
                MatchRoom room = matchCache.Enter(userId,client);
                //广播给房间内所有用户,有玩家加入
                #region 构造一个UserDto  Dto就是针对UI定义的 UI需要什么我们就加什么字段
                UserModel model = userCache.GetModelById(userId);
                UserDto userDto = new UserDto(model.Id, model.Name, model.Been, model.WinCount, model.LoseCount, model.RunCount, model.Lv, model.Exp);
                #endregion
                room.Brocast(OpCode.MATCH, MatchCode.ENTER_BRO, userDto, client);
                //返回给当前客户端房间的数据模型
                MatchRoomDto dto = MakeRoomDto(room);
                client.Send(OpCode.MATCH, MatchCode.ENTER_SRES, dto);
            });
        }
        private MatchRoomDto MakeRoomDto(MatchRoom room)
        {
            MatchRoomDto dto = new MatchRoomDto();
            foreach (var uid in room.UIdClientDict.Keys)
            {
                UserModel userModel = userCache.GetModelById(uid);
                UserDto UserDto = new UserDto(userModel.Id,userModel.Name, userModel.Been, userModel.WinCount, userModel.LoseCount, userModel.RunCount, userModel.Lv, userModel.Exp);
                dto.UIdUserDict.Add(uid, UserDto);

                dto.uIdList.Add(uid);
            }
            dto.ReadyUIdList = room.ReadyUIdList;
            return dto;
        }

        /// <summary>
        /// 离开匹配
        /// </summary>
        /// <param name="client"></param>
        private void Leave(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (!userCache.IsOnline(client))
                    return;
                
                int userId = userCache.GetId(client);
                if (matchCache.IsMatching(userId) == false)
                    return;

                MatchRoom room = matchCache.Leave(userId);
                //广播
                room.Brocast(OpCode.MATCH, MatchCode.LEAVE_BRO, userId);
                Console.WriteLine("有玩家离开匹配房间");
            });
        }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="client"></param>
        private void Ready(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (!userCache.IsOnline(client))
                    return;
                int userId = userCache.GetId(client);
                if (!matchCache.IsMatching(userId))
                    return;
                MatchRoom room = matchCache.GetRoom(userId);
                room.Ready(userId);
                //广播 有人准备
                room.Brocast(OpCode.MATCH, MatchCode.READY_BRO, userId);
                //检测是否所有人都准备了
                if(room.IsAllReady())
                {
                    //开始战斗
                    //TODO
                    startFight(room.GetUIdList());
                    //广播 开始游戏
                    room.Brocast(OpCode.MATCH, MatchCode.START_BRO, null);
                    //销毁房间
                    matchCache.Destroy(room);
                }
            });
        }
    }
}
