using Protocol.Constant;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Fight
{
    /// <summary>
    /// 战斗房间
    /// </summary>
    public class FightRoom
    {
        /// <summary>
        /// 房间id
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 存储所有玩家
        /// </summary>
        public List<PlayerDto> PlayerList { get; set; }

        /// <summary>
        /// 中途退出的玩家的UId列表
        /// </summary>
        public List<int> LeaveUIdList { get; set; }

        /// <summary>
        /// 牌库
        /// </summary>
        public LibraryModel LibraryModel { get; set; }

        /// <summary>
        /// 底牌
        /// </summary>
        public List<CardDto> TableCardList { get; set; }

        /// <summary>
        /// 倍数
        /// </summary>
        public int Multiple { get; set; }

        /// <summary>
        /// 回合管理
        /// </summary>
        public RoundModel RoundModel { get; set; }

        public FightRoom(int id,List<int> uidList)
        {
            this.Id = id;
            this.PlayerList = new List<PlayerDto>();
            foreach (int uid in uidList)
            {
                PlayerDto player = new PlayerDto(uid);
                this.PlayerList.Add(player);
            }
            this.LeaveUIdList = new List<int>();
            this.LibraryModel = new LibraryModel();
            this.TableCardList = new List<CardDto>();
            this.Multiple = 1;
            this.RoundModel = new RoundModel();
        }

        public void Init(List<int> uidList)
        {
            foreach (int uid in uidList)
            {
                PlayerDto player = new PlayerDto(uid);
                this.PlayerList.Add(player);
            }
        }

        /// <summary>
        /// 转换出牌
        /// </summary>
        public int Turn()
        {
            int currUId = RoundModel.CurrentUId;
            int nextUId = GetNextUId(currUId);
            //更改回合信息
            RoundModel.CurrentUId = nextUId;
            return nextUId;
        }

        /// <summary>
        /// 计算下一个出牌者
        /// </summary>
        /// <param name="currId">当前出牌者</param>
        /// <returns></returns>
        public int GetNextUId(int currId)
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if(PlayerList[i].UserId==currId)
                {
                    if (i == 2)
                        return PlayerList[0].UserId;
                    else
                        return PlayerList[i + 1].UserId;
                }
            }
            throw new Exception("没有出牌者");
        }

        /// <summary>
        /// 判断能不能管上上一回合的牌
        /// </summary>
        /// <returns></returns>
        public bool DealCard(int type,int weight,int length,int userId,List<CardDto> cardList)
        {
            bool canDeal = false;
            //相同牌型,且权值大
            if(type==RoundModel.LastCardType&&weight>RoundModel.LastWeight)
            {
                //顺子
                if(type==CardType.STRAIGHT|| type == CardType.DOUBLE_STRAIGHT|| type == CardType.TRIPLE_STRAIGHT)
                {
                    if(length==RoundModel.LastLength)
                    {
                        canDeal = true;
                    }
                }
                else
                {
                    canDeal = true;
                }
            }
            //炸弹
            else if(type==CardType.BOOM && RoundModel.LastCardType!=CardType.BOOM)
            {
                canDeal = true;
            }
            //王炸
            else if(type==CardType.JOKER_BOOM)
            {
                canDeal = true;
            }
            //第一次出牌 或者自己是最大出牌者
            else if(userId==RoundModel.BiggestUId)
            {
                canDeal = true;
            }

            //出牌
            if(canDeal)
            {
                //移除玩家手牌
                RemoveCards(userId, cardList);
                //是否翻倍
                if(type==CardType.BOOM)
                {
                    Multiple *= 2;
                }
                else if(type==CardType.JOKER_BOOM)
                {
                    Multiple *= 4;
                }
                //保存回合信息
                RoundModel.Change(length, type, weight, userId);
            }

            return canDeal;
        }

        /// <summary>
        /// 移除玩家手牌
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cardList"></param>
        private void RemoveCards(int userId,List<CardDto> cardList)
        {
            //获取玩家现有手牌
            List<CardDto> currList = GetUserCards(userId);

            List<CardDto> list = new List<CardDto>();
            foreach (var select in cardList)
            {
                for (int i = currList.Count-1; i >=0; i--)
                {
                    if(currList[i].Name==select.Name)
                    {
                        list.Add(currList[i]);
                        break;
                    }
                }
            }
            foreach (var card in list)
            {
                currList.Remove(card);
            }
            //for (int i = currList.Count-1; i >=0; i--)
            //{
            //    foreach (CardDto temp in cardList)
            //    {
            //        if(currList[i].Name==temp.Name)
            //        {
            //            currList.Remove(currList[i]);
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 获取玩家的现有手牌
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CardDto> GetUserCards(int userId)
        {
            foreach (PlayerDto player in PlayerList)
            {
                if (player.UserId == userId)
                    return player.CardList;
            }
            throw new Exception("不存在这个玩家");
        }

        /// <summary>
        /// 发牌
        /// </summary>
        public void InitPlayerCards()
        {
            //一人17张,剩3张底牌
            for (int i = 0; i < 17; i++)
            {
                CardDto card = LibraryModel.Deal();
                PlayerList[0].Add(card);
            }
            for (int i = 0; i < 17; i++)
            {
                CardDto card = LibraryModel.Deal();
                PlayerList[1].Add(card);
            }
            for (int i = 0; i < 17; i++)
            {
                CardDto card = LibraryModel.Deal();
                PlayerList[2].Add(card);
            }
            //底牌
            for (int i = 0; i < 3; i++)
            {
                CardDto card = LibraryModel.Deal();
                TableCardList.Add(card);
            }
        }

        /// <summary>
        /// 设置地主身份
        /// </summary>
        public void SetLandlord(int userId)
        {
            foreach (PlayerDto player in PlayerList)
            {
                if (player.UserId == userId)
                {
                    //改变身份
                    player.Identity = Identity.LANDLORD;
                    //发底牌
                    for (int i = 0; i < TableCardList.Count; i++)
                    {
                        player.Add(TableCardList[i]);
                    }
                    Sort();
                    //开始回合
                    RoundModel.Start(userId);
                }
            }
        }

        /// <summary>
        /// 获取玩家数据模型
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PlayerDto GetPlayerModel(int userId)
        {
            foreach (PlayerDto player in PlayerList)
            {
                if(player.UserId==userId)
                {
                    return player;
                }
            }
            throw new Exception("没有这个玩家");
        }

        /// <summary>
        /// 获取玩家的身份
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetPlayerIdentity(int userId)
        {
            return GetPlayerModel(userId).Identity;
        }

        /// <summary>
        /// 获取相同身份的用户id
        /// </summary>
        /// <returns></returns>
        public List<int> GetSameIdentityUIds(int identity)
        {
            List<int> uids = new List<int>();
            foreach (PlayerDto player in PlayerList)
            {
                if(player.Identity==identity)
                {
                    uids.Add(player.UserId);
                }
            }
            return uids;
        }

        /// <summary>
        /// 获取不同身份的用户id
        /// </summary>
        /// <returns></returns>
        public List<int> GetDifferentIdentityUIds(int identity)
        {
            List<int> uids = new List<int>();
            foreach (PlayerDto player in PlayerList)
            {
                if (player.Identity != identity)
                {
                    uids.Add(player.UserId);
                }
            }
            return uids;
        }

        /// <summary>
        /// 获取房间内第一个玩家的ID
        /// </summary>
        /// <returns></returns>
        public int GetFirstUId()
        {
            return PlayerList[0].UserId;
        }

        /// <summary>
        /// 手牌排序
        /// </summary>
        /// <param name="cardList"></param>
        /// <param name="asc">升序</param>
        private void SortCard(List<CardDto> cardList,bool asc=true)
        {
            cardList.Sort(
                delegate (CardDto a, CardDto b)
                {
                    if (asc)
                    {
                        return a.Weight.CompareTo(b.Weight);
                    }
                    else
                    {
                        return a.Weight.CompareTo(b.Weight) * -1;
                    }
                });
        }

        /// <summary>
        /// 排序,默认升序
        /// </summary>
        public void Sort(bool asc=true)
        {
            SortCard(PlayerList[0].CardList, asc);
            SortCard(PlayerList[1].CardList, asc);
            SortCard(PlayerList[2].CardList, asc);
            SortCard(TableCardList, asc);
        }
    }
}
