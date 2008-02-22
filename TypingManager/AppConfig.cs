using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TypingManager
{
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

        /// <summary>定期的にログを保存するか</summary>
        public bool scheduleLogging;

        /// <summary>ログを保存する間隔（分）</summary>
        public int scheduleTiming;

        /// <summary>打鍵速度履歴グラフで打鍵が続いていると認識して描画する時間（秒）</summary>
        public int noStrokeLimitTime;

        /// <summary>終了時に確認メッセージを出すか</summary>
        public bool showExitMessage;

        /// <summary>「選択した項目をコピー」する形式</summary>
        public string selectedItemCopyFormat;

        /// <summary>打鍵数リストビューを右クリックしたときのコピーする形式</summary>
        public string rightClickCopyFormat;

        /// <summary>低水準フックを使うか（falseの場合は_proxy.exeを使うフック）</summary>
        public bool hookLowLevel;

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
        public static bool ScheduleLogging
        {
            get { return __instance.scheduleLogging; }
            set { __instance.scheduleLogging = value; }
        }
        public static int ScheduleTiming
        {
            get { return __instance.scheduleTiming; }
            set { __instance.scheduleTiming = value; }
        }
        public static int NoStrokeLimitTime
        {
            get { return __instance.noStrokeLimitTime; }
            set { __instance.noStrokeLimitTime = value; }
        }
        public static bool ShowExitMessage
        {
            get { return __instance.showExitMessage; }
            set { __instance.showExitMessage = value; }
        }
        public static string SelectedItemCopyFormat
        {
            get { return __instance.selectedItemCopyFormat; }
            set { __instance.selectedItemCopyFormat = value; }
        }
        public static string RightClickCopyFormat
        {
            get { return __instance.rightClickCopyFormat; }
            set { __instance.rightClickCopyFormat = value; }
        }
        public static bool HookLowLevel
        {
            get { return __instance.hookLowLevel; }
            set { __instance.hookLowLevel = value; }
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
            scheduleLogging = true;
            scheduleTiming = 10;
            noStrokeLimitTime = 10;
            showExitMessage = false;
            selectedItemCopyFormat = "%1, %2, %3";
            rightClickCopyFormat = "%1 - [%2]";
            hookLowLevel = true;
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
            AppConfig.Load(Plugin.LogDir.CONFIG_FILE);
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
            Save(Plugin.LogDir.CONFIG_FILE);
        }
    }
}
