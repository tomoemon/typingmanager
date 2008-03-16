using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Plugin;

namespace TypingManager
{
    public class TimeLog
    {
        private int today_time = 0;
        private int[] hour_time = new int[24];

        #region プロパティ...
        public int TodayTime
        {
            get { return today_time; }
            set { today_time = value; }
        }
        #endregion

        public void AddTime(int hour)
        {
            today_time++;
            hour_time[hour]++;
        }

        public string GetTodayFormatTime()
        {
            int hour = today_time / 3600;
            int minute = today_time % 3600 / 60;
            int sec = today_time % 60;
            string format = "";
            if (hour > 0)
            {
                format = string.Format("{0}時間{1:00}分{2:00}秒", hour, minute, sec);
            }
            else
            {
                format = string.Format("{0}分{1:00}秒", minute, sec);
            }
            return format;
        }

        public string GetHourFormatTime(int hour)
        {
            int minute = hour_time[hour] / 60;
            int sec = hour_time[hour] % 60;
            string format = "";
            if (minute > 0)
            {
                format = string.Format("{0}分{1:00}秒", minute, sec);
            }
            else
            {
                format = string.Format("{0}秒", sec);
            }
            return format;
        }

        public int GetHourTime(int hour)
        {
            if (0 <= hour && hour < 24)
            {
                return hour_time[hour];
            }
            return 0;
        }

        public void SetHourTime(int hour, int time)
        {
            if (0 <= hour && hour < 24)
            {
                hour_time[hour] = time;
            }
        }
    }

    public class StrokeTimeLog : ITimerTask, IStrokePlugin
    {
        public const int TIMER_ID_COUNT = 0;
        private bool valid = true;
        public string log_dir = "";
        private IPluginController controller;
        private Form main_form;
        private TypingSpeed speed;
        private TimeLog normal_log;
        private TimeLog specific_log;
        private DateTime last_date;
        private int[] max_thresh = new int[24];
        private int[] min_thresh = new int[24];

        public string GetNormalToday()
        {
            return normal_log.GetTodayFormatTime();
        }
        public string GetNormalHour(int hour)
        {
            return normal_log.GetHourFormatTime(hour);
        }
        public string GetSpecificToday()
        {
            return specific_log.GetTodayFormatTime();
        }
        public string GetSpecificHour(int hour)
        {
            return specific_log.GetHourFormatTime(hour);
        }

        public StrokeTimeLog(TypingSpeed _speed)
        {
            speed = _speed;
            last_date = DateTime.Now;
        }

        public void AutoSave()
        {
            Save();
        }

        public void Save()
        {
            SaveDayLog(last_date);
        }

        public void Load()
        {
            LoadDayLog(DateTime.Now);
        }

        private string GetLogFileName(DateTime date)
        {
            string filename = date.ToString(LogDir.DAY_FORMAT) + ".xml";
            string path = Path.Combine(log_dir, filename);
            return path;
        }

