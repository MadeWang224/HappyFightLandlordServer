using AhpilyServer;
using GameServer.Cache;
using GameServer.Model;
using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic
{
    /// <summary>
    /// 用户逻辑处理层
    /// </summary>
    public class UserHandler : IHandler
    {
        private UserCache userCache = Caches.User;
        private AccountCache accountCache = Caches.Account;

        public void OnDisconnect(ClientPeer client)
        {
            if (userCache.IsOnline(client))
                userCache.Offline(client);
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case UserCode.CREATE_CREQ:
                    Create(client, value.ToString());
                    break;
                case UserCode.GET_INFO_CREQ:
                    GetInfo(client);
                    break;
                case UserCode.ONLINE_CREQ:
                    Online(client);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="client">客户端连接对象</param>
        /// <param name="name">客户端传输过来的名字</param>
        private void Create(ClientPeer client,string name)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //判断客户端是不是非法登录
                if (!accountCache.IsOnline(client))
                {
                    client.Send(OpCode.USER, UserCode.CREATE_SRES, -1);
                    return;
                }
                //获取账号id
                int accountId = accountCache.GetId(client);
                //判断这个账号以前有没有角色
                if (userCache.IsExist(accountId))
                {
                    client.Send(OpCode.USER, UserCode.CREATE_SRES, -2);
                    return;
                }
                userCache.Create(name, accountId);
                client.Send(OpCode.USER, UserCode.CREATE_SRES, 0);
            });
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <param name="client"></param>
        private void GetInfo(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //判断客户端是不是非法登录
                if (!accountCache.IsOnline(client))
                {
                    client.Send(OpCode.USER, UserCode.GET_INFO_SRES,null);
                    return;
                }
                int accountId = accountCache.GetId(client);
                //判断这个账号以前有没有角色
                if (userCache.IsExist(accountId) == false)
                {
                    client.Send(OpCode.USER, UserCode.GET_INFO_SRES,null);
                    return;
                }
                //有角色
                //上线角色
                if(userCache.IsOnline(client)==false)
                    Online(client);
                //给客户端返回消息
                UserModel model = userCache.GetModelByAccountId(accountId);
                UserDto dto = new UserDto(model.Id,model.Name, model.Been, model.WinCount, model.LoseCount, model.RunCount, model.Lv, model.Exp);
                client.Send(OpCode.USER, UserCode.GET_INFO_SRES, dto);
            });
        }

        /// <summary>
        /// 角色上线
        /// </summary>
        /// <param name="client"></param>
        private void Online(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //判断客户端是不是非法登录
                if (!accountCache.IsOnline(client))
                {
                    client.Send(OpCode.USER, UserCode.ONLINE_SRES, -1);
                    return;
                }
                int accountId = accountCache.GetId(client);
                //判断这个账号以前有没有角色
                if (userCache.IsExist(accountId) == false)
                {
                    client.Send(OpCode.USER, UserCode.ONLINE_SRES, -2);
                    return;
                }
                int userId = userCache.GetId(accountId);
                userCache.Online(client, userId);
                client.Send(OpCode.USER, UserCode.ONLINE_SRES, 0);
            });
        }
    }
}
