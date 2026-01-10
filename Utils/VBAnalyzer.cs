using Dumpify;
using NMC.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace NMC.Utils;

public record VBAnalyzer(string frameAnalysis, Dictionary<string, List<string>> vbFiles, ObservableCollection<DrawIB> drawIBList)
{
    private FileStreamBuilder streamBuilder = new FileStreamBuilder();

    public void Analyze()
    {
        Dictionary<string, string> strides = GetVBFileStride();
        Dictionary<string, string> vertexCounts = GetVBFileVertexCount();
        GetVBFileCorrectSemanticName();
    }
   
    /// <summary>
    /// 获取VB文件中stride字段的值
    /// </summary>
    /// <returns>由文件名为Key, stride字段的值为Value的字典</returns>
    private Dictionary<string, string> GetVBFileStride()
    {
        Dictionary<string, string> strides = new Dictionary<string, string>();

        foreach (var ibHash in vbFiles.Keys)
        {
            foreach (var vbFile in vbFiles[ibHash])
            {
                using (var fs = streamBuilder.GetFileStream(Path.Combine(frameAnalysis, vbFile)))
                {
                    StreamReader sr = new StreamReader(fs);
                    strides.Add(vbFile, sr.ReadLine()!.Split(": ")[1]);
                }
            }
        }

        return strides;
    }

    /// <summary>
    /// 获取VB文件中vertex count字段的值
    /// </summary>
    private Dictionary<string, string> GetVBFileVertexCount()
    {
        Dictionary<string, string> vertexCounts = new Dictionary<string, string>();

        foreach (var ibHash in vbFiles.Keys)
        {
            foreach (var vbFile in vbFiles[ibHash])
            {
                using (var fs = streamBuilder.GetFileStream(Path.Combine(frameAnalysis, vbFile)))
                {
                    StreamReader sr = new StreamReader(fs);
                    string line;
                    while ((line = sr.ReadLine()!) is not null)
                    {
                        if (line.StartsWith("vertex count:"))
                        {
                            vertexCounts.Add(vbFile, line.Split(": ")[1]);
                            break;
                        }
                    }
                }
            }
        }

        return vertexCounts;
    }

    /// <summary>
    /// 获取VB文件中正确的语义名称
    /// </summary>
    /// <returns>由文件名为Key, 正确的语义名称列表为Value的字典</returns>
    private List<Dictionary<string, Dictionary<string, string>>> GetVBFileCorrectSemanticName()
    {
        // 用于存储所有正确的语义组合成的字典
        List<Dictionary<string, Dictionary<string, string>>> semanticList = new List<Dictionary<string, Dictionary<string, string>>>();
        foreach (var ibHash in vbFiles.Keys)
        {
            // 用于存储文件对应的语义
            Dictionary<string, Dictionary<string, string>> fileSemanticMap = new Dictionary<string, Dictionary<string, string>>();
            foreach (var vbFile in vbFiles[ibHash])
            {
                List<string> contentList = new List<string>();
                Dictionary<string, string> semantics = new Dictionary<string, string>();
                using (var fs = streamBuilder.GetFileStream(Path.Combine(frameAnalysis, vbFile)))
                {
                    StreamReader sr = new StreamReader(fs);
                    string? line;
                    while ((line = sr.ReadLine()) is not null)
                    {
                        if (string.IsNullOrEmpty(line))
                            continue;

                        contentList.Add(line.Trim());
                    }
                }

                for (int i = contentList.IndexOf("vertex-data:") + 1; i < contentList.Count; i++)
                {
                    string semanticName = contentList[i].Split(": ")[0].Split(" ")[1];
                    string semanticValue = contentList[i].Split(": ")[1];
                    if (!semantics.ContainsKey(semanticName) && !semantics.ContainsValue(semanticValue))
                    {
                        semantics.Add(semanticName, semanticValue);
                        if (!fileSemanticMap.ContainsKey(vbFile))
                            fileSemanticMap.Add(vbFile, semantics);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            semanticList.Add(fileSemanticMap);
        }

        return semanticList;
    }
}
