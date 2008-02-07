using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Plugin
{
    /// <summary>
    /// ログを保存するディレクトリ
    /// </summary>
    public class LogDir
    {
        public const string DAY_FORMAT = "yyyyMMdd";
        public const string LOG_DIR = "log";
        public const string PLUGIN_DIR = "plugins";
        public const string CONFIG_FILE = "config.xml";

        public static string COMMENT_FILE
        { get { return Path.Combine(LOG_DIR, "comment.txt"); } }
        public static string TOTAL_FILE
        { get { return Path.Combine(LOG_DIR, "total.xml"); } }
        public static string ALL_DAY_FILE
        { get { return Path.Combine(LOG_DIR, "allday.xml"); } }
        public static string PROCESS_FILE
        { get { return Path.Combine(LOG_DIR, "process.xml"); } }
        public static string DAY_LOG_DIR
        { get { return Path.Combine(LOG_DIR, "day_log"); } }
        public static string DETAIL_LOG_DIR
        { get { return Path.Combine(LOG_DIR, "detail_log"); } }
        public static string DETAIL_CSV_DIR
        { get { return Path.Combine(DETAIL_LOG_DIR, "csv"); } }
        public static string DETAIL_XML_DIR
        { get { return DETAIL_LOG_DIR; } }
        public static string DAY_LOG_FILE(DateTime date)
        { return Path.Combine(DAY_LOG_DIR, date.ToString(DAY_FORMAT) + ".xml"); }
        public static string DETAIL_XML_FILE(DateTime date)
        { return Path.Combine(DETAIL_XML_DIR, date.ToString("yyyyMMdd_HHmmss") + ".xml"); }

        public static void LogDirectoryCheck()
        {
            if (!Directory.Exists(DAY_LOG_DIR))
            {
                Directory.CreateDirectory(DAY_LOG_DIR);
            }
            if (!Directory.Exists(DETAIL_XML_DIR))
            {
                Directory.CreateDirectory(DETAIL_XML_DIR);
            }
            if (!Directory.Exists(DETAIL_CSV_DIR))
            {
                Directory.CreateDirectory(DETAIL_CSV_DIR);
            }
            if (!Directory.Exists(PLUGIN_DIR))
            {
                Directory.CreateDirectory(PLUGIN_DIR);
            }
        }
    }
}
