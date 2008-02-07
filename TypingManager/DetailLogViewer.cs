using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.IO;

namespace TypingManager
{
    public class DetailLogViewer
    {
        // <タグ,タグが登録してある詳細ログ情報>
        Dictionary<string, List<DetailLogInfo>> tag_log;

        // <日付,日付に一致する詳細ログ情報>
        Dictionary<string, List<DetailLogInfo>> date_log;

        public const string DATE_FORMAT = "yyyy年MM月dd日";
        public const string TIME_FORMAT = "HH:mm";

        #region プロパティ...
        public List<string> TagList
        {
            get { return new List<string>(tag_log.Keys); }
        }
        public List<string> DateList
        {
            get { return new List<string>(date_log.Keys); }
        }
        #endregion

        public DetailLogViewer()
        {
            tag_log = new Dictionary<string, List<DetailLogInfo>>();
            date_log = new Dictionary<string, List<DetailLogInfo>>();
        }

        /// <summary>
        /// 指定された日付に作成された詳細ログの数を返す
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public int GetDateSetNum(string date)
        {
            return date_log[date].Count;
        }

        /// <summary>
        /// 指定されたタグを付けられた詳細ログの数を返す
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public int GetTagSetNum(string tag)
        {
            return tag_log[tag].Count;
        }

        public List<DetailLogInfo> GetDateInfo(string date)
        {
            return date_log[date];
        }

        public List<DetailLogInfo> GetTagInfo(string tag)
        {
            return tag_log[tag];
        }

        /// <summary>
        /// 詳細ログディレクトリにあるすべての詳細ログの情報を取得する
        /// </summary>
        public void LoadInfo()
        {
            string [] files = Directory.GetFiles(LogDir.DETAIL_XML_DIR, "*.xml");

            tag_log.Clear();
            date_log.Clear();

            foreach (string path in files)
            {
                Debug.WriteLine(path);
                DetailLogInfo info = new DetailLogInfo();
                info.FileName = path;

                FileStream fs = new FileStream(path,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                XmlTextReader reader = new XmlTextReader(fs);
                try
                {
                    bool b_tag = false;
                    bool b_comment = false;
                    bool b_date = false;
                    bool b_finish = false;
                    while (reader.Read() && !b_finish)
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name == "Tag")
                                {
                                    b_tag = true;
                                }
                                else if (reader.Name == "Comment")
                                {
                                    b_tag = false;
                                    b_comment = true;
                                }
                                else if (reader.Name == "Date")
                                {
                                    b_comment = false;
                                    b_date = true;
                                }
                                /*
                                if (reader.MoveToFirstAttribute())
                                {
                                    do
                                    {
                                        Debug.WriteLine(string.Format("属性を発見 {0}={1}", reader.Name, reader.Value));
                                    } while (reader.MoveToNextAttribute());
                                }
                                */
                                break;
                            case XmlNodeType.Text:
                                if (b_tag)
                                {
                                    b_tag = false;
                                    info.Tag.AddRange(reader.Value.Split(new char[] { ',' }));
                                }
                                if (b_comment)
                                {
                                    b_comment = false;
                                    info.Comment = reader.Value;
                                }
                                if (b_date)
                                {
                                    b_date = false;
                                    info.Date = DateTime.Parse(reader.Value);
                                    b_finish = true;
                                }
                                break;
                        }
                    }
                }
                finally
                {
                    fs.Close();
                    reader.Close();
                }

                foreach (string tag in info.Tag)
                {
                    if(!tag_log.ContainsKey(tag)){
                        tag_log[tag] = new List<DetailLogInfo>();
                    }
                    tag_log[tag].Add(info);
                }
                string date = info.Date.ToString(DATE_FORMAT);
                if (!date_log.ContainsKey(date))
                {
                    date_log[date] = new List<DetailLogInfo>();
                }
                date_log[date].Add(info);

                /*
                foreach (string tag in TagList)
                {
                    Debug.Write(tag + ",");
                }
                Debug.Write("\n");
                Debug.WriteLine(info.Comment);
                Debug.WriteLine(info.Date.ToString());
                 * */
            }
        }
    }
}
