using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace TypingManager
{
    public class TotalLog
    {
        public int TotalType;
        public int TotalApp;
        public DateTime Date;

        public TotalLog(int type, int app, DateTime time)
        {
            TotalType = type;
            TotalApp = app;
            Date = time;
        }
    }

    public class AllDayLog
    {
        /// <summary>日付ごとのログ</summary>
        private List<TotalLog> day_log;

        /// <summary>日付とログの対応関係を持つ辞書（同じ日付のログがかぶらないように）</summary>
        private Dictionary<string, int> log_dic;

        public AllDayLog()
        {
            day_log = new List<TotalLog>();
            log_dic = new Dictionary<string, int>();
        }

        #region プロパティ...
        public List<TotalLog> DayLog
        {
            get { return day_log; }
        }
        [XmlIgnoreAttribute]
        public int TodayTotalType
        {
            get
            {
                string today = DateTime.Now.ToString(Plugin.LogDir.DAY_FORMAT);
                if (log_dic.ContainsKey(today))
                {
                    return day_log[log_dic[today]].TotalType;
                }
                return 0;
            }
            set
            {
                string today = DateTime.Now.ToString(Plugin.LogDir.DAY_FORMAT);
                if (log_dic.ContainsKey(today))
                {
                    day_log[log_dic[today]].TotalType = value;
                }
            }
        }
        [XmlIgnoreAttribute]
        public int TodayTotalApp
        {
            get
            {
                string today = DateTime.Now.ToString(Plugin.LogDir.DAY_FORMAT);
                if (log_dic.ContainsKey(today))
                {
                    return day_log[log_dic[today]].TotalApp;
                }
                return 0;
            }
            set
            {
                string today = DateTime.Now.ToString(Plugin.LogDir.DAY_FORMAT);
                if (log_dic.ContainsKey(today))
                {
                    day_log[log_dic[today]].TotalApp = value;
                }
            }
        }
        [XmlIgnoreAttribute]
        public int YesterdayTotalType
        {
            get
           {
               string yesterday = DateTime.Now.AddDays(-1).ToString(Plugin.LogDir.DAY_FORMAT);
               if (log_dic.ContainsKey(yesterday))
               {
                   return day_log[log_dic[yesterday]].TotalType;
               }
               return 0;
            }
        }
        [XmlIgnoreAttribute]
        public int YesterdayTotalApp
        {
            get
            {
                string yesterday = DateTime.Now.AddDays(-1).ToString(Plugin.LogDir.DAY_FORMAT);
                if (log_dic.ContainsKey(yesterday))
                {
                    return day_log[log_dic[yesterday]].TotalApp;
                }
                return 0;
            }
        }
        [XmlIgnoreAttribute]
        public int TotalDay
        {
            get { return day_log.Count; }
        }
        [XmlIgnoreAttribute]
        public int TotalType
        {
            get
            {
                int type = 0;
                foreach (TotalLog log in day_log)
                {
                    type += log.TotalType;
                }
                return type;
            }
        }
        #endregion

        /// <summary>
        /// これまでのログにない新しい日付かどうかチェック
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsNewDay(DateTime date)
        {
            if (!log_dic.ContainsKey(date.ToString(Plugin.LogDir.DAY_FORMAT)))
            {
                return true;
            }
            return false;
        }

        public void NewDay()
        {
            DateTime today = DateTime.Now;
            if (!log_dic.ContainsKey(today.ToString(Plugin.LogDir.DAY_FORMAT)))
            {
                day_log.Add(new TotalLog(0, 0, today));
                log_dic[today.ToString(Plugin.LogDir.DAY_FORMAT)] = day_log.Count - 1;
            }
        }

        /// <summary>
        /// 最後に打っていた日付を返す
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastDate()
        {
            // リストの最後にあるものが今まで打っていた日付
            // システムの日付が最新のログの日付よりも前になる可能性もあるが
            // その場合でも初めての日の打鍵をしているのであれば，
            // リストの最後にその日付が格納されている
            if (day_log.Count > 0)
            {
                return day_log[day_log.Count - 1].Date;
            }
            return DateTime.Now;
        }

        public int GetDayTotalType(DateTime date)
        {
            string day = date.ToString(Plugin.LogDir.DAY_FORMAT);
            if (log_dic.ContainsKey(day))
            {
                return day_log[log_dic[day]].TotalType;
            }
            return 0;
        }

        public int GetDayTotalApp(DateTime date)
        {
            string day = date.ToString(Plugin.LogDir.DAY_FORMAT);
            if (log_dic.ContainsKey(day))
            {
                return day_log[log_dic[day]].TotalApp;
            }
            return 0;
        }

        public int[] GetDayStroke(DateTime start, int count)
        {
            List<int> result = new List<int>();
            for (int i = 0; i <= count; i++)
            {
                string day = start.AddDays(-i).ToString(Plugin.LogDir.DAY_FORMAT);
                if (log_dic.ContainsKey(day))
                {
                    result.Add(day_log[log_dic[day]].TotalType);
                }
                else
                {
                    result.Add(0);
                }
            }
            /*
            List<int> result = new List<int>();
            int back_day = 0;
            for (int i = day_log.Count - 1; i >= 0; i--)
            {
                //Debug.WriteLine("daylog {0}:{1}", day_log[i].Date.Day, day_log[i].TotalType);
                if (start - back_day < 1 || result.Count > count)
                {
                    break;
                }
                if (day_log[i].Date.Day == start - back_day)
                {
                    result.Add(day_log[i].TotalType);
                }
                else
                {
                    result.Add(0);
                }
                back_day++;
            }
             */
            return result.ToArray();
        }

        public static AllDayLog Load(string filename, DateTime date)
        {
            AllDayLog log = new AllDayLog();
            if (File.Exists(filename))
            {
                string xml = "";
                using (StreamReader sr = new StreamReader(filename))
                {
                    xml = sr.ReadToEnd();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                XmlNodeList day_list = doc.SelectNodes("//TotalLog");
                int log_num = 0;
                foreach (XmlNode log_node in day_list)
                {
                    XmlNode type_node = log_node.SelectSingleNode("TotalType");
                    XmlNode app_node = log_node.SelectSingleNode("TotalApp");
                    XmlNode date_node = log_node.SelectSingleNode("Date");
                    int total_type = int.Parse(type_node.InnerText);
                    int total_app = int.Parse(app_node.InnerText);
                    DateTime log_date = DateTime.Parse(date_node.InnerText);
                    log.day_log.Add(new TotalLog(total_type, total_app, log_date));
                    log.log_dic[log_date.ToString(Plugin.LogDir.DAY_FORMAT)] = log_num;
                    log_num++;
                }
                // 今日の日付がない場合（その日の初めての起動）はログ作成

                if (!log.log_dic.ContainsKey(date.ToString(Plugin.LogDir.DAY_FORMAT)))
                {
                    log.NewDay();
                }
            }
            else
            {
                log = new AllDayLog();
                log.NewDay();  // 今日の分のログ作成
            }
            return log;
        }

        public static AllDayLog Load(DateTime date)
        {
            return AllDayLog.Load(Plugin.LogDir.ALL_DAY_FILE, date);
        }

        /// <summary>
        /// ログを日付順に並び変える．日付辞書の順番も修正
        /// </summary>
        private void DateSort()
        {
            day_log.Sort(delegate(TotalLog a, TotalLog b)
            {
                return a.Date.CompareTo(b.Date);
            });
            log_dic.Clear();
            for (int i = 0; i < day_log.Count; i++)
            {
                string day = day_log[i].Date.ToString(Plugin.LogDir.DAY_FORMAT);
                log_dic[day] = i;
            }
        }

        public void Save(string filename)
        {
            DateSort();
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(Plugin.LogDir.ALL_DAY_FILE, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("AllDayLog");
                foreach (TotalLog log in day_log)
                {
                    writer.WriteStartElement("TotalLog");
                    writer.WriteElementString("TotalType", log.TotalType.ToString());
                    writer.WriteElementString("TotalApp", log.TotalApp.ToString());
                    writer.WriteElementString("Date", log.Date.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }
        }

        public void Save()
        {
            Save(Plugin.LogDir.ALL_DAY_FILE);
        }
    }
}
