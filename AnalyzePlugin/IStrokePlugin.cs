using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

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
        /// 各プラグインがログの保存に使うディレクトリ
        /// プラグインから呼ばれる関数
        /// </summary>
        /// <returns></returns>
        string GetSaveDir(string pluginname);

        /// <summary>
        /// 各プラグインが設定の保存に使うディレクトリ
        /// プラグインから呼ばれる関数
        /// </summary>
        /// <returns></returns>
        string GetConfigDir(string pluginname);

        /// <summary>
        /// メインウィンドウのアイコンを取得する
        /// </summary>
        /// <returns></returns>
        Icon GetMainIcon();
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
        /// <param name="keycode">押されたキーの状態</param>
        /// <param name="militime">キーが押された時間[ミリ秒]（OSが起動してからの経過時間）</param>
        /// <param name="app_path">キーが押されたアプリケーションのフルパス</param>
        /// <param name="app_title">キーが押されたウィンドウのタイトル</param>
        void KeyDown(IKeyState keystate, int militime, string app_path, string app_title);

        /// <summary>キーが上がったときに呼び出される</summary>
        void KeyUp(IKeyState keycode, int militime, string app_path, string app_title);

        /// <summary>ディレクトリチェックなどの初期化処理</summary>
        void Init();

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
}
