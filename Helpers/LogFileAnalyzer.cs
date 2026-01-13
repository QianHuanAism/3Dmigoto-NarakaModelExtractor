using Dumpify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;

namespace NMC.Helpers;

public enum StatusSettingMethod
{
    UAV,
    SRV,
    None,
}

public class LogFileAnalyzer
{
    private string _logFilePath;
    private List<string> logFileContentList;
    private StreamBuilder? streamBuilder;
    private StatusSettingMethod currentStatus;

    public LogFileAnalyzer(string logFilePath)
    {
        _logFilePath = logFilePath;
        logFileContentList = new List<string>();
        // 在初始化时就将Log文件的内容读进内存中, 避免每次分析都要重新读取文件
        if (File.Exists(logFilePath))
        {
            streamBuilder = new StreamBuilder();
            using var fs = streamBuilder.GetFileStream(logFilePath);
            StreamReader sr = new StreamReader(fs);
            string? line;
            while((line = sr.ReadLine()) != null)
            {
                logFileContentList.Add(line);
            }
        }
    }

    public void AnalyzeLog(Dictionary<string, string> drawCallVBHashMap)
    {
        Dictionary<string, List<string>> cstDrawCalls = new Dictionary<string, List<string>>();
        foreach (var ibDrawCall in drawCallVBHashMap.Keys)
        {
            drawCallVBHashMap[ibDrawCall].Dump();
            List<string> cstDrawCallList = GetCSTDrawCall(drawCallVBHashMap[ibDrawCall]);
            if (!cstDrawCalls.ContainsKey(drawCallVBHashMap[ibDrawCall]))
                cstDrawCalls.Add(drawCallVBHashMap[ibDrawCall], cstDrawCallList);
        }

        foreach (var vbHash in cstDrawCalls.Keys)
        {
            // 匹配资源行(例如: 0: view = ... hash = a309efd9)
            string reResource = @"^\s+(\d+):.*hash=([0-9a-fA-F]{8})";
            string reCallHeader = @"^\d{6}\s+(CSSetUnorderedAccessViews|CSSetShaderResources)";
            foreach (var vbDrawCall in cstDrawCalls[vbHash])
            {
                // 匹配方法调用行 (例如: 000056 CSSetUnorderedAccessViews...)
                foreach (var content in logFileContentList)
                {
                    var haderMacth = Regex.Match(content, reCallHeader);
                    if (haderMacth.Success)
                    {
                        string funcName = haderMacth.Groups["1"].Value;

                        switch (funcName)
                        {
                            case "CSSetUnorderedAccessViews":
                                currentStatus = StatusSettingMethod.UAV;
                                break;
                            case "CSSetShaderResources":
                                currentStatus = StatusSettingMethod.SRV;
                                break;
                            default:
                                currentStatus = StatusSettingMethod.None;
                                break;
                        }

                        // 进一步过滤掉没有指定DrawCall的着色器资源试图
                        if (currentStatus != StatusSettingMethod.None && !content.StartsWith($"{vbDrawCall} {funcName}"))
                        {
                            currentStatus = StatusSettingMethod.None;
                            // 如果不是任意一种状态, 并且不是指定的DrawCall, 则跳过这一行
                            continue;
                        }

                        // 如果是SRV, 并且是指定的DrawCall, 则分析它的下一行是否是资源定义行
                        continue;
                    }

                    if (currentStatus != StatusSettingMethod.None && content.StartsWith(" "))
                    {
                        var resMacth = Regex.Match(content, reResource);
                        if (resMacth.Success)
                        {
                            var slotIdx = int.Parse(resMacth.Groups["1"].Value);
                            resMacth.Value.Dump();
                        }
                    }
                    else if(!content.StartsWith(" "))
                    {
                        currentStatus = StatusSettingMethod.None;
                    }
                }
            }
        }
    }

    private List<string> GetCSTDrawCall(string vbHash)
    {
        List<string> cstDrawCallList = new List<string>();
        foreach (var content in logFileContentList)
        {
            string pattern = @$"^(\d+).*-u0={vbHash}";
            if (Regex.Match(content, pattern).Success)
            {
                string cstDrawCall = Regex.Match(content, pattern).Value.Substring(0, 6);
                cstDrawCallList.Add(cstDrawCall);
            }
        }

        return cstDrawCallList;
    }
}
