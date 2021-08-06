using AhpilyServer.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AhpilyServer.Timer
{
    /// <summary>
    /// 定时任务(计时器)管理类
    /// </summary>
    public class TimerManager
    {
        private static TimerManager instance = null;
        public static TimerManager Instance
        {
            get
            {
                lock(instance)
                {
                    if (instance == null)
                        instance = new TimerManager();
                    return instance;
                }
            }
        }

        /// <summary>
        /// 实现定时器主要功能的timer类
        /// </summary>
        private System.Timers.Timer timer;

        /// <summary>
        /// 存储 任务ID 和 任务模型 的映射
        /// </summary>
        private ConcurrentDictionary<int, TimerModel> idModelDict = new ConcurrentDictionary<int, TimerModel>();

        /// <summary>
        /// 要移出的任务ID列表
        /// </summary>
        private List<int> removeList = new List<int>();

        /// <summary>
        /// 用来表示ID
        /// </summary>
        private ConcurrentInt id = new ConcurrentInt(-1);

        public TimerManager()
        {
            timer = new System.Timers.Timer(10);
            timer.Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// 达到时间间隔时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock(removeList)
            {
                TimerModel tmpModel = null;
                foreach (var id in removeList)
                {
                    idModelDict.TryRemove(id, out tmpModel);
                }
                removeList.Clear();
            }

            foreach (var model in idModelDict.Values)
            {
                if(model.Time<=DateTime.Now.Ticks)
                    model.Run();
            }
        }

        /// <summary>
        /// 添加定时任务,指定触发的时间
        /// </summary>
        public void AddTimeEvent(DateTime dateTime,TimerDelegate timerDelegate)
        {
            long delayTime = dateTime.Ticks - DateTime.Now.Ticks;
            if (delayTime <= 0)
                return;
            AddTimeEvent(delayTime, timerDelegate);
        }

        /// <summary>
        /// 添加定时任务,指定延迟的时间
        /// </summary>
        /// <param name="delayTime">毫秒</param>
        /// <param name="timerDelegate"></param>
        public void AddTimeEvent(long delayTime, TimerDelegate timerDelegate)
        {
            TimerModel model = new TimerModel(id.Add_Get(), DateTime.Now.Ticks + delayTime, timerDelegate);
            idModelDict.TryAdd(model.Id, model);
        }
    }
}
