using AhpilyServer;
using AhpilyServer.Concurrent;
using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache
{
    /// <summary>
    /// 角色缓存层
    /// </summary>
    public class UserCache
    {
        /// <summary>
        /// 角色id对应角色数据模型的字典
        /// </summary>
        private Dictionary<int, UserModel> idModelDict = new Dictionary<int, UserModel>();

        /// <summary>
        /// 账号id 对应 角色id 的字典
        /// </summary>
        private Dictionary<int, int> accIdUIdDict = new Dictionary<int, int>();

        /// <summary>
        /// 作为角色的id
        /// </summary>
        ConcurrentInt id = new ConcurrentInt(-1);

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name"></param>
        /// <param name="accountId"></param>
        public void Create(string name,int accountId)
        {
            UserModel model = new UserModel(id.Add_Get(), name, accountId);
            //保存到字典
            idModelDict.Add(model.Id, model);
            accIdUIdDict.Add(model.AccountId, model.Id);
        }

        /// <summary>
        /// 判断此账号下是否有角色
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool IsExist(int accountId)
        {
            return accIdUIdDict.ContainsKey(accountId);
        }

        /// <summary>
        /// 更新角色数据
        /// </summary>
        /// <param name="model"></param>
        public void Update(UserModel model)
        {
            idModelDict[model.Id] = model;
        }

        /// <summary>
        /// 根据账号id获取角色数据模型
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public UserModel GetModelByAccountId(int accountId)
        {
            int userId = accIdUIdDict[accountId];
            UserModel model = idModelDict[userId];
            return model;
        }

        /// <summary>
        /// 根据账号id获取角色id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public int GetId(int accountId)
        {
            return accIdUIdDict[accountId];
        }

        /// <summary>
        /// 存储在线玩家的id与client对应
        /// </summary>
        private Dictionary<int, ClientPeer> idClientDict = new Dictionary<int, ClientPeer>();
        private Dictionary<ClientPeer, int> clientIdDict = new Dictionary<ClientPeer, int>();

        /// <summary>
        /// 是否在线
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsOnline(ClientPeer client)
        {
            return clientIdDict.ContainsKey(client);
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline(int id)
        {
            return idClientDict.ContainsKey(id);
        }

        /// <summary>
        /// 角色上线
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        public void Online(ClientPeer client,int id)
        {
            idClientDict.Add(id, client);
            clientIdDict.Add(client, id);
        }

        /// <summary>
        /// 角色下线
        /// </summary>
        /// <param name="client"></param>
        public void Offline(ClientPeer client)
        {
            int id = clientIdDict[client];
            clientIdDict.Remove(client);
            idClientDict.Remove(id);
        }

        /// <summary>
        /// 根据连接对象获取角色数据模型
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public UserModel GetModelByClientPeer(ClientPeer client)
        {
            int id = clientIdDict[client];
            UserModel model = idModelDict[id];
            return model;
        }

        /// <summary>
        /// 根据用户id获取角色数据模型
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserModel GetModelById(int userId)
        {
            UserModel model = idModelDict[userId];
            return model;
        }

        /// <summary>
        /// 根据角色id获取连接对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ClientPeer GetClientPeer(int id)
        {
            return idClientDict[id];
        }

        /// <summary>
        /// 根据在线玩家的连接对象获取角色id
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GetId(ClientPeer client)
        {
            return clientIdDict[client];
        }
    }
}
