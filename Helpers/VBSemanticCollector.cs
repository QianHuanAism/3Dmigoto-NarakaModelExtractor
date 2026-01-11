using Dumpify;
using NMC.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NMC.Helpers;

public class VBSemanticCollector(string frameAnalysis, Dictionary<string, List<string>> vbFiles)
{
    private StreamBuilder streamBuilder = new StreamBuilder();

    public List<Dictionary<string, Dictionary<string, string>>> GetValidSemantic()
    {
        List<Dictionary<string, Dictionary<string, string>>> fileSemanticList = new List<Dictionary<string, Dictionary<string, string>>>();
        foreach (var ibHash in vbFiles.Keys)
        {
            Dictionary<string, Dictionary<string, string>> fileSemanticMap = new Dictionary<string, Dictionary<string, string>>();
            foreach (var file in vbFiles[ibHash])
            {
                var vbFile = Path.Combine(frameAnalysis, file);

                using var fs = streamBuilder.GetFileStream(vbFile);
                var sr = new StreamReader(fs);
                List<string> contentList = new List<string>();
                Dictionary<string, string> semanticToValueMap = new Dictionary<string, string>();
                string? line;
                while((line = sr.ReadLine()) != null)
                {
                    if(line.StartsWith("vertex-data:"))
                    {
                        while((line = sr.ReadLine()) != null)
                        {
                            if (string.IsNullOrEmpty(line))
                                continue;

                            contentList.Add(line.TrimStart());
                        }
                    }
                }

                foreach (var content in contentList)
                {
                    string semanticName = content.Split(": ")[0].Split(" ")[1];
                    string semanticValue = content.Split(": ")[1];

                    if (semanticToValueMap.ContainsKey(semanticName) || semanticToValueMap.ContainsValue(semanticValue))
                        break;

                    semanticToValueMap.Add(semanticName, semanticValue);
                }

                fileSemanticMap.Add(file, GetSeamanticIndex(semanticToValueMap));
            }

            fileSemanticList.Add(fileSemanticMap);
        }

        return fileSemanticList;
    }

    private Dictionary<string, string> GetSeamanticIndex(Dictionary<string, string> semanticToValueMap)
    {
        Dictionary<string, string> semanticIndexMap = new Dictionary<string, string>();
        string pattern = @"\d+\b";
        foreach (var semanticName in semanticToValueMap.Keys)
        {
            bool isSuccess = Regex.Match(semanticName, pattern).Success;
            if(isSuccess)
            {
                string semanticIndex = Regex.Match(semanticName, pattern).Value;
                
                semanticIndexMap.Add(semanticName, semanticIndex);
            }
            else
            {
                semanticIndexMap.Add(semanticName, "0");
            }
        }

        return semanticIndexMap;
    }
}
