using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Code
{
    public class AccountCode
    {
        /// <summary>
        /// 客户端注册请求
        /// </summary>
        public const int REGIST_CREQ = 0;
        /// <summary>
        /// 服务器注册响应
        /// </summary>
        public const int REGIST_SRES = 1;
        /// <summary>
        /// 客户端登录
        /// </summary>
        public const int LOGIN = 2;
    }
}
