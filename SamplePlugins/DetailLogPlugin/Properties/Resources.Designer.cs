﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:2.0.50727.1433
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DetailLogPlugin.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DetailLogPlugin.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   厳密に型指定されたこのリソース クラスを使用して、すべての検索リソースに対し、
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   CSV出力エラー に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string CsvErrMsgTitle {
            get {
                return ResourceManager.GetString("CsvErrMsgTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   ファイル出力結果 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string FileOutputTitle {
            get {
                return ResourceManager.GetString("FileOutputTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   ファイル出力に失敗しました。別のプロセスがファイルを開いていないか確認してください。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string FileSaveFailed {
            get {
                return ResourceManager.GetString("FileSaveFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   詳細ログリストから一つ以上のログを選択してください。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string NoLogSelect {
            get {
                return ResourceManager.GetString("NoLogSelect", resourceCulture);
            }
        }
    }
}
