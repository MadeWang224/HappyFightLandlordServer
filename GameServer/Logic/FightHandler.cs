using AhpilyServer;
using GameServer.Cache;
using GameServer.Cache.Fight;
using GameServer.Model;
using Protocol.Code;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic
{
    public class FightHandler : IHandler
    {
        public FightCache fightCache = Caches.Fight;
        public UserCache userCache = Caches.User;

        public void OnDisconnect(ClientPeer client)
        {
            Leave(client);
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case FightCode.GRAB_LANDLORD_CREQ:
                    bool result = (bool)value;
                    GrabLandlord(client, result);
                    break;
                case FightCode.DEAL_CREQ:
                    Deal(client, value as DealDto);
                    break;
                case FightCode.PASS_CREQ:
                    Pass(client);
                    break;
                default:
                    break;
            }
        }
        
        /// <summary>
        /// 用户离开
        /// </summary>
        /// <param name="client"></param>
        private void Leave(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (userCache.IsOnline(client) == false)
                    return;
                int userId = userCache.GetId(client);
                if(fightCache.IsFighting(userId)==false)
                {
                    return;
                }
                FightRoom room = fightCache.GetRoomByUId(userId);

                //中途退出人
                room.LeaveUIdList.Add(userId);
                Brocast(room, OpCode.FIGHT, FightCode.LEAVE_BRO, userId);

                //3人都离开
                if(room.LeaveUIdList.Count==3)
                {
                    for (int i = 0; i < room.LeaveUIdList.Count; i++)
                    {
                        UserModel um = userCache.GetModelById(room.LeaveUIdList[i]);
                        um.RunCount++;
                        um.Been -= room.Multiple * 1000;
                        um.Exp += 0;
                        userCache.Update(um);
                    }

                    fightCache.Destroy(room);
                }
            });
        }

        /// <summary>
        /// 不出
        /// </summary>
        /// <param name="client"></param>
        private void Pass(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (userCache.IsOnline(client) == false)
                    return;
                int userId = userCache.GetId(client);
                FightRoom room = fightCache.GetRoomByUId(userId);

                //当前玩家为最大出牌者,不能不出
                if(room.RoundModel.BiggestUId==userId)
                {
                    client.Send(OpCode.FIGHT, FightCode.PASS_SRES, -1);
                    return;
                }
                else
                {
                    //可以不出
                    client.Send(OpCode.FIGHT, FightCode.PASS_SRES, 0);
                    Turn(room);
                }
            });
        }

        /// <summary>
        /// 出牌的处理
        /// </summary>
        private void Deal(ClientPeer client,DealDto dto)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (userCache.IsOnline(client) == false)
                    return;
                int userId = userCache.GetId(client);
                if (userId != dto.UserId)
                    return;
                FightRoom room = fightCache.GetRoomByUId(userId);

                //玩家出牌
                //玩家中途退出,掉线
                if(room.LeaveUIdList.Contains(userId))
                {
                    //转换出牌
                    Turn(room);
                }
                //玩家还在
                bool canDeal = room.DealCard(dto.Type, dto.Weight, dto.Lenght, userId, dto.SelectCardList);
                if(canDeal==false)
                {
                    //玩家要不起
                    client.Send(OpCode.FIGHT, FightCode.DEAL_SRES, -1);
                    return;
                }
                else
                {
                    //给自身客户端发送出牌成功的消息
                    client.Send(OpCode.FIGHT, FightCode.DEAL_SRES, 0);
                    //检测剩余手牌,如果手牌为0,游戏结束
                    List<CardDto> remainCardList = room.GetPlayerModel(userId).CardList;
                    dto.RemainCardList = remainCardList;
                    //广播
                    Brocast(room, OpCode.FIGHT, FightCode.DEAL_BRO, dto);

                    if(remainCardList.Count==0)
                    {
                        //游戏结束
                        GameOver(userId, room);
                    }
                    else
                    {
                        //转换出牌
                        Turn(room);
                    }
                }
            });
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="room"></param>
        private void GameOver(int userId,FightRoom room)
        {
            //获取获胜玩家身份,相同身份的玩家id
            int winIdentity = room.GetPlayerIdentity(userId);
            List<int> winUIds = room.GetSameIdentityUIds(winIdentity);

            //结算
            //胜利方
            for (int i = 0; i < winUIds.Count; i++)
            {
                UserModel um = userCache.GetModelById(winUIds[i]);
                um.WinCount++;
                um.Been += room.Multiple * 1000;
                um.Exp += 100;
                int maxExp = um.Lv * 100;
                while(maxExp<=um.Exp)
                {
                    um.Lv++;
                    um.Exp -= maxExp;
                    maxExp = um.Lv * 100;
                }
                userCache.Update(um);
            }
            //失败方
            List<int> loseUIds = room.GetDifferentIdentityUIds(winIdentity);
            for (int i = 0; i < loseUIds.Count; i++)
            {
                UserModel um = userCache.GetModelById(loseUIds[i]);
                um.LoseCount++;
                um.Been -= room.Multiple * 1000;
                um.Exp += 10;
                int maxExp = um.Lv * 100;
                while (maxExp <= um.Exp)
                {
                    um.Lv++;
                    um.Exp -= maxExp;
                    maxExp = um.Lv * 100;
                }
                userCache.Update(um);
            }
            //逃跑的玩家
            for (int i = 0; i < room.LeaveUIdList.Count; i++)
            {
                UserModel um = userCache.GetModelById(room.LeaveUIdList[i]);
                um.RunCount++;
                um.Been -= room.Multiple * 1000;
                um.Exp += 0;
                userCache.Update(um);
            }

            //给客户端发消息:谁赢了,加多少豆
            OverDto dto = new OverDto();
            dto.WinIdentity = winIdentity;
            dto.WinUIdList = winUIds;
            dto.BeenCount = room.Multiple * 1000;
            Brocast(room, OpCode.FIGHT, FightCode.OVER_BRO, dto);

            //在缓存层销毁房间数据
            fightCache.Destroy(room);
        }

        /// <summary>
        /// 转换出牌
        /// </summary>
        private void Turn(FightRoom room)
        {
            //下一个出牌的id
            int nextUId = room.Turn();
            //不在线
            if(room.LeaveUIdList.Contains(nextUId)==true)
            {
                Turn(room);
            }
            else
            {
                //ClientPeer nextClient = userCache.GetClientPeer(nextUId);
                //nextClient.Send(OpCode.FIGHT, FightCode.TURN_DEAL_BRO, nextUId);
                Brocast(room, OpCode.FIGHT, FightCode.TURN_DEAL_BRO, nextUId);
            }
        }

        /// <summary>
        /// 抢地主的处理
        /// </summary>
        private void GrabLandlord(ClientPeer client,bool result)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (userCache.IsOnline(client) == false)
                    return;
                int userId = userCache.GetId(client);
                FightRoom room = fightCache.GetRoomByUId(userId);

                if(result==true)
                {
                    //设置地主身份
                    room.SetLandlord(userId);
                    //广播谁当了地主,3张底牌
                    GrabDto dto = new GrabDto(userId, room.TableCardList,room.GetUserCards(userId));
                    Brocast(room, OpCode.FIGHT, FightCode.GRAB_LANDLORD_BRO, dto);

                    //发一个出牌响应
                    ClientPeer nextClient = userCache.GetClientPeer(userId);
                    nextClient.Send(OpCode.FIGHT, FightCode.TURN_DEAL_BRO, userId);
                }
                else
                {
                    //下一位
                    Brocast(room, OpCode.FIGHT, FightCode.TURN_DEAL_BRO, userId);
                }
            });
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartFight(List<int> uidList)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //创建战斗房间
                FightRoom room = fightCache.Create(uidList);
                room.InitPlayerCards();
                room.Sort();
                //发送给每个客户端,自身有什么牌
                foreach (int uid in uidList)
                {
                    ClientPeer client = userCache.GetClientPeer(uid);
                    List<CardDto> cardList = room.GetUserCards(uid);
                    client.Send(OpCode.FIGHT, FightCode.GET_CARD_SRES, cardList);
                }

                //开始抢地主
                int firstUserId = room.GetFirstUId();
                Brocast(room, OpCode.FIGHT, FightCode.TURN_GRAB_BRO, firstUserId, null);
            });
        }

        /// <summary>
        /// 广播房间内的所有玩家信息
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="subCode"></param>
        /// <param name="value"></param>
        public void Brocast(FightRoom room,int opCode, int subCode, object value, ClientPeer exClient = null)
        {
            SocketMsg msg = new SocketMsg(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);

            foreach (var player in room.PlayerList)
            {
                if (userCache.IsOnline(player.UserId))
                {
                    ClientPeer client = userCache.GetClientPeer(player.UserId);
                    if (client == exClient)
                        continue;
                    client.Send(packet);
                }
            }
        }
    }
}
