using System;
using System.Collections.Generic;
using System.Text;

namespace TypingManager
{
    /// <summary>
    /// アプリケーション別の打鍵数のログを取る
    /// StrokeNumLogクラスから使用される
    /// </summary>
    public class AppKeyLog : ICloneable
    {
        class MinuteLog
        {
            Dictionary<string, int> min_log = new Dictionary<string, int>();
            int total;
            int minute_index;

            #region プロパティ...
            public int Total
            {
                get { return total; }
                set { total = value; }
            }
            public int MinuteIndex
            {
                get { return minute_index; }
                set { minute_index = value; }
            }
            public List<string> TitleValues
            {
                get { return new List<string>(min_log.Keys); }
            }
            public int this[string index]
            {
                get
                {
                    if (min_log.ContainsKey(index))
                    {
                        return min_log[index];
                    }
                    return 0;
                }
            }
            #endregion

            public MinuteLog(int _today_minute)
            {
                minute_index = _today_minute;
            }

            public MinuteLog(int _today_minute, string title, int num)
            {
                minute_index = _today_minute;
                SetTitle(title, num);
            }

            public void SetTitle(string title, int num)
            {
                if (title != "")
                {
                    min_log[title] = num;
                }
            }

            public void Stroke(string title)
            {
                total++;
                if (title != "")
                {
                    if (min_log.ContainsKey(title))
                    {
                        min_log[title]++;
                    }
                    else
                    {
                        min_log[title] = 1;
                    }
                }
            }
        }

        // 今日の合計打鍵数
        private int total;

        // 当日の1分ごとの打鍵数
        private List<MinuteLog> per_minute = new List<MinuteLog>();

        // <hour * 60 + minute, MinuteLogのindex>
        private Dictionary<int, int> minlog_dic = new Dictionary<int, int>();

        private int app_id;

        #region プロパティ...
        public int this[int index]
        {
            get
            {
                if (minlog_dic.ContainsKey(index))
                {
                    return per_minute[minlog_dic[index]].Total;
                }
                return 0;
            }
        }
        public int Total
        {
            get { return total; }
            set { total = value; }
        }
        public int AppID
        {
            get { return app_id; }
        }
        #endregion

        public AppKeyLog(int id)
        {
            app_id = id;
        }

        public int GetMinuteTotal(int hour, int minute)
        {
            return this[hour * 60 + minute];
        }

        public int GetHourTotal(int hour)
        {
            int hour_total = 0;
            for (int i = 0; i < 60; i++)
            {
                hour_total += this[hour * 60 + i];
            }
            return hour_total;
        }

        /// <summary>
        /// その日1日にこのプロセスの指定されたタイトルで打った数を返す
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public int GetTitleTotal(string title)
        {
            int title_total = 0;
            foreach (MinuteLog log in per_minute)
            {
                title_total += log[title];
            }
            return title_total;
        }

        /// <summary>
        /// その日1日にこのプロセスで打ったタイトルをすべて列挙する
        /// </summary>
        /// <returns></returns>
        public List<string> GetTitleList()
        {
            // 重複する要素を取り除くために使う
            Dictionary<string, int> title_list = new Dictionary<string, int>();
            foreach (MinuteLog log in per_minute)
            {
                foreach (string title in log.TitleValues)
                {
                    title_list[title] = 1;
                }
            }
            return new List<string>(title_list.Keys);
        }

        /// <summary>
        /// 指定した時間にこのプロセスが打ったタイトルを列挙する
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        public List<string> GetTitleList(int hour, int minute)
        {
            int index = hour * 60 + minute;
            if (minlog_dic.ContainsKey(index))
            {
                return per_minute[minlog_dic[index]].TitleValues;
            }
            return new List<string>();
        }

        /// <summary>
        /// 指定した時間にこのプロセスの指定したタイトルで打った打鍵数を取得
        /// </summary>
        /// <param name="title"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        public int GetTitleStroke(string title, int hour, int minute)
        {
            int index = hour * 60 + minute;
            if (minlog_dic.ContainsKey(index))
            {
                return per_minute[minlog_dic[index]][title];
            }
            return 0;
        }

        public void SetMinuteTitle(string title, int hour, int minute, int num)
        {
            int index = hour * 60 + minute;
            if (!minlog_dic.ContainsKey(index))
            {
                MinuteLog log = new MinuteLog(index);
                per_minute.Add(log);
                minlog_dic[index] = per_minute.Count - 1;
            }
            per_minute[minlog_dic[index]].SetTitle(title, num);
        }

        public void SetMinuteTotal(int hour, int minute, int num)
        {
            int index = hour * 60 + minute;
            if (!minlog_dic.ContainsKey(index))
            {
                MinuteLog log = new MinuteLog(index);
                per_minute.Add(log);
                minlog_dic[index] = per_minute.Count - 1;
            }
            per_minute[minlog_dic[index]].Total = num;
        }

        public void Sort()
        {
            per_minute.Sort(delegate(MinuteLog a, MinuteLog b)
            {
                return a.MinuteIndex - b.MinuteIndex;
            });

            minlog_dic.Clear();
            for (int i = 0; i < per_minute.Count; i++)
            {
                int index = per_minute[i].MinuteIndex;
                minlog_dic[index] = i;
            }
        }

        public void Stroke(string title, int hour, int minute)
        {
            total++;
            int index = hour * 60 + minute;
            if (!minlog_dic.ContainsKey(index))
            {
                MinuteLog log = new MinuteLog(index);
                per_minute.Add(log);
                minlog_dic[index] = per_minute.Count - 1;
            }
            per_minute[minlog_dic[index]].Stroke(title);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
