using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TypingManager
{
    /// <summary>
    /// ログを保存するディレクトリ
    /// </summary>
    public class LogDir
    {
        public const string LOG_DIR = "log";
        public const string PLUGIN_DIR = "plugins";
        public const string CONFIG_FILE = "config.xml";

        public static string COMMENT_FILE
            { get { return Path.Combine(LOG_DIR, "comment.txt"); }}
        public static string TOTAL_FILE
            { get { return Path.Combine(LOG_DIR, "total.xml"); }}
        public static string ALL_DAY_FILE
            { get { return Path.Combine(LOG_DIR, "allday.xml"); }}
        public static string PROCESS_FILE
            { get { return Path.Combine(LOG_DIR, "process.xml"); } }
        public static string DAY_LOG_DIR
            { get { return Path.Combine(LOG_DIR, "day_log"); }}
        public static string DETAIL_LOG_DIR
            { get { return Path.Combine(LOG_DIR, "detail_log"); }}
        public static string DETAIL_CSV_DIR
            { get { return Path.Combine(DETAIL_LOG_DIR, "csv"); }}
        public static string DETAIL_XML_DIR
            { get { return DETAIL_LOG_DIR; }}
        public static string DAY_LOG_FILE(DateTime date)
            { return Path.Combine(DAY_LOG_DIR, date.ToString(AllDayLog.DAY_FORMAT) + ".xml"); }
        public static string DETAIL_XML_FILE(DateTime date)
            { return Path.Combine(DETAIL_XML_DIR, date.ToString("yyyyMMdd_HHmmss") + ".xml");}

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
        }
    }

    /// <summary>
    /// 表示するグラフのタイプを表す
    /// </summary>
    public enum LineGraphType
    {
        StrokeNumPerMinute, // 打鍵数／分
        StrokeNumPerHour,   // 打鍵数／時
        StrokeNumPerDay,    // 打鍵数／日
        TypeSpeedPerStroke, // 打鍵速度（打／分）
    }

    public enum ProcessStrokeViewType
    {
        Today,
        All,
    }

    /// <summary>
    /// アプリケーション全体の設定を保存するクラス
    /// </summary>
    public class AppConfig
    {
        /// <summary>「プロセス別打鍵数」タブで今日の打鍵数orすべての打鍵数の切り替え</summary>
        public ProcessStrokeViewType processViewType;

        /// <summary>グラフにつけるマーク</summary>
        public LineGraphMarkType markType;

        /// <summary>グラフのタイプ</summary>
        public LineGraphType lineGraphType;

        /// <summary>常に手前に表示するかどうか</summary>
        public bool topMost;

        /// <summary>ウィンドウタイトル別の打鍵数を保存するかどうか</summary>
        public bool saveTitleStroke;

        /// <summary>選択していたタブ番号</summary>
        public int tabIndex;

        private static AppConfig __instance;

        #region プロパティ...
        public static ProcessStrokeViewType ProcessViewType
        {
            get { return __instance.processViewType; }
            set { __instance.processViewType = value; }
        }
        public static LineGraphMarkType MarkType
        {
            get { return __instance.markType; }
            set { __instance.markType = value; }
        }
        public static LineGraphType LineGraphType
        {
            get { return __instance.lineGraphType; }
            set { __instance.lineGraphType = value; }
        }
        public static bool TopMost
        {
            get { return __instance.topMost; }
            set { __instance.topMost = value; }
        }
        public static bool SaveTitleStroke
        {
            get { return __instance.saveTitleStroke; }
            set { __instance.saveTitleStroke = value; }
        }
        public static int TabIndex
        {
            get { return __instance.tabIndex; }
            set { __instance.tabIndex = value; }
        }
        #endregion

        private AppConfig()
        {
            // xmlSerializerはコンストラクタを呼び出してから
            // XMLファイルに記述してある値をセットするので
            // ファイルがない場合の初期値をコンストラクタに書いておける
            lineGraphType = LineGraphType.TypeSpeedPerStroke;
            topMost = false;
            processViewType = ProcessStrokeViewType.Today;
            markType = LineGraphMarkType.None;
            saveTitleStroke = false;
            tabIndex = 0;
        }

        public static void Load(string filename)
        {
            if (File.Exists(filename))
            {
                //XmlSerializerオブジェクトの作成
                XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));

                //ファイルを開く
                FileStream fs = new FileStream(filename,
                    FileMode.Open, FileAccess.Read, FileShare.Read);

                XmlReader reader = XmlReader.Create(fs);

                //XMLファイルから読み込み、逆シリアル化する
                __instance = (AppConfig)serializer.Deserialize(reader);

                //閉じる
                fs.Close();
            }
            else
            {
                __instance = new AppConfig();
            }
        }

        public static void Load()
        {
            AppConfig.Load(LogDir.CONFIG_FILE);
        }

        public static void Save(string filename)
        {
            //XmlSerializerオブジェクトを作成
            //書き込むオブジェクトの型を指定する
            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));

            //ファイルを開く
            FileStream fs = new FileStream(filename, FileMode.Create);

            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(fs, __instance);
            
            //閉じる
            fs.Close();
        }

        public static void Save()
        {
            Save(LogDir.CONFIG_FILE);
        }
    }
}
