using System.IO;
using Dumpify;
using NMC.Helpers;
using NMC.Model;

namespace NMC.Utils.FMT;

public class FmtBuilder
{
    private Dictionary<string, string> vbStrides;
    private Dictionary<string, string> cstStrides;
    private List<Dictionary<string, List<string>>> vbInputElementList;
    private List<Dictionary<string, List<string>>> cstInputElementList;
    private Dictionary<string, List<string>> vbBufFiles;
    private Dictionary<string, List<string>> ibTxtFiles;
    private string frameAnalysisPath;

    public FmtBuilder(
        Dictionary<string, string> vbStrides,
        Dictionary<string, string> cstStrides,
        List<Dictionary<string, List<string>>> vbInputElementList,
        List<Dictionary<string, List<string>>> cstInputElementList,
        Dictionary<string, List<string>> vbBufFiles,
        Dictionary<string, List<string>> ibTxtFiles,
        string frameAnalysisPath
    )
    {
        this.vbStrides = vbStrides;
        this.cstStrides = cstStrides;
        this.vbInputElementList = vbInputElementList;
        this.cstInputElementList = cstInputElementList;
        this.vbBufFiles = vbBufFiles;
        this.ibTxtFiles = ibTxtFiles;
        this.frameAnalysisPath = frameAnalysisPath;
    }

    public List<string> Build()
    {
        // 修复输入元素
        FmtFixer fmtFixer = new FmtFixer(vbStrides, vbInputElementList, vbBufFiles);
        Dictionary<string, List<string>> fixedFileElementMap = fmtFixer.FixElement();
        vbInputElementList.Clear();
        vbInputElementList.Add(fixedFileElementMap);
        // 合并输入元素
        List<string> totalInputElementList = MergeInputElement(vbInputElementList, cstInputElementList);
        string stride = GetTotalStride(totalInputElementList);
        Dictionary<string, string> alignedByteOffsetMap = CalcAlignedByteOffset(totalInputElementList);
        totalInputElementList = RebuildAlignedByteOffset(totalInputElementList, alignedByteOffsetMap);
        AddElementIndex(ref totalInputElementList);
        AddSpace(ref totalInputElementList);
        totalInputElementList = [.. GetIBFormat(), .. totalInputElementList];
        totalInputElementList.Insert(0, $"stride: {stride}");

        return totalInputElementList;
    }

    private List<string> MergeInputElement(
        List<Dictionary<string, List<string>>> vbInputElementList,
        List<Dictionary<string, List<string>>> cstInputElementList
    )
    {
        vbInputElementList.Dump();
        cstInputElementList.Dump();
        List<string> totalInputElementList = new List<string>();
        foreach (var fileElementMap in vbInputElementList)
        {
            foreach (var vbFile in fileElementMap.Keys)
            {
                totalInputElementList.AddRange(fileElementMap[vbFile]);
            }
        }

        foreach (var fileElementMap in cstInputElementList)
        {
            foreach (var cstFile in fileElementMap.Keys)
            {
                totalInputElementList.AddRange(fileElementMap[cstFile]);
            }
        }

        for (int i = 0; i < totalInputElementList.Count; i++)
        {
            if (totalInputElementList[i].StartsWith("InputSlot: "))
            {
                totalInputElementList[i] = totalInputElementList[i].Replace(totalInputElementList[i], "InputSlot: 0");
            }
        }

        return totalInputElementList;
    }

