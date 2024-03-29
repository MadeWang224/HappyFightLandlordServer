﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AhpilyServer
{
    /// <summary>
    /// 关于编码的工具类
    /// </summary>
    public static class EncodeTool
    {
        #region 粘包拆包问题 封装一个有规定的数据包

        /// <summary>
        /// 构造数据包:包头+包尾
        /// </summary>
        /// <returns></returns>
        public static byte[] EncodePacket(byte[] data)
        {
            //内存流对象
            using (MemoryStream ms=new MemoryStream())
            {
                using (BinaryWriter bw=new BinaryWriter(ms))
                {
                    //先写入长度
                    bw.Write(data.Length);
                    //再写入数据
                    bw.Write(data);

                    byte[] byteArray = new byte[(int)ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, byteArray, 0, (int)ms.Length);

                    return byteArray;
                }
            }
        }

        /// <summary>
        /// 解析消息体:从缓存里取出一个一个完整的数据包
        /// </summary>
        /// <returns></returns>
        public static byte[] DecodePacket(ref List<byte> dataCache)
        {
            //四个字节 构成一个int长度 不能构成一个完整的信息
            if (dataCache.Count < 4)
                return null;
                //throw new Exception("数据缓存长度不足4,不能构成一个完整的消息");

            using (MemoryStream ms = new MemoryStream(dataCache.ToArray()))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    int length = br.ReadInt32();
                    int dataRemainLenght = (int)(ms.Length - ms.Position);
                    if (length > dataRemainLenght)
                        return null;
                        //throw new Exception("数据长度不够包头约定的长度,不能构成一个完整的消息");

                    byte[] data = br.ReadBytes(length);
                    //更新缓存区
                    dataCache.Clear();
                    dataCache.AddRange(br.ReadBytes(dataRemainLenght));

                    return data;
                }
            }
        }

        #endregion

        #region 构造发送的SocketMsg类

        /// <summary>
        /// 把SocketMsg类转换成字节数组发送
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] EncodeMsg(SocketMsg msg)
        {
            MemoryStream ms = new MemoryStream(0);
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(msg.OpCode);
            bw.Write(msg.SubCode);
            //有数据,才需要把object转换成字节数据,存起来
            if(msg.Value!=null)
            {
                byte[] valueBytes = EncodeObj(msg.Value);
                bw.Write(valueBytes);
            }
            byte[] data = new byte[ms.Length];
            Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int)ms.Length);
            bw.Close();
            ms.Close();
            return data;
        }

        /// <summary>
        /// 将收到的字节数据,转换成SocketMsg对象,供使用
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static SocketMsg DecodeMsg(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms);
            SocketMsg msg = new SocketMsg();
            msg.OpCode = br.ReadInt32();
            msg.SubCode = br.ReadInt32();
            //还有剩余的字节没读取,代表value有值
            if(ms.Length>ms.Position)
            {
                byte[] valueBytes = br.ReadBytes((int)(ms.Length - ms.Position));
                object value = DecodeObj(valueBytes);
                msg.Value = value;
            }
            br.Close();
            ms.Close();
            return msg;
        }

        #endregion

        #region 把一个object类型转换成byte[]

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] EncodeObj(object value)
        {
            using (MemoryStream ms=new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, value);
                byte[] valueBytes = new byte[ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, valueBytes, 0, (int)ms.Length);
                return valueBytes;
            }
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="valueBytes"></param>
        /// <returns></returns>
        public static object DecodeObj(byte[] valueBytes)
        {
            using (MemoryStream ms = new MemoryStream(valueBytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object value = bf.Deserialize(ms);
                return value;
            }
        }

        #endregion
    }
}
