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
    /// �C�ӂ̎w�肳�ꂽ�Ԋu���Ƃ�ITimerTask��Task���Ăяo��
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
                if (task_sec > 0 && now_sec % task_sec == 0)
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
        /// TimerHandler��Task�͊�{�I�Ɉ�b���ƂɌĂяo������
        /// </summary>
        public void CallTask(DateTime date)
        {
            foreach (TaskBase task in timer_task)
            {
                task.TimerTask(date, task.TaskID);
            }
        }

        /// <summary>
        /// �ǂꂭ�炢�̊Ԋu���ƂɌĂяo���ꂽ�������w�肵��ITimerTask��ǉ�����
        /// </summary>
        /// <param name="task"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        public void AddTask(ITimerTask task, int id, int hour, int min, int sec)
        {
            TaskBase task_base = new TaskBase(task, id, hour, min, sec);
            //Console.WriteLine("TimerHandler::AddTask");
            for (int i = 0; i < timer_task.Count; i++)
            {
                TaskBase t_task = timer_task[i];
                if (ITimerTask.ReferenceEquals(task, t_task.Task))
                {
                    if (id == t_task.TaskID)
                    {
                        timer_task.RemoveAt(i);
                        //Console.WriteLine("�^�X�N����폜���܂���");
                        break;
                    }
                }
            }
            timer_task.Add(task_base);
            //Console.WriteLine("TimerTask Num={0}", timer_task.Count);
        }
    }
}
