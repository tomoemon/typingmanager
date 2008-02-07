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
    /// ”CˆÓ‚Ìw’è‚³‚ê‚½ŠÔŠu‚²‚Æ‚ÉITimerTask‚ÌTask‚ğŒÄ‚Ño‚·
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
        /// TimerHandler‚ÌTask‚ÍŠî–{“I‚Éˆê•b‚²‚Æ‚ÉŒÄ‚Ño‚·‚±‚Æ
        /// </summary>
        public void CallTask(DateTime date)
        {
            foreach (TaskBase task in timer_task)
            {
                task.TimerTask(date, task.TaskID);
            }
        }

        /// <summary>
        /// ‚Ç‚ê‚­‚ç‚¢‚ÌŠÔŠu‚²‚Æ‚ÉŒÄ‚Ño‚³‚ê‚½‚¢‚©‚ğw’è‚µ‚ÄITimerTask‚ğ’Ç‰Á‚·‚é
        /// </summary>
        /// <param name="task"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        public void AddTask(ITimerTask task, int id, int hour, int min, int sec)
        {
            if (hour == 0 && min == 0 && sec == 0)
            {
                throw new TimerTaskException("TimerTask‚ğŒÄ‚Ño‚·ŠÔŠu‚ğ0‚É‚·‚é‚±‚Æ‚Í‚Å‚«‚Ü‚¹‚ñ");
            }
            TaskBase task_base = new TaskBase(task, id, hour, min, sec);
            timer_task.Add(task_base);
        }
    }
}
