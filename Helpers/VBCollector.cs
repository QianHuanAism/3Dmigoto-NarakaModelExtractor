using Dumpify;
using NMC.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NMC.Helpers;

public class VBCollector(string frameAnalysis, Dictionary<string, List<string>> ibDrawCallDict)
{
    public Dictionary<string, List<string>>? CollectVBFile()
    {        
        Dictionary<string, List<string>> vbFileDict = new Dictionary<string, List<string>>();
        foreach (var ibHash in ibDrawCallDict.Keys)
        {
            List<string> drawCalls = ibDrawCallDict[ibHash];
            List<string> vbList = new List<string>();
            foreach (var drawCall in drawCalls)
            {
                foreach (var file in Directory.GetFiles(frameAnalysis).ToList())
                {
                    if (file.Contains($"{drawCall}-vb") && Path.GetExtension(file).Equals(".txt"))
                    {
                        vbList.Add(Path.GetFileName(file));
                        Log.Info($"绘制调用: {drawCall} 对应的 VB 文件: {Path.GetFileName(file)}");
                        if (!vbFileDict.ContainsKey(ibHash))
                        {
                            vbFileDict.Add(ibHash, vbList);
                        }
                    }
                }

                if (vbList.Count <= 0)
                {
                    Log.Info($"未找到绘制调用: {drawCall} 对应的 VB 文件，继续下一个绘制调用");
                }
            }
        }

        if(vbFileDict.Count <= 0)
        {
            return null;
        }

        return vbFileDict;
    }
}
