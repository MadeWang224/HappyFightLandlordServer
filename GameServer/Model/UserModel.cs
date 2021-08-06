using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    /// <summary>
    /// 角色数据模型
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// 唯一id
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
        /// <summary>
        /// 外键:与这个角色账号关联的账号id
        /// </summary>
        public int AccountId;

        public UserModel(int id,string name,int accountId)
        {
            this.Id = id;
            this.Name = name;
            this.Been = 10000;
            this.WinCount = 0;
            this.LoseCount = 0;
            this.RunCount = 0;
            this.Lv = 1;
            this.Exp = 0;
            this.AccountId = accountId;
        }
    }
}
