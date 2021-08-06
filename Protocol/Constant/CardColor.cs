using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Constant
{
    /// <summary>
    /// 卡牌花色
    /// </summary>
    public class CardColor
    {
        /// <summary>
        /// 没有牌
        /// </summary>
        public const int NONE = 0;
        /// <summary>
        /// 梅花
        /// </summary>
        public const int CLUB = 1;
        /// <summary>
        /// 红桃
        /// </summary>
        public const int HEART = 2;
        /// <summary>
        /// 黑桃
        /// </summary>
        public const int SPADE = 3;
        /// <summary>
        /// 方片
        /// </summary>
        public const int SQUARE = 4;

        public static string GetString(int color)
        {
            switch (color)
            {
                case CLUB:
                    return "Club";
                case HEART:
                    return "Heart";
                case SPADE:
                    return "Spade";
                case SQUARE:
                    return "Square";
                default:
                    throw new Exception("不存在此花色");
            }
        }
    }
}
