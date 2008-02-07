using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TypingManager
{
    /// <summary>
    /// ���O��ۑ�����f�B���N�g��
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
    /// �\������O���t�̃^�C�v��\��
    /// </summary>
    public enum LineGraphType
    {
        StrokeNumPerMinute, // �Ō����^��
        StrokeNumPerHour,   // �Ō����^��
        StrokeNumPerDay,    // �Ō����^��
        TypeSpeedPerStroke, // �Ō����x�i�Ł^���j
    }

    public enum ProcessStrokeViewType
    {
        Today,
        All,
    }

    /// <summary>
    /// �A�v���P�[�V�����S�̂̐ݒ��ۑ�����N���X
    /// </summary>
    public class AppConfig
    {
        /// <summary>�u�v���Z�X�ʑŌ����v�^�u�ō����̑Ō���or���ׂĂ̑Ō����̐؂�ւ�</summary>
        public ProcessStrokeViewType processViewType;

        /// <summary>�O���t�ɂ���}�[�N</summary>
        public LineGraphMarkType markType;

        /// <summary>�O���t�̃^�C�v</summary>
        public LineGraphType lineGraphType;

        /// <summary>��Ɏ�O�ɕ\�����邩�ǂ���</summary>
        public bool topMost;

        /// <summary>�E�B���h�E�^�C�g���ʂ̑Ō�����ۑ����邩�ǂ���</summary>
        public bool saveTitleStroke;

        /// <summary>�I�����Ă����^�u�ԍ�</summary>
        public int tabIndex;

        private static AppConfig __instance;

        #region �v���p�e�B...
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
            // xmlSerializer�̓R���X�g���N�^���Ăяo���Ă���
            // XML�t�@�C���ɋL�q���Ă���l���Z�b�g����̂�
            // �t�@�C�����Ȃ��ꍇ�̏����l���R���X�g���N�^�ɏ����Ă�����
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
                //XmlSerializer�I�u�W�F�N�g�̍쐬
                XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));

                //�t�@�C�����J��
                FileStream fs = new FileStream(filename,
                    FileMode.Open, FileAccess.Read, FileShare.Read);

                XmlReader reader = XmlReader.Create(fs);

                //XML�t�@�C������ǂݍ��݁A�t�V���A��������
                __instance = (AppConfig)serializer.Deserialize(reader);

                //����
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
            //XmlSerializer�I�u�W�F�N�g���쐬
            //�������ރI�u�W�F�N�g�̌^���w�肷��
            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));

            //�t�@�C�����J��
            FileStream fs = new FileStream(filename, FileMode.Create);

            //�V���A�������AXML�t�@�C���ɕۑ�����
            serializer.Serialize(fs, __instance);
            
            //����
            fs.Close();
        }

        public static void Save()
        {
            Save(LogDir.CONFIG_FILE);
        }
    }
}
