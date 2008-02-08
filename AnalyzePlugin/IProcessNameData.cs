using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin
{
    public interface IProcessNameData
    {
        string GetPath(int id);
        string GetOriginalPath(int id);
        string GetName(int id);
        string GetName(string path);
        int GetID(string path);
        List<int> GetProcessList();
        int GetTotal(int id);
    }
}
