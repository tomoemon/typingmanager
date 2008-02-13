using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using Plugin;

namespace TypingManager
{
    /// <summary>
    /// アプリケーション別の打鍵数のログを取る
    /// </summary>
    public class AppKeyLog : ICloneable
    {
        class MinuteLog
        {
            Dictionary<string, int> min_log = new Dictionary<string,int>();
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

    public class StrokeEventArgs : EventArgs
    {
        public string app_path;
        public string app_name;
        public int app_id;
        public bool today_new_app;  // 今日はじめてのプロセスを打つ場合
        public bool all_new_app;    // いままで一度も打ったことのないプロセスを打つ場合
        public bool new_day;
        public DateTime now;

        public StrokeEventArgs(DateTime _now)
        {
            app_path = StrokeProcessName.NO_TARGET;
            app_name = "";
            app_id = -1;
            today_new_app = false;
            all_new_app = false;
            new_day = false;
            now = _now;
        }

        public StrokeEventArgs(string path, string name, int id, bool b_newapp, bool b_newday) : base()
        {
            app_path = path;
            app_name = name;
            app_id = id;
            today_new_app = b_newapp;
            new_day = b_newday;
        }
    }
    public delegate void StrokeEventHandler(object sender, StrokeEventArgs args);

    public class StrokeNumLog : Plugin.BaseStrokePlugin, Plugin.IStrokeNumData, ITimerTask
    {
        public const int TIMER_ID_SAVE = 0;
        public const int TIMER_ID_NEWDAY = 1;

        private StrokeProcessName processName;

        // <AppID, 該当プロセスの1分ごと，タイトル別のログ>
        private Dictionary<int, AppKeyLog> today_app_log;
        private Dictionary<int, AppKeyLog> yesterday_app_log;

        // 1日ごとの打鍵数とプロセス数，最後にあるのが今日の記録
        private AllDayLog allday_log;
        
        public event StrokeEventHandler strokeEvent;
        
        #region プロパティ...
        public int TotalType
        {
            get { return allday_log.TotalType; }
        }
        public int TotalApp
        {
            get { return processName.TotalApp; }
        }
        public int TotalDay
        {
            get { return allday_log.TotalDay; }
        }
        public int TodayTotalType
        {
            get { return allday_log.TodayTotalType; }
        }
        public int TodayTotalApp
        {
            get { return allday_log.TodayTotalApp; }
        }
        public int YesterdayTotalType
        {
            get { return allday_log.YesterdayTotalType; }
        }
        public int YesterdayTotalApp
        {
            get { return allday_log.YesterdayTotalApp; }
        }
        /// <summary>
        /// AppIDに関連付けられたAppLog
        /// </summary>
        /// <param name="index">AppID</param>
        /// <returns></returns>
        public AppKeyLog this[int index]
        {
            get { return today_app_log[index]; }
        }
        public StrokeProcessName ProcessName
        {
            get { return processName; }
        }
        public AllDayLog AllDay
        {
            get { return allday_log; }
        }
        /// <summary>
        /// AppIDに関連付けられたAppLog
        /// </summary>
        public Dictionary<int, AppKeyLog> AppLog
        {
            get { return today_app_log; }
        }
        #endregion

        /// <summary>
        /// 現在の日付のログを取得したい場合に利用する
        /// </summary>
        public StrokeNumLog()
        {
            base.Valid = true;
            processName = new StrokeProcessName();
            today_app_log = new Dictionary<int, AppKeyLog>();
            today_app_log[0] = new AppKeyLog(0); // "null" の分
            yesterday_app_log = new Dictionary<int, AppKeyLog>();
            yesterday_app_log[0] = new AppKeyLog(0); // "null" の分
            Load();
        }

        /// <summary>
        /// 特定の日付のログをすべて取得したい場合に利用する
        /// </summary>
        /// <param name="date"></param>
        public StrokeNumLog(DateTime date)
        {
            base.Valid = true;
            processName = new StrokeProcessName();
            today_app_log = new Dictionary<int, AppKeyLog>();
            today_app_log[0] = new AppKeyLog(0); // "null" の分
            yesterday_app_log = new Dictionary<int, AppKeyLog>();
            yesterday_app_log[0] = new AppKeyLog(0); // "null" の分
            Load(date);
        }

