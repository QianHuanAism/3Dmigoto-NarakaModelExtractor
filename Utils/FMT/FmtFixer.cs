using NMC.Model;

namespace NMC.Utils.FMT;

public class FmtFixer
{
    private Dictionary<string, string> strides;
    private List<Dictionary<string, List<string>>> inputElementList;
    private Dictionary<string, List<string>> vbBufFiles;
    private Dictionary<string, Dictionary<string, string>> strideElementFormatMap = new Dictionary<string, Dictionary<string, string>>()
    {
        ["12"] = new Dictionary<string, string> { ["TANGENT"] = "R32G32B32A32_FLOAT" },
        ["8"] = new Dictionary<string, string> { ["TEXCOORD"] = "R32G32B32A32_FLOAT" }
    };

    public FmtFixer(Dictionary<string, string> strides, List<Dictionary<string, List<string>>> inputElementList, Dictionary<string, List<string>> vbBufFiles)
    {
        this.strides = strides;
        this.inputElementList = inputElementList;
        this.vbBufFiles = vbBufFiles;
    }

    public Dictionary<string, List<string>> FixElement()
    {
        Dictionary<string, List<string>> newFileElementMap = new Dictionary<string, List<string>>();
        foreach (var fileElementMap in inputElementList)
        {
            foreach (var vbFile in fileElementMap.Keys)
            {
                string totalStride = strides[vbFile];
                List<string> inputElementList = fileElementMap[vbFile];
                if (!newFileElementMap.ContainsKey(vbFile))
                    newFileElementMap.Add(vbFile, MatchElement(inputElementList, totalStride));
            }
        }

        return newFileElementMap;
    }

    private List<string> MatchElement(List<string> inputElementList, string totalStride)
    {
        int currentStride = 0;
        int index = 0;
        foreach (var inputElement in inputElementList)
        {
            string elementName = inputElement.Split(": ")[0];
            string elementValue = inputElement.Split(": ")[1];

            switch (elementValue)
            {
                case "POSITION":
                    currentStride += CalcStride(inputElementList, index);
                    break;
                case "NORMAL":
                    currentStride += CalcStride(inputElementList, index);
                    break;
                case "TANGENT":
                    currentStride += CalcStride(inputElementList, index);
                    break;
                case "TEXCOORD":
                    currentStride += CalcStride(inputElementList, index);
                    break;
            }
            index++;
        }
        // 计算出还差多少步长
        string missingStride = (int.Parse(totalStride) - currentStride).ToString();
        if (missingStride == "0")
        {
            return inputElementList;
        }

        inputElementList = ReplaceErrorElement(inputElementList, missingStride);

        return inputElementList;
    }

    private int CalcStride(List<string> inputElementList, int index)
    {
        int offset = index + 2;
        string attributeName = inputElementList[offset].Split(": ")[0];
        string attributeValue = inputElementList[offset].Split(": ")[1];
        return DXGIFormat.SemanticNameDXGIFormatMaps[attributeValue];
    }

    private List<string> ReplaceErrorElement(List<string> inputElementList, string missingStride)
    {
        int index = 0;
        int offset = 0;
        List<string> tempElementList = new List<string>();
        tempElementList.AddRange(inputElementList);
        if (strideElementFormatMap.ContainsKey(missingStride))
        {
            foreach (var inputElement in inputElementList)
            {
                string elementName = inputElement.Split(": ")[0];
                string elementValue = inputElement.Split(": ")[1];

                switch (elementValue)
                {
                    case "TANGENT":
                        offset = index + 2;
                        tempElementList[offset] = $"{tempElementList[offset].Split(": ")[0]}: {strideElementFormatMap[missingStride][elementValue]}";
                        break;
                    case "TEXCOORD":
                        offset = index + 2;
                        tempElementList[offset] = $"{tempElementList[offset].Split(": ")[0]}: {strideElementFormatMap[missingStride][elementValue]}";
                        break;
                }
                index++;
            }
        }

        return tempElementList;
    }
}
