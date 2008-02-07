using System;
using System.Collections.Generic;
using System.Text;

namespace TypingManager
{
    public interface ITimerTask
    {
        void TimerTask(DateTime date, int id);
    }

    /// <summary>
    /// 
    /// </summary>
    class TimerTaskException : Exception
    {
        public TimerTaskException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// 任意の指定された間隔ごとにITimerTaskのTaskを呼び出す
    /// </summary>
    public class TimerTaskController
    {
        class TaskBase : ITimerTask
        {
            public ITimerTask Task;
            public int Hour;
            public int Min;
            public int Sec;
            public int TaskID;

            public TaskBase(ITimerTask task, int id, int hour, int min, int sec)
            {
                Task = task;
                Hour = hour;
                Min = min;
                Sec = sec;
                TaskID = id;
            }

            public void TimerTask(DateTime date, int id)
            {
                int now_sec = date.Hour * 60 * 60 + date.Minute * 60 + date.Second;
                int task_sec = Hour * 60 * 60 + Min * 60 + Sec;
                if (now_sec % task_sec == 0)
                {
                    Task.TimerTask(date, id);
                }
            }
        }

        private List<TaskBase> timer_task = new List<TaskBase>();

        public TimerTaskController()
        {

        }

        /// <summary>
        /// TimerHandlerのTaskは基本的に一秒ごとに呼び出すこと
        /// </summary>
        public void CallTask(DateTime date)
        {
            foreach (TaskBase task in timer_task)
            {
                task.TimerTask(date, task.TaskID);
            }
        }

        /// <summary>
        /// どれくらいの間隔ごとに呼び出されたいかを指定してITimerTaskを追加する
        /// </summary>
        /// <param name="task"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        public void AddTask(ITimerTask task, int id, int hour, int min, int sec)
        {
            if (hour == 0 && min == 0 && sec == 0)
            {
                throw new TimerTaskException("TimerTaskを呼び出す間隔を0にすることはできません");
            }
            TaskBase task_base = new TaskBase(task, id, hour, min, sec);
            timer_task.Add(task_base);
        }
    }
}