        private void SaveDayLog(DateTime date)
        {
            string filename = GetLogFileName(date);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            XmlWriter writer = XmlWriter.Create(filename, settings);
            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("TimeLog");
                writer.WriteElementString("Date", date.ToString(Plugin.LogDir.DAY_FORMAT));
                writer.WriteElementString("NomalToday", normal_log.TodayTime.ToString());
                writer.WriteElementString("SpecificToday", specific_log.TodayTime.ToString());

                writer.WriteStartElement("StrokeTimeList");

                for (int i = 0; i <= 23; i++)
                {
                    writer.WriteStartElement("Hour");
                    writer.WriteAttributeString("hour", "", i.ToString());
                    writer.WriteAttributeString("min_thresh", "", min_thresh[i].ToString());
                    writer.WriteAttributeString("max_thresh", "", max_thresh[i].ToString());
                    writer.WriteStartElement("Time");
                    writer.WriteAttributeString("normal", "", normal_log.GetHourTime(i).ToString());
                    writer.WriteAttributeString("specific", "", specific_log.GetHourTime(i).ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement();
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

        private void LoadDayLog(DateTime date)
        {
            string filename = GetLogFileName(date);

            for (int i = 0; i < 24; i++)
            {
                min_thresh[i] = 0;
                max_thresh[i] = 0;
            }
            normal_log = new TimeLog();
            specific_log = new TimeLog();

            if (File.Exists(filename))
            {
                string xml = "";
                using (StreamReader sr = new StreamReader(filename))
                {
                    xml = sr.ReadToEnd();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                int normal_total = 0;
                int specific_total = 0;
                XmlNodeList node_list = doc.SelectNodes("//Hour");
                foreach (XmlNode hour_node in node_list)
                {
                    XmlAttributeCollection hour_attrs = hour_node.Attributes;
                    int hour = int.Parse(hour_attrs["hour"].Value);
                    min_thresh[hour] = int.Parse(hour_attrs["min_thresh"].Value);
                    max_thresh[hour] = int.Parse(hour_attrs["max_thresh"].Value);
                    XmlAttributeCollection time_attrs = hour_node.SelectSingleNode("Time").Attributes;
                    int normal = int.Parse(time_attrs["normal"].Value);
                    int specific = int.Parse(time_attrs["specific"].Value);
                    normal_total += normal;
                    specific_total += specific;
                    normal_log.SetHourTime(hour, normal);
                    specific_log.SetHourTime(hour, specific);
                }
                normal_log.TodayTime = normal_total;
                specific_log.TodayTime = specific_total;
            }
        }

        #region ITimerTask メンバ
        public void TimerTask(DateTime date, int id)
        {
            if (!valid) return;

            if (last_date.Day != date.Day)
            {
                SaveDayLog(last_date);
                Load();
                last_date = date;
            }
            int stroke_speed = (int)speed.GetSpeed();

            if (stroke_speed > 0)
            {
                normal_log.AddTime(date.Hour);
            }

            min_thresh[date.Hour] = AppConfig.MinStrokeTimeSpeed;
            max_thresh[date.Hour] = AppConfig.MaxStrokeTimeSpeed;

            int min = AppConfig.MinStrokeTimeSpeed;
            int max = AppConfig.MaxStrokeTimeSpeed;
            if (min < stroke_speed && stroke_speed < max)
            {
                specific_log.AddTime(date.Hour);
            }
        }
        #endregion

        #region IPluginBase メンバ
        public void Close()
        {
            Save();
        }

        public IPluginController Controller
        {
            get { return controller; }
            set { controller = value; }
        }

        public string GetAccessName()
        {
            return "stroke_time_log";
        }

        public string GetAuthorName()
        {
            return "tomoemon";
        }

        public string GetComment()
        {
            return "打鍵していた時間を計測します";
        }

        public object GetInfo()
        {
            return null;
        }

        public string GetPluginName()
        {
            return "打鍵時間計測ログ";
        }

        public List<System.Windows.Forms.ToolStripMenuItem> GetToolStripMenu()
        {
            return null;
        }

        public string GetVersion()
        {
            return "0.0.1";
        }

        public void Init()
        {
            log_dir = controller.GetSaveDir(this.GetAccessName());
            Load();
        }

        public bool IsHasConfigForm()
        {
            return false;
        }

        public void KeyDown(IKeyState keystate, uint militime, string app_path, string app_title)
        {
        }

        public void KeyUp(IKeyState keystate, uint militime, string app_path, string app_title)
        {
        }

        public System.Windows.Forms.Form MainForm
        {
            get { return main_form; }
            set { main_form = value; }
        }

        public void ShowConfigForm()
        {
        }

        public bool Valid
        {
            get { return valid; }
            set { valid = value; }
        }
        #endregion
    }

    public class StrokeTimeView : ITimerTask
    {
        public const int TIMER_ID_UPDATE = 0;
        private StrokeTimeLog log;
        private TextBox normal_today;
        private TextBox normal_hour;
        private TextBox specific_today;
        private TextBox specific_hour;

        public StrokeTimeView(StrokeTimeLog _log, TextBox _today, TextBox _hour,
            TextBox _specific_today, TextBox _specific_hour)
        {
            log = _log;
            normal_today = _today;
            normal_hour = _hour;
            specific_today = _specific_today;
            specific_hour = _specific_hour;
        }

        #region ITimerTask メンバ
        public void TimerTask(DateTime date, int id)
        {
            normal_today.Text = log.GetNormalToday();
            normal_hour.Text = log.GetNormalHour(date.Hour);
            specific_today.Text = log.GetSpecificToday();
            specific_hour.Text = log.GetSpecificHour(date.Hour);
        }
        #endregion
    }
}
