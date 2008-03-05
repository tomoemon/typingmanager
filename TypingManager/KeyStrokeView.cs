using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace TypingManager
{
    /// <summary>
    /// 詳細ロギングに関わらずキーUPイベントが発生したときに
    /// GUIコントロールを更新する
    /// </summary>
    public class KeyStrokeView
    {
        private Form1 main_form;
        private StrokeNumLog stroke_num;
        private const string DATE_FORMAT = "yyyy年MM月dd日（ddd）";
        private const string TOTAL_DAY_FORMAT = "記録日数：{0}日";

        public KeyStrokeView(Form1 main_form, StrokeNumLog stroke_num)
        {
            this.main_form = main_form;
            this.stroke_num = stroke_num;
            this.stroke_num.strokeEvent += new StrokeEventHandler(ProcessViewUpdate);
            this.stroke_num.strokeEvent += new StrokeEventHandler(DayStrokeViewUpdate);
            this.stroke_num.strokeEvent += new StrokeEventHandler(MainTabUpdate);
        }

        public void ProcessViewLoad()
        {
            ListView view = main_form.ProcessStrokeView;
            view.Items.Clear();

            // プロセス別打鍵数のリストビューを更新
            if (AppConfig.ProcessViewType == ProcessStrokeViewType.Today)
            {
                foreach (AppKeyLog log in stroke_num.AppLog.Values)
                {
                    if (log.AppID == 0) continue;

                    string app_path = stroke_num.ProcessName.GetPath(log.AppID);
                    string app_name = stroke_num.ProcessName.GetName(log.AppID);
                    view.Items.Add(app_path, app_path, "");
                    view.Items[app_path].ToolTipText = app_path;
                    view.Items[app_path].SubItems.Add(app_name);
                    view.Items[app_path].SubItems.Add(log.Total.ToString());

                    // アイコンの追加
                    Icon icon = ModuleIcon.GetIcon(app_path,
                        ModuleIcon.SHGFI_ICON | ModuleIcon.SHGFI_SMALLICON);
                    if (icon != null)
                    {
                        view.Items[app_path].ImageList.ColorDepth = ColorDepth.Depth32Bit;
                        view.Items[app_path].ImageList.Images.Add(app_path, icon);
                        view.Items[app_path].ImageKey = app_path;
                        //Debug.WriteLine("Icon added");
                    }
                }
            }
            else
            {
                foreach (ProcessNameInfo info in stroke_num.ProcessName.ProcessDic.Values)
                {
                    if (info.name == StrokeProcessName.NO_TARGET) continue;
                    view.Items.Add(info.path, info.path, "");
                    view.Items[info.path].ToolTipText = info.path;
                    view.Items[info.path].SubItems.Add(info.name);
                    view.Items[info.path].SubItems.Add(info.total.ToString());

                    // アイコンの追加
                    Icon icon = ModuleIcon.GetIcon(info.path,
                        ModuleIcon.SHGFI_ICON | ModuleIcon.SHGFI_SMALLICON);
                    if (icon != null)
                    {
                        view.Items[info.path].ImageList.ColorDepth = ColorDepth.Depth32Bit;
                        view.Items[info.path].ImageList.Images.Add(info.path, icon);
                        view.Items[info.path].ImageKey = info.path;
                        //Debug.WriteLine("Icon added");
                    }
                }   
            }
            view.Sort();
        }

        private void ProcessViewUpdate(object sender, StrokeEventArgs args)
        {
            int app_id = args.app_id;
            string app_name = args.app_name;
            string app_path = args.app_path;
            ListView view = main_form.ProcessStrokeView;

            // プロセス別打鍵数のリストビューを更新
            if (args.new_day && AppConfig.ProcessViewType == ProcessStrokeViewType.Today)
            {
                view.Items.Clear();
            }

            if (app_path == StrokeProcessName.NO_TARGET)
            {
                return;
            }

            if (args.all_new_app || (args.today_new_app &&
                        AppConfig.ProcessViewType == ProcessStrokeViewType.Today))
            {
                view.Items.Add(app_path, app_path, "");
                view.Items[app_path].ToolTipText = app_path;
                view.Items[app_path].SubItems.Add(app_name);
                view.Items[app_path].SubItems.Add(stroke_num[app_id].Total.ToString());
                Icon icon = ModuleIcon.GetIcon(app_path,
                    ModuleIcon.SHGFI_ICON | ModuleIcon.SHGFI_SMALLICON);
                if (icon != null)
                {
                    view.Items[app_path].ImageList.ColorDepth = ColorDepth.Depth32Bit;
                    view.Items[app_path].ImageList.Images.Add(app_path, icon);
                    view.Items[app_path].ImageKey = app_path;
                    //Debug.WriteLine("Icon added");
                }
            }
            if (AppConfig.ProcessViewType == ProcessStrokeViewType.Today)
            {
                view.Items[app_path].SubItems[2].Text = stroke_num[app_id].Total.ToString();
            }
            else
            {
                int app_total = stroke_num.ProcessName.GetTotal(app_id);
                view.Items[app_path].SubItems[2].Text = app_total.ToString();
            }
        }

        public void ProcessViewNameUpdate(string path, string new_name)
        {
            ListView view = main_form.ProcessStrokeView;
            if(view.Items.ContainsKey(path))
            {
                view.Items[path].SubItems[1].Text = new_name;
            }
        }

        public void DayStrokeViewLoad()
        {
            ListView view = main_form.DayStrokeView;
            view.Items.Clear();
            foreach (TotalLog log in stroke_num.AllDay.DayLog)
            {
                // 日別打鍵数のリストビューを更新
                string date = log.Date.ToString(DATE_FORMAT);
                view.Items.Add(date, date, "");
                view.Items[date].Tag = log.Date;
                view.Items[date].SubItems.Add(log.TotalApp.ToString());
                view.Items[date].SubItems.Add(log.TotalType.ToString());
                Debug.WriteLine(date);
            }
            main_form.TotalLogDayNum.Text =
                    string.Format(TOTAL_DAY_FORMAT, stroke_num.TotalDay);
            view.Sort();
        }

        private void DayStrokeViewUpdate(object sender, StrokeEventArgs args)
        {
            ListView view = main_form.DayStrokeView;

            // 日別打鍵数のリストビューを更新
            string date = args.now.ToString(DATE_FORMAT);
            if (args.new_day && !view.Items.ContainsKey(date))
            {
                view.Items.Add(date, date, "");
                view.Items[date].Tag = args.now;
                view.Items[date].SubItems.Add(stroke_num.TodayTotalApp.ToString());
                view.Items[date].SubItems.Add(stroke_num.TodayTotalType.ToString());

                main_form.TotalLogDayNum.Text = 
                    string.Format(TOTAL_DAY_FORMAT, stroke_num.TotalDay);
            }
            view.Items[date].SubItems[1].Text = stroke_num.TodayTotalApp.ToString();
            view.Items[date].SubItems[2].Text = stroke_num.TodayTotalType.ToString();
        }

        /// <summary>
        /// キーの押し上げイベントが発生したときにGUIコントロールの値を変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainTabUpdate(object sender, StrokeEventArgs args)
        {
            // メインのタブを更新
            MainTabLoad();
        }

        public void MainTabLoad()
        {
            main_form.TodayStrokeNum.Text = stroke_num.TodayTotalType.ToString();
            main_form.YesterdayStrokeNum.Text = stroke_num.YesterdayTotalType.ToString();
            main_form.TotalNum.Text = stroke_num.TotalType.ToString();
            main_form.TotalAppNum.Text = stroke_num.TotalApp.ToString();
        }
    }
}