        #region BaseStrokePluginの実装上書き
        /// <summary>プラグインの名前を返すこと</summary>
        public override string GetPluginName() { return "打鍵数記録"; }

        /// <summary>プラグインにアクセスするための名前を返すこと</summary>
        public override string GetAccessName() { return "stroke_num_log"; }

        /// <summary>プラグインに関する簡単な説明を書くこと</summary>
        public override string GetComment() { return "プロセス別・タイトル別の打鍵数を記録します"; }

        /// <summary>プラグイン作者の名前を返すこと</summary>
        public override string GetAuthorName() { return "tomoemon"; }

        /// <summary>プラグインのバージョンを書くこと</summary>
        public override string GetVersion() { return "0.0.1"; }

        public override object GetInfo()
        {
            return (Plugin.IStrokeNumData)this;
        }
        public override void Close()
        {
            Save();
        }
        #endregion

        #region ITimerTaskの実装
        /// <summary>
        /// 1秒ごとにTimerTaskControllerから呼ばれるTask
        /// </summary>
        /// <param name="date"></param>
        public void TimerTask(DateTime date, int id)
        {
            if (id == TIMER_ID_SAVE)
            {
                if (!IsNewDay(date))
                {
                    Save();
                }
            }
            else if (id == TIMER_ID_NEWDAY)
            {
                if (IsNewDay(date))
                {
                    Save(allday_log.GetLastDate());
                    NewDay(date);
                }
            }
        }
        #endregion

        public bool IsNewDay(DateTime date)
        {
            return allday_log.IsNewDay(date);
        }

        /// <summary>
        ///  これまでのログにない日付になった時の処理
        /// </summary>
        public void NewDay(DateTime now)
        {
            allday_log.NewDay();
            yesterday_app_log.Clear();
            foreach (int id in today_app_log.Keys)
            {
                yesterday_app_log[id] = (AppKeyLog)today_app_log[id].Clone();
            }
            today_app_log.Clear();
            today_app_log[0] = new AppKeyLog(0); // "null" の分

            // リストビューの更新
            StrokeEventArgs args = new StrokeEventArgs(now);
            args.new_day = true;
            strokeEvent(this, args);
        }

        /// <summary>
        /// 打鍵数のカウント．キーを上げたときに呼び出される
        /// </summary>
        /// <param name="app_path"></param>
        /// <param name="win_title"></param>
        public override void KeyUp(IKeyState keycode, uint militime, string app_path, string app_title)
        {
            if (app_path == "")
            {
                app_path = StrokeProcessName.NO_TARGET;
            }
            if (!AppConfig.SaveTitleStroke)
            {
                app_title = "";
            }
            
            DateTime now = DateTime.Now;
            StrokeEventArgs args = new StrokeEventArgs(now);
            
            // これまで打ったプロセスリストになければ追加（完全に新しい場合）
            int app_id = processName.GetID(app_path);
            if (app_id == -1)
            {
                string new_name = Path.GetFileNameWithoutExtension(app_path);
                app_id = processName.Add(app_path, new_name);
                args.all_new_app = true;
            }
            args.app_id = app_id;
            args.app_path = args.app_name = processName.GetPath(app_id);
            processName.Stroke(app_path);

            if (IsNewDay(now))
            {
                // 日付が変わった時の処理
                // 基本的には日が変わった瞬間にタイマーからNewDayが呼ばれるはずだが
                // タイマーは1時間ごとのチェックなので，時計を手動で変えられた場合は
                // 認識することができないので打鍵したタイミングでもチェックを行う．
                Save(allday_log.GetLastDate());
                NewDay(now);
                args.new_day = true;
            }
            allday_log.TodayTotalType++;

            // 今日の打鍵対象プロセスの中にあれば打鍵．なければ追加．
            // (app_id == 0)のときは対象プロセス名が見つからない場合で
            // app_path == "null"となっており，"null"の打鍵数を増やす実装となっている
            if (today_app_log.ContainsKey(app_id))
            {
                today_app_log[app_id].Stroke(app_title, now.Hour, now.Minute);
            }
            else
            {
                today_app_log[app_id] = new AppKeyLog(app_id);
                today_app_log[app_id].Stroke(app_title, now.Hour, now.Minute);
                allday_log.TodayTotalApp++;
                args.today_new_app = true;
                args.app_name = processName.GetName(app_id);
            }
            strokeEvent(this, args);
        }

