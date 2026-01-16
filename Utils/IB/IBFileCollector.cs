using NMC.Helpers;
using System.IO;

namespace NMC.Utils.IB;

public class IBFileCollector
{
    public Dictionary<string, List<string>>? CollectTxtIBFile(
        string frameAnalysisPath,
        Dictionary<string, List<string>> ibDrawCallMap
    )
    {
        Dictionary<string, List<string>> ibFiles = new Dictionary<string, List<string>>();
        foreach (var ibHash in ibDrawCallMap.Keys)
        {
            List<string> drawCalls = ibDrawCallMap[ibHash];
            List<string> vbFileList = new List<string>();
            foreach (var drawCall in drawCalls)
            {
                foreach (var file in Directory.GetFiles(frameAnalysisPath).ToList())
                {
                    if (file.Contains($"{drawCall}-ib") && Path.GetExtension(file).Equals(".txt"))
                    {
                        vbFileList.Add(Path.GetFileName(file));
                        Log.Info($"绘制调用: {drawCall} --> {Path.GetFileName(file)}");
                    }
                }

                if (vbFileList.Count <= 0)
                {
                    Log.Err(
                        $"未找到绘制调用 {drawCall} 对应的 IB 文件提取失败, 开始提取下一个 DrawIB"
                    );
                    return null;
                }
                if (!ibFiles.ContainsKey(ibHash))
                    ibFiles.Add(ibHash, vbFileList);
            }
        }

        return ibFiles;
    }

    public Dictionary<string, List<string>>? CollectBufIBFile(
        string frameAnalysisPath,
        Dictionary<string, List<string>> ibDrawCallMap
    )
    {
        Dictionary<string, List<string>> ibFiles = new Dictionary<string, List<string>>();
        foreach (var ibHash in ibDrawCallMap.Keys)
        {
            List<string> drawCalls = ibDrawCallMap[ibHash];
            List<string> ibFileList = new List<string>();
            foreach (var drawCall in drawCalls)
            {
                foreach (var file in Directory.GetFiles(frameAnalysisPath).ToList())
                {
                    if (file.Contains($"{drawCall}-ib") && Path.GetExtension(file).Equals(".buf"))
                    {
                        ibFileList.Add(Path.GetFileName(file));
                        Log.Info($"绘制调用: {drawCall} --> {Path.GetFileName(file)}");
                    }
                }

                if (ibFileList.Count <= 0)
                {
                    Log.Err(
                        $"未找到绘制调用 {drawCall} 对应的 IB 文件提取失败, 开始提取下一个 DrawIB"
                    );
                    return null;
                }
                if (!ibFiles.ContainsKey(ibHash))
                    ibFiles.Add(ibHash, ibFileList);
            }
        }
        return ibFiles;
    }
}
