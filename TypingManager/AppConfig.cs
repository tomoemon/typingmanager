using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TypingManager
{
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

        /// <summary>����I�Ƀ��O��ۑ����邩</summary>
        public bool scheduleLogging;

        /// <summary>���O��ۑ�����Ԋu�i���j</summary>
        public int scheduleTiming;

        /// <summary>�Ō����x�����O���t�őŌ��������Ă���ƔF�����ĕ`�悷�鎞�ԁi�b�j</summary>
        public int noStrokeLimitTime;

        /// <summary>�I�����Ɋm�F���b�Z�[�W���o����</summary>
        public bool showExitMessage;

        /// <summary>�u�I���������ڂ��R�s�[�v����`��</summary>
        public string selectedItemCopyFormat;

        /// <summary>�Ō������X�g�r���[���E�N���b�N�����Ƃ��̃R�s�[����`��</summary>
        public string rightClickCopyFormat;

        /// <summary>�ᐅ���t�b�N���g�����ifalse�̏ꍇ��_proxy.exe���g���t�b�N�j</summary>
        public bool hookLowLevel;

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
            // xmlSerializer�̓R���X�g���N�^���Ăяo���Ă���
            // XML�t�@�C���ɋL�q���Ă���l���Z�b�g����̂�
            // �t�@�C�����Ȃ��ꍇ�̏����l���R���X�g���N�^�ɏ����Ă�����
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
            AppConfig.Load(Plugin.LogDir.CONFIG_FILE);
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
            Save(Plugin.LogDir.CONFIG_FILE);
        }
    }
}
