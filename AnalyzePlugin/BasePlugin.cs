using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Plugin;

namespace Plugin
{
    public class BaseStrokePlugin : IStrokePlugin
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
        public virtual void KeyDown(IKeyState keycode, uint militime, string app_path, string app_title) { }

        /// <summary>キーが上がったときに呼び出される</summary>
        public virtual void KeyUp(IKeyState keycode, uint militime, string app_path, string app_title) { }

        /// <summary>ディレクトリチェックなどの初期化処理</summary>
        public virtual void Init() { }

        /// <summary>ログ保存などの終了処理</summary>
        public virtual void Close() { }

        /// <summary>プラグイン設定用のフォームを持っているか</summary>
        public virtual bool IsHasConfigForm() { return false; }

        /// <summary>プラグイン設定用のフォームを表示</summary>
        public virtual void ShowConfigForm() { }

        /// <summary>ユーザが設定した自動保存時のタイミングに呼ばれる</summary>
        public virtual void AutoSave() { }

        /// <summary>
        /// 他のプラグインでも使える情報を渡したいときに使う
        /// IStrokePluginControllerから呼ばれる
        /// </summary>
        /// <returns></returns>
        public virtual object GetInfo() { return null; }

        /// <summary>メインメニューに加えるメニューを返す</summary>
        public virtual List<ToolStripMenuItem> GetToolStripMenu() { return null; }

        public IPluginController Controller
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

        private IPluginController _controller;
        private bool _valid;
        private Form _mainform;
    }
}
