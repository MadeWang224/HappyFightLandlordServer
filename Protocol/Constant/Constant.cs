using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Constant
{
    /// <summary>
    /// 聊天类型对应的文字
    /// </summary>
    public static class Constant
    {
        private static Dictionary<int, string> typeTextDict = new Dictionary<int, string>();

        static Constant()
        {
            typeTextDict = new Dictionary<int, string>();

            typeTextDict.Add(1, "大家好,很高心见到各位~");
            typeTextDict.Add(2, "和你合作真是太愉快啦!");
            typeTextDict.Add(3, "快点吧,我等的花都谢了.");
            typeTextDict.Add(4, "你的牌打得太好了!");
            typeTextDict.Add(5, "不要吵了,有什么好吵的,专心玩游戏吧!");
            typeTextDict.Add(6, "不要走,决战到天亮!");
            typeTextDict.Add(7, "再见了,我会想念大家的~");
        }

        public static string GetChatText(int chatType)
        {
            return typeTextDict[chatType];
        }
    }
}
