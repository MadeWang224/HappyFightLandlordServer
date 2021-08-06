using AhpilyServer;
using GameServer.Cache;
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
    /// 账号处理的逻辑层
    /// </summary>
    public class AccountHandler : IHandler
    {
        AccountCache accountCache = Caches.Account;

        public void OnDisconnect(ClientPeer client)
        {
            if(accountCache.IsOnline(client))
                accountCache.Offline(client);
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case AccountCode.REGIST_CREQ:
                    {
                        AccountDto dto = value as AccountDto;
                        Regist(client, dto.Account, dto.Password);
                    }
                    break;
                case AccountCode.LOGIN:
                    {
                        AccountDto dto = value as AccountDto;
                        Login(client, dto.Account, dto.Password);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="client"></param>
        /// <param name="account"></param>
        /// <param name="password"></param>
        private void Regist(ClientPeer client,string account,string password)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (accountCache.IsExist(account))
                {
                    //账号已存在
                    client.Send(OpCode.ACCOUNT, AccountCode.REGIST_SRES, -1);
                    return;
                }
                if (string.IsNullOrEmpty(account))
                {
                    //账号不能为空
                    client.Send(OpCode.ACCOUNT, AccountCode.REGIST_SRES, -2);
                    return;
                }
                if (string.IsNullOrEmpty(password) || password.Length < 4 || password.Length > 16)
                {
                    //密码不合法
                    client.Send(OpCode.ACCOUNT, AccountCode.REGIST_SRES, -3);
                    return;
                }
                //验证完成,进行注册
                accountCache.Create(account, password);
                //注册成功
                client.Send(OpCode.ACCOUNT, AccountCode.REGIST_SRES, 0);
            });
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="client"></param>
        /// <param name="account"></param>
        /// <param name="password"></param>
        private void Login(ClientPeer client,string account,string password)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (!accountCache.IsExist(account))
                {
                    //账号不存在
                    client.Send(OpCode.ACCOUNT, AccountCode.LOGIN, -1);
                    return;
                }
                if (accountCache.IsOnline(account))
                {
                    //账号在线
                    client.Send(OpCode.ACCOUNT, AccountCode.LOGIN, -2);
                    return;
                }
                if (!accountCache.IsMatch(account, password))
                {
                    //账号密码错误
                    client.Send(OpCode.ACCOUNT, AccountCode.LOGIN, -3);
                    return;
                }
                //验证完毕,登录
                accountCache.Online(client, account);
                //登录成功
                client.Send(OpCode.ACCOUNT, AccountCode.LOGIN, 0);
            });
        }
    }
}
