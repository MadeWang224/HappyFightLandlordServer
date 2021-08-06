using AhpilyServer;
using GameServer.Cache;
using GameServer.Cache.Match;
using Protocol.Code;
using Protocol.Constant;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic
{
    public class ChatHandler : IHandler
    {
        private UserCache userCache = Caches.User;
        private MatchCache matchCache = Caches.Match;

        public void OnDisconnect(ClientPeer client)
        {

        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case ChatCode.CREQ:
                    ChatRequest(client, (int)value);
                    break;
                default:
                    break;
            }
        }

        private void ChatRequest(ClientPeer client,int chatType)
        {
            //接收到聊天类型
            //发送者id,发送的内容
            if (userCache.IsOnline(client) == false)
                return;
            int userId = userCache.GetId(client);
            ChatDto dto = new ChatDto(userId, chatType);
            //广播
            if(matchCache.IsMatching(userId))
            {
                MatchRoom mRoom = matchCache.GetRoom(userId);
                mRoom.Brocast(OpCode.CHAT, ChatCode.SRES, dto);
            }
            else
            {
                //TODO
                //战斗房间
            }
        }
    }
}
