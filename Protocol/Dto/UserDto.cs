using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
    /// <summary>
    /// 用户数据的传输模型
    /// </summary>
    [Serializable]
    public class UserDto
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int Id;
        /// <summary>
        /// 用户名字
        /// </summary>
        public string Name;
        /// <summary>
        /// 欢乐豆数量
        /// </summary>
        public int Been;
        /// <summary>
        /// 胜场
        /// </summary>
        public int WinCount;
        /// <summary>
        /// 负场
        /// </summary>
        public int LoseCount;
        /// <summary>
        /// 逃跑场次
        /// </summary>
        public int RunCount;
        /// <summary>
        /// 等级
        /// </summary>
        public int Lv;
        /// <summary>
        /// 经验
        /// </summary>
        public int Exp;

        public UserDto()
        {

        }
        public UserDto(int id,string name,int been,int winCount,int loseCount,int runCount,int lv,int exp)
        {
            this.Id = id;
            this.Name = name;
            this.Been = been;
            this.WinCount = winCount;
            this.LoseCount = loseCount;
            this.RunCount = runCount;
            this.Lv = lv;
            this.Exp = exp;
        }
    }
}