        /// <summary>
        /// 今日の分の1分ごとの打鍵数をstartからcount個分前までの配列で返す
        /// </summary>
        /// <param name="start">欲しい時間（時間×分）</param>
        /// <param name="count">欲しい個数（分単位）</param>
        /// <returns>start時刻が0番目で過去の記録が後ろに格納されている配列</returns>
        public int[] GetMinuteStroke(int start, int count)
        {
            //Debug.WriteLine("start: {0}, count: {1}", start, count);
            // 「今日の分」だけを返すので昨日に割り込む場合はcountを少なくする
            // startが1だったときは0時1分の分と0時0分の分を返せるのでcountは2までOK
            int today_count = count;
            int yesterday_count = 0;
            if (start - count + 1 < 0)
            {
                today_count = start + 1;
                yesterday_count = count - today_count;
            }
            int[] result = new int[count];
            for (int i = 0; i < today_count; i++)
            {
                foreach (AppKeyLog log in today_app_log.Values)
                {
                    result[i] += log[start - i];
                }
                //Debug.Write("[i={0},{1}] ", i, result[i]);
            }
            int day_minute = 23 * 60 + 59;
            for (int i = 0; i < yesterday_count; i++)
            {
                foreach (AppKeyLog log in yesterday_app_log.Values)
                {
                    result[today_count + i] += log[day_minute - i];
                }
            }
            /*
            for (int i = 0; i < count; i++)
            {
                Console.Write("{0},", result[i]);
            }
            Console.WriteLine("");
            */
            return result;
        }

        public int[] GetMinuteStroke(int count)
        {
            DateTime now = DateTime.Now;
            int start = now.Hour * 60 + now.Minute;
            return GetMinuteStroke(start, count);
        }

        /// <summary>
        /// 1時間ごとの打鍵数をstartからcount個分前までの配列で返す
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int[] GetHourStroke(int start, int count)
        {
            int today_count = count;
            int yesterday_count = 0;
            if (start - count + 1 < 0)
            {
                today_count = start + 1;
                yesterday_count = count - today_count;
            }
            
            int[] result = new int[count];
            for (int i = 0; i < today_count; i++)
            {
                foreach (AppKeyLog log in today_app_log.Values)
                {
                    result[i] += log.GetHourTotal(start - i);
                }
            }
            int day_hour = 23;
            for (int i = 0; i < yesterday_count; i++)
            {
                foreach (AppKeyLog log in yesterday_app_log.Values)
                {
                    result[today_count+i] += log.GetHourTotal(day_hour - i);
                }
            }
            /*
            for (int i = 0; i < count; i++)
            {
                Console.Write("{0},", result[i]);
            }
            Console.WriteLine("");
            */ 
            return result;
        }

        public int[] GetHourStroke(int count)
        {
            int start = DateTime.Now.Hour;
            return GetHourStroke(start, count);
        }

        /// <summary>
        /// 1日ごとの打鍵数をcountで指定した日数分配列で返す
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int[] GetDayStroke(DateTime start, int count)
        {
            return allday_log.GetDayStroke(start, count);
        }

        public int[] GetDayStroke(int count)
        {
            return GetDayStroke(DateTime.Now, count);
        }

        /// <summary>
        /// 前回終了時のログを読み込んで状態を復帰する
        /// ・新しいアプリケーション用の次のID
        /// 
        /// 日別ログ
        /// ・その日のアプリケーションごとのログ（分刻み）
        /// 
        /// </summary>
        public void Load()
        {
            DateTime today = DateTime.Now;
            processName.Load();
            allday_log = AllDayLog.Load(today);
            Load(today);

            Console.WriteLine("today hour log");
            for (int i = 0; i < 24; i++)
            {
                int total = 0;
                foreach (AppKeyLog log in today_app_log.Values)
                {
                    total += log.GetHourTotal(i);
                }
                Console.Write("{0},", total);
            }
            Console.WriteLine("\nyesterday hour log");
            for (int i = 0; i < 24; i++)
            {
                int total = 0;
                foreach (AppKeyLog log in yesterday_app_log.Values)
                {
                    total += log.GetHourTotal(i);
                }
                Console.Write("{0},", total);
            }
            Console.WriteLine("");

        }

