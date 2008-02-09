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
        public const string CONFIG_DIR = "config";
        
        public static string TOTAL_FILE
        { get { return Path.Combine(LOG_DIR, "total.xml"); } }
        public static string ALL_DAY_FILE
        { get { return Path.Combine(LOG_DIR, "allday.xml"); } }
        public static string PROCESS_FILE
        { get { return Path.Combine(LOG_DIR, "process.xml"); } }
        public static string CONFIG_FILE
        { get { return Path.Combine(CONFIG_DIR, "config.xml"); } }
        public static string PLUGIN_CONFIG_FILE
        { get { return Path.Combine(CONFIG_DIR, "plugin_config.xml"); } }
        public static string DAY_LOG_DIR
        { get { return Path.Combine(LOG_DIR, "day_log"); } }
        public static string DAY_LOG_FILE(DateTime date)
        { return Path.Combine(DAY_LOG_DIR, date.ToString(DAY_FORMAT) + ".xml"); }
        
        public static void LogDirectoryCheck()
        {
            if (!Directory.Exists(DAY_LOG_DIR))
            {
                Directory.CreateDirectory(DAY_LOG_DIR);
            }
            if (!Directory.Exists(PLUGIN_DIR))
            {
                Directory.CreateDirectory(PLUGIN_DIR);
            }
            if (!Directory.Exists(CONFIG_DIR))
            {
                Directory.CreateDirectory(CONFIG_DIR);
            }
        }
    }
}
