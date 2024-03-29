﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
    /// <summary>
    /// 匹配房间数据的传输模型
    /// </summary>
    [Serializable]
    public class MatchRoomDto
    {
        /// <summary>
        /// 用户id对应的用户数据的传输模型
        /// </summary>
        public Dictionary<int, UserDto> UIdUserDict;

        /// <summary>
        /// 准备的玩家的id列表
        /// </summary>
        public List<int> ReadyUIdList;

        /// <summary>
        /// 存储玩家进入的顺序
        /// </summary>
        public List<int> uIdList;

        public MatchRoomDto()
        {
            this.UIdUserDict = new Dictionary<int, UserDto>();
            this.ReadyUIdList = new List<int>();
            this.uIdList = new List<int>();
        }

        public void Add(UserDto newUser)
        {
            UIdUserDict.Add(newUser.Id, newUser);
            this.uIdList.Add(newUser.Id);
        }

        public void Leave(int userId)
        {
            UIdUserDict.Remove(userId);
            this.uIdList.Remove(userId);
        }

        public void Ready(int userId)
        {
            ReadyUIdList.Add(userId);
        }

        /// <summary>
        /// 左边玩家的ID
        /// </summary>
        public int LeftId;
        /// <summary>
        /// 右边玩家的ID
        /// </summary>
        public int RightId;

        /// <summary>
        /// 重置位置:在玩家进入或离开房间的时候,就需要调整位置
        /// </summary>
        public void ResetPosition(int myUserId)
        {
            LeftId = -1;
            RightId = -1;

            //1
            if(uIdList.Count==1)
            {

            }
            //2
            else if(uIdList.Count==2)
            {
                if(uIdList[0]==myUserId)
                {
                    RightId = uIdList[1];
                }
                if(uIdList[1]==myUserId)
                {
                    LeftId = uIdList[0];
                }
            }
            //3
            else if(uIdList.Count==3)
            {
                if(uIdList[0]==myUserId)
                {
                    LeftId = uIdList[2];
                    RightId = uIdList[1];
                }
                if (uIdList[1] == myUserId)
                {
                    LeftId = uIdList[0];
                    RightId = uIdList[2];
                }
                if (uIdList[2] == myUserId)
                {
                    LeftId = uIdList[1];
                    RightId = uIdList[0];
                }
            }
        }
    }
}
