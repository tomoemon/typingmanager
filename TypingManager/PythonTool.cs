using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using IronPython.Hosting;
using Plugin;
using Microsoft.Scripting.Hosting;

namespace TypingManager
{
    public class PythonTool{
        private static PythonTool __instance = new PythonTool();
        private ScriptEngine _engine;

        private PythonTool(){}

        private ScriptEngine Engine {
            get {
                if (_engine == null) {
                    _engine = Python.CreateEngine();

                    Assembly a = this.GetType().Assembly;
                    _engine.Runtime.LoadAssembly(a);
                    string current_dir = Directory.GetCurrentDirectory();
                    _engine.Runtime.LoadAssembly(Assembly.LoadFile(Path.Combine(current_dir,"AnalyzePlugin.dll")));
                    
                    string path = Path.Combine(Path.GetDirectoryName(a.Location), "tools");
                    var searchPath = _engine.GetSearchPaths();
                    searchPath.Add(path);
                    _engine.SetSearchPaths(searchPath);
                }
                return _engine;
            }
        }

        ~PythonTool() {
            if (_engine != null) {
                _engine.Runtime.Shutdown();
            }
        }

        public static AnalyzeTool Create(string filename, string command)
        {
            __instance.Engine.ExecuteFile(filename);
            string file_without_ext = Path.GetFileNameWithoutExtension(filename);
            return __instance.Engine.Execute<AnalyzeTool>(file_without_ext + "()");
        }
    }
}