    private Dictionary<string, string> CalcAlignedByteOffset(List<string> totalInputElementList)
    {
        int start = -1;
        int current = 0;
        int byteOffset = 0;
        int previousTotal = 0;
        Dictionary<string, string> alignedByteOffsetMap = new Dictionary<string, string>();
        for (int i = 0; i < totalInputElementList.Count; i++)
        {
            string inputElement = totalInputElementList[i];
            if (inputElement.StartsWith("SemanticName:"))
            {
                start++;
                if (start == 0)
                {
                    current = DXGIFormat.DXGIFormatMaps[totalInputElementList[i + 2].Split(": ")[1]];
                    byteOffset = byteOffset + previousTotal;
                    previousTotal = current;
                    alignedByteOffsetMap.Add(inputElement.Split(": ")[1], byteOffset.ToString());
                }
                else
                {
                    current = DXGIFormat.DXGIFormatMaps[totalInputElementList[i + 2].Split(": ")[1]];
                    byteOffset = byteOffset + previousTotal;
                    previousTotal = current;
                    alignedByteOffsetMap.Add(inputElement.Split(": ")[1], byteOffset.ToString());
                }
            }
        }
        return alignedByteOffsetMap;
    }

    private List<string> RebuildAlignedByteOffset(
        List<string> totalInputElementList,
        Dictionary<string, string> alignedByteOffsetList
    )
    {
        int index = 0;
        List<string> newTotalInputElementList = new List<string>();
        newTotalInputElementList.AddRange(totalInputElementList);
        foreach (var inputElement in totalInputElementList)
        {
            foreach (var elementName in alignedByteOffsetList.Keys)
            {
                if (inputElement.StartsWith($"SemanticName: {elementName}"))
                {
                    newTotalInputElementList[index + 4] = $"AlignedByteOffset: {alignedByteOffsetList[elementName]}";
                }
            }
            index++;
        }

        return newTotalInputElementList;
    }

    private string GetTotalStride(List<string> totalInputElementList)
    {
        int totalStride = 0;
        foreach (var inputElement in totalInputElementList)
        {
            if (inputElement.StartsWith("Format: "))
            {
                totalStride += DXGIFormat.DXGIFormatMaps[inputElement.Split(": ")[1]];
            }
        }

        return totalStride.ToString();
    }

    private void AddElementIndex(ref List<string> totalInputElementList)
    {
        List<string> tempInputElemntList = new List<string>();

        for (int i = 0; i < totalInputElementList.Count; i++)
        {
            if (totalInputElementList[i].StartsWith("SemanticName: "))
            {
                List<string> tempInputElementBlockList = new List<string>();
                for (int j = i; j <= i + 6; j++)
                {
                    tempInputElementBlockList.Add(totalInputElementList[j]);
                }
                tempInputElementBlockList.Insert(0, "element[");

                foreach (var inputElement in tempInputElementBlockList)
                {
                    tempInputElemntList.Add(inputElement);
                }
            }
        }

        int elementIndex = 0;
        for (int i = 0; i < tempInputElemntList.Count; i++)
        {
            if (tempInputElemntList[i].StartsWith("element["))
            {
                tempInputElemntList[i] = tempInputElemntList[i]
                    .Replace(tempInputElemntList[i], $"element[{elementIndex}]:");
                elementIndex++;
            }
        }

        totalInputElementList = tempInputElemntList;
    }

    private void AddSpace(ref List<string> totalInputElementList)
    {
        List<string> tempList = new List<string>();
        tempList.AddRange(totalInputElementList);
        for (int i = 0; i < tempList.Count; i++)
        {
            if (tempList[i].Contains("element["))
            {
                continue;
            }

            tempList[i] = $"  {tempList[i]}";
        }

        totalInputElementList = tempList;
    }

    private List<string> GetIBFormat()
    {
        List<string> ibFormat = new List<string>();
        foreach (var ibHash in ibTxtFiles.Keys)
        {
            using var fs = new StreamHelper().GetFileStream(Path.Combine(frameAnalysisPath, ibTxtFiles[ibHash][0]));
            var sr = new StreamReader(fs);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("topology: "))
                {
                    ibFormat.Add(line);
                }

                if (line.StartsWith("format: "))
                {
                    ibFormat.Add(line);
                    break;
                }
            }
        }

        return ibFormat;
    }
}
