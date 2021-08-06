using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Code
{
    public class FightCode
    {
        /// <summary>
        /// 抢地主的请求
        /// </summary>
        public const int GRAB_LANDLORD_CREQ = 0;
        /// <summary>
        /// 广播抢地主的结果
        /// </summary>
        public const int GRAB_LANDLORD_BRO = 1;
        /// <summary>
        /// 广播下一个玩家抢地主
        /// </summary>
        public const int TURN_GRAB_BRO = 2;

        /// <summary>
        /// 客户端出牌请求
        /// </summary>
        public const int DEAL_CREQ = 3;
        /// <summary>
        /// 出牌的响应
        /// </summary>
        public const int DEAL_SRES = 4;
        /// <summary>
        /// 出牌的广播
        /// </summary>
        public const int DEAL_BRO = 5;

        /// <summary>
        /// 不出的请求
        /// </summary>
        public const int PASS_CREQ = 6;
        /// <summary>
        /// 不出的响应
        /// </summary>
        public const int PASS_SRES = 7;

        /// <summary>
        /// 转换出牌的广播
        /// </summary>
        public const int TURN_DEAL_BRO = 8;

        /// <summary>
        /// 玩家退出游戏的广播
        /// </summary>
        public const int LEAVE_BRO = 9;

        /// <summary>
        /// 游戏结束的广播
        /// </summary>
        public const int OVER_BRO = 10;

        /// <summary>
        /// 给牌的响应
        /// </summary>
        public const int GET_CARD_SRES = 11;
    }
}
