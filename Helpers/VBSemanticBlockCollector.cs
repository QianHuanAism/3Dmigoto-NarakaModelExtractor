using Dumpify;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace NMC.Helpers;

public class VBSemanticBlockCollector(string frameAnalysis, Dictionary<string, List<string>> vbFiles)
{
    private StreamBuilder streamBuilder = new StreamBuilder();

    public List<Dictionary<string, List<string>>> GetValidSemanticBlock(List<Dictionary<string, Dictionary<string, string>>> semanticList)
    {
        //semanticList.Dump();
        List<Dictionary<string, List<string>>> fileSemanticBlockList = new List<Dictionary<string, List<string>>>();
        foreach (var fileSemanticMap in semanticList)
        {
            Dictionary<string, List<string>> fileSemanticBlockMap = new Dictionary<string, List<string>>();
            foreach (var file in fileSemanticMap.Keys)
            {
                string vbFile = Path.Combine(frameAnalysis, file);
                List<string> contentList = new List<string>();
                List<string> semanticBlockList = new List<string>();

                using var fs = streamBuilder.GetFileStream(vbFile);
                StreamReader sr = new StreamReader(fs);
                string? line;
                while ((line = sr.ReadLine()) is not "vertex-data:")
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    contentList.Add(line.TrimStart());
                }

                foreach (var semanticName in fileSemanticMap[file].Keys)
                {
                    for (int i = 0; i < contentList.Count; i++)
                    {
                        if (contentList[i].Equals($"SemanticName: {semanticName}")
                            && contentList[i + 1].Equals($"SemanticIndex: {fileSemanticMap[file][semanticName]}"))
                        {
                            int offset = i + 6;
                            for (int j = i; j <= offset; j++)
                            {
                                semanticBlockList.Add(contentList[j]);
                            }
                        }
                    }
                }
                fileSemanticBlockMap.Add(file, semanticBlockList);
            }
            fileSemanticBlockList.Add(fileSemanticBlockMap);
        }

        return fileSemanticBlockList;
    }    
}
