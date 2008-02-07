using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using IronPython.Hosting;
using Plugin;

namespace TypingManager
{
    public class PythonTool{
        private static PythonTool __instance = new PythonTool();
        private PythonEngine _engine;

        private PythonTool(){}

        private PythonEngine Engine {
            get {
                if (_engine == null) {
                    _engine = new PythonEngine();
                    _engine.Sys.DefaultEncoding = Encoding.UTF8;

                    Assembly a = this.GetType().Assembly;
                    _engine.LoadAssembly(a);
                    string current_dir = Directory.GetCurrentDirectory();
                    _engine.LoadAssembly(Assembly.LoadFile(Path.Combine(current_dir,"AnalyzePlugin.dll")));
                    
                    string path = Path.Combine(Path.GetDirectoryName(a.Location), "tools");
                    _engine.Sys.path.Append(path);
                    _engine.Sys.path.Append(current_dir);
                }
                return _engine;
            }
        }

        ~PythonTool() {
            if (_engine != null) {
                _engine.Dispose();
            }
        }

        public static AnalyzeTool Create(string filename, string command)
        {
            __instance.Engine.ExecuteFile(filename);
            string file_without_ext = Path.GetFileNameWithoutExtension(filename);
            return __instance.Engine.EvaluateAs<AnalyzeTool>(file_without_ext + "()");
        }
    }
}
