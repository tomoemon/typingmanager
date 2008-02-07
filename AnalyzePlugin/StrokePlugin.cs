using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Plugin
{
    public interface IStrokePluginController
    {
        /// <summary>
        /// 他のプラグインに関する情報を返す
        /// プラグインから呼ばれる関数
        /// </summary>
        /// <param name="name">情報の欲しいプラグインの名前</param>
        /// <returns></returns>
        object GetInfo(string name);

        /// <summary>
        /// 各プラグインが保存に使っても良いディレクトリを返す．
        /// プラグインから呼ばれる関数
        /// </summary>
        /// <returns></returns>
        string GetSaveDir(string pluginname);
    }

    public interface IStrokePlugin
    {
        /// <summary>プラグインの名前を返すこと(英数字)</summary>
        string GetPluginName();

        /// <summary>メインフォームのメニューに追加するための日本語の名前を返すこと</summary>
        string GetAccessName();

        /// <summary>プラグインに関する簡単な説明を書くこと</summary>
        string GetComment();

        /// <summary>プラグイン作者の名前を返すこと</summary>
        string GetAuthorName();

        /// <summary>プラグインのバージョンを書くこと</summary>
        string GetVersion();

        /// <summary>キーが押されたときに呼び出される</summary>
        /// <param name="keycode">押されたキーの仮想キーコード</param>
        /// <param name="militime">キーが押された時間[ミリ秒]（OSが起動してからの経過時間）</param>
        /// <param name="app_path">キーが押されたアプリケーションのフルパス</param>
        /// <param name="app_title">キーが押されたウィンドウのタイトル</param>
        void KeyDown(int keycode, int militime, string app_path, string app_title);

        /// <summary>キーが上がったときに呼び出される</summary>
        void KeyUp(int keycode, int militime, string app_path, string app_title);

        /// <summary>ログ保存などの終了処理</summary>
        void Close();

        /// <summary>プラグイン設定用のフォームを持っているか</summary>
        bool IsHasConfigForm();

        /// <summary>プラグイン設定用のフォームを表示</summary>
        void ShowConfigForm();

        /// <summary>
        /// 他のプラグインでも使える情報を渡したいときに使う
        /// IStrokePluginControllerから呼ばれる
        /// </summary>
        /// <returns></returns>
        object GetInfo();

        /// <summary>メインメニューに加えるメニューを返す</summary>
        List<ToolStripMenuItem> GetToolStripMenu();

        IStrokePluginController Controller
        {
            get;
            set;
        }

        /// <summary>現在プラグインが有効になっているかどうかを返す</summary>
        bool Valid
        {
            get;
            set;
        }

        /// <summary>メインフォームの情報を受け取る</summary>
        Form MainForm
        {
            get;
            set;
        }
    }

    public class BaseStrokePlugin :IStrokePlugin
    {
        /// <summary>プラグインの名前を返すこと</summary>
        public virtual string GetPluginName() { return ""; }

        public virtual string GetAccessName() { return ""; }

        /// <summary>プラグインに関する簡単な説明を書くこと</summary>
        public virtual string GetComment() { return ""; }

        /// <summary>プラグイン作者の名前を返すこと</summary>
        public virtual string GetAuthorName() { return ""; }

        /// <summary>プラグインのバージョンを書くこと</summary>
        public virtual string GetVersion() { return ""; }

        /// <summary>キーが押されたときに呼び出される</summary>
        /// <param name="keycode">押されたキーの仮想キーコード</param>
        /// <param name="militime">キーが押された時間[ミリ秒]（OSが起動してからの経過時間）</param>
        /// <param name="app_path">キーが押されたアプリケーションのフルパス</param>
        /// <param name="app_title">キーが押されたウィンドウのタイトル</param>
        public virtual void KeyDown(int keycode, int militime, string app_path, string app_title) { }

        /// <summary>キーが上がったときに呼び出される</summary>
        public virtual void KeyUp(int keycode, int militime, string app_path, string app_title) { }

        /// <summary>ログ保存などの終了処理</summary>
        public virtual void Close(){}

        /// <summary>プラグイン設定用のフォームを持っているか</summary>
        public virtual bool IsHasConfigForm() { return false; }

        /// <summary>プラグイン設定用のフォームを表示</summary>
        public virtual void ShowConfigForm() { }

        /// <summary>
        /// 他のプラグインでも使える情報を渡したいときに使う
        /// IStrokePluginControllerから呼ばれる
        /// </summary>
        /// <returns></returns>
        public virtual object GetInfo() { return null; }

        /// <summary>メインメニューに加えるメニューを返す</summary>
        public virtual List<ToolStripMenuItem> GetToolStripMenu() { return null; }

        public IStrokePluginController Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

        public bool Valid
        {
            get { return _valid; }
            set { _valid = value; }
        }

        public Form MainForm
        {
            get { return _mainform; }
            set { _mainform = value; }
        }

        private IStrokePluginController _controller;
        private bool _valid;
        private Form _mainform;
    }
}
