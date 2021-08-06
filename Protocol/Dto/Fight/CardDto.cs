using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto.Fight
{
    /// <summary>
    /// 卡牌
    /// </summary>
    [Serializable]
    public class CardDto
    {
        public string Name;
        /// <summary>
        /// 花色
        /// </summary>
        public int Color;
        /// <summary>
        /// 值
        /// </summary>
        public int Weight;

        public CardDto()
        {

        }

        public CardDto(string name,int color,int weight)
        {
            this.Name = name;
            this.Color = color;
            this.Weight = weight;
        }
    }
}
