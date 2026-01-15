using System.IO;
using System.Text.RegularExpressions;
using NMC.Helpers;

namespace NMC.Utils.VB;

public class VBValidElementCollector
{
    private StreamHelper streamBuilder;

    public VBValidElementCollector()
    {
        streamBuilder = new StreamHelper();
    }

    public List<Dictionary<string, Dictionary<string, string>>> GetVBValidElementList(
        string frameAnalysis,
        Dictionary<string, List<string>> vbFiles
    )
    {
        List<Dictionary<string, Dictionary<string, string>>> fileSemanticList =
            new List<Dictionary<string, Dictionary<string, string>>>();
        foreach (var ibHash in vbFiles.Keys)
        {
            Dictionary<string, Dictionary<string, string>> fileSemanticMap =
                new Dictionary<string, Dictionary<string, string>>();
            foreach (var file in vbFiles[ibHash])
            {
                var vbFile = Path.Combine(frameAnalysis, file);

                using var fs = streamBuilder.GetFileStream(vbFile);
                var sr = new StreamReader(fs);
                List<string> contentList = new List<string>();
                Dictionary<string, string> semanticToValueMap = new Dictionary<string, string>();
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("vertex-data:"))
                    {
                        while ((line = sr.ReadLine()) != null)
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
                    {
                        break;
                    }

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
            if (isSuccess)
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