        public void Load(DateTime date)
        {
            DateTime lastday = date.AddDays(-1);
            processName.Load();
            allday_log = AllDayLog.Load(date);
            LoadDayLog(date, today_app_log);
            LoadDayLog(lastday, yesterday_app_log);
        }

        public void Save()
        {
            processName.Save();
            allday_log.Save();
            TotalSave();
            SaveDayLog(DateTime.Now);
        }

        public void Save(DateTime date)
        {
            processName.Save();
            allday_log.Save();
            TotalSave();
            SaveDayLog(date);
        }

        private void SaveDayLog(DateTime date)
        {
            string filename = LogDir.DAY_LOG_FILE(date);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(filename, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DayLog");
                writer.WriteElementString("Date", date.ToString(Plugin.LogDir.DAY_FORMAT));
                writer.WriteElementString("TotalType", allday_log.GetDayTotalType(date).ToString());
                writer.WriteElementString("TotalApp", allday_log.GetDayTotalApp(date).ToString());

                writer.WriteStartElement("SpanList");
                
                for (int i = 0; i <= 23; i++)
                {
                    for (int j = 0; j <= 59; j++)
                    {
                        int minute_total = 0;
                        foreach (AppKeyLog log in today_app_log.Values)
                        {
                            minute_total += log.GetMinuteTotal(i, j);
                        }
                        // どのプロセスも一打鍵もしていなければその時間のログは残さない
                        if (minute_total == 0) continue;

                        writer.WriteStartElement("Span");
                        writer.WriteAttributeString("hour", "", i.ToString());
                        writer.WriteAttributeString("minute", "", j.ToString());
                        foreach (AppKeyLog log in today_app_log.Values)
                        {
                            if (log[60 * i + j] != 0)
                            {
                                writer.WriteStartElement("Count");
                                writer.WriteAttributeString("app_id", log.AppID.ToString());
                                writer.WriteAttributeString("num", log[60 * i + j].ToString());
                                foreach (string title in log.GetTitleList(i, j))
                                {
                                    int title_total = log.GetTitleStroke(title, i, j);
                                    writer.WriteStartElement("Title");
                                    writer.WriteAttributeString("num", title_total.ToString());
                                    writer.WriteValue(title);
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();
                    }
                }
                
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }
        }

        private void LoadDayLog(DateTime date, Dictionary<int, AppKeyLog> log)
        {
            string filename = LogDir.DAY_LOG_FILE(date);

            if (File.Exists(filename))
            {
                string xml = "";
                using (StreamReader sr = new StreamReader(filename))
                {
                    xml = sr.ReadToEnd();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                XmlNodeList node_list = doc.SelectNodes("//Count");
                foreach (XmlNode count_node in node_list)
                {
                    XmlNode span_node = count_node.ParentNode;
                    XmlAttributeCollection span_attrs = span_node.Attributes;
                    int hour = int.Parse(span_attrs["hour"].Value);
                    int min = int.Parse(span_attrs["minute"].Value);

                    XmlAttributeCollection count_attrs = count_node.Attributes;
                    int app_id = int.Parse(count_attrs["app_id"].Value);
                    int num = int.Parse(count_attrs["num"].Value);
                    if (!log.ContainsKey(app_id))
                    {
                        log[app_id] = new AppKeyLog(app_id);
                    }
                    
                    log[app_id].Total += num;
                    log[app_id].SetMinuteTotal(hour, min, num);

                    XmlNodeList title_list = count_node.SelectNodes("Title");
                    foreach (XmlNode title_node in title_list)
                    {
                        XmlAttributeCollection title_attrs = title_node.Attributes;
                        int title_num = int.Parse(title_attrs["num"].Value);
                        string title_name = title_node.InnerText;
                        //Console.WriteLine("title:{0}, num:{1}", title_name,title_num);
                        log[app_id].SetMinuteTitle(title_name, hour, min, title_num);
                    }
                }
            }
        }

        /// <summary>
        /// 全体の合計を書き出し（書き出すだけで読み込みには使わない）
        /// </summary>
        public void TotalSave()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(LogDir.TOTAL_FILE, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("TotalLog");
                writer.WriteElementString("LastUpdate", DateTime.Now.ToString());
                writer.WriteElementString("TotalType", TotalType.ToString());
                writer.WriteElementString("TotalApp",  TotalApp.ToString());
                writer.WriteElementString("TotalDay", TotalDay.ToString());
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Close();
            }
        }
    }
}
