using NMC.Helpers;
using System.IO;

namespace NMC.Utils.VB;

public class VBBuilder
{
    private string writePath;
    private string frameAnalysisPath;
    private Dictionary<string, string> vbStrides;
    private Dictionary<string, string> cstStrides;
    private Dictionary<string, string> vbVertexCounts;
    private string ibHash;

    public VBBuilder(
        string writePath,
        string frameAnalysisPath,
        Dictionary<string, string> vbStrides,
        Dictionary<string, string> cstStrides,
        Dictionary<string, string> vbVertexCounts,
        string ibHash
    )
    {
        this.writePath = writePath;
        this.frameAnalysisPath = frameAnalysisPath;
        this.vbStrides = vbStrides;
        this.cstStrides = cstStrides;
        this.vbVertexCounts = vbVertexCounts;
        this.ibHash = ibHash;
    }

    public void BuildAndWriteVB(
        Dictionary<string, List<string>> vbBufFiles,
        List<string> cstFileList,
        string? alias
    )
    {
        #region TODO
        // 这里或许需要验证一下顶点数是否都是一样的, 但是感觉没什么必要, 先留着吧, 如果遇到了收集到的VB顶点数不一样以后再加逻辑
        //string previousVertex = "";
        //foreach (var vbFile in vbVertexCounts.Keys)
        //{

        //    string vertex = vbVertexCounts[vbFile];
        //    if (!(vertex == previousVertex))
        //    {

        //    }
        //    previousVertex = vertex;
        //}
        #endregion
        List<string> ValidFileList = new List<string>();
        ValidFileList.Add(GetCSTFile(cstFileList, "0"));
        var vbFileList = GetVBFile(vbBufFiles);
        foreach (var vbFile in vbFileList)
        {
            ValidFileList.Add(vbFile);
        }
        ValidFileList.Add(GetCSTFile(cstFileList, "1"));
        Write(vbStrides, cstStrides, vbVertexCounts, ValidFileList, alias);
    }

    private void Write(
        Dictionary<string, string> vbStrides,
        Dictionary<string, string> cstStrides,
        Dictionary<string, string> vbVertexCounts,
        List<string> ValidFileList,
        string? alias
    )
    {
        int currentVBVertexCount = 0;
        foreach (var vbfile in vbVertexCounts.Keys)
        {
            currentVBVertexCount = int.Parse(vbVertexCounts[vbfile]);
            break;
        }

        Dictionary<string, int> fileStrideMap = new Dictionary<string, int>();

        foreach (var file in ValidFileList)
        {
            if (cstStrides.ContainsKey(file))
            {
                fileStrideMap.Add(file, int.Parse(cstStrides[file]));
            }

            if (vbStrides.ContainsKey(Path.ChangeExtension(file, ".txt")))
            {
                fileStrideMap.Add(file, int.Parse(vbStrides[Path.ChangeExtension(file, ".txt")]));
            }
        }

        Dictionary<string, BinaryReader> fileBinaryReaderMap =
            new Dictionary<string, BinaryReader>();

        try
        {
            foreach (var file in fileStrideMap.Keys)
            {
                var sourceFile = Path.Combine(frameAnalysisPath, file);
                var fs = new StreamHelper().GetFileStream(sourceFile);
                BinaryReader sr = new BinaryReader(fs);
                fileBinaryReaderMap.Add(file, sr);
            }
            string targetFile = "";
            if (!string.IsNullOrEmpty(alias))
            {
                targetFile = Path.Combine(writePath, $"{alias}_{ibHash}.vb");
            }
            else
            {
                targetFile = Path.Combine(writePath, $"{ibHash}.vb");
            }

            if (!File.Exists(targetFile))
            {
                using (File.Create(targetFile)) { }
            }
            using var targetFileFileStream = new StreamHelper().GetFileStream(targetFile);
            BinaryWriter bw = new BinaryWriter(targetFileFileStream);

            for (int i = 0; i < currentVBVertexCount; i++)
            {
                foreach (var file in fileStrideMap.Keys)
                {
                    var sourceFile = Path.Combine(frameAnalysisPath, file);
                    for (int j = 0; j < fileStrideMap[file] / 4; j++)
                    {
                        bw.Write(fileBinaryReaderMap[file].ReadSingle());
                    }
                }
            }
        }
        finally
        {
            foreach (var file in fileBinaryReaderMap.Keys)
            {
                if (fileBinaryReaderMap.ContainsKey(file))
                {
                    fileBinaryReaderMap[file].Dispose();
                }
            }
        }
    }

    private string GetCSTFile(List<string> cstFileList, string slotIndex)
    {
        string cst0File = "";
        foreach (var cstFile in cstFileList)
        {
            if (cstFile.Contains($"cs-t{slotIndex}"))
            {
                cst0File = cstFile;
            }
        }

        return cst0File;
    }

    private List<string> GetVBFile(Dictionary<string, List<string>> vbBufFiles)
    {
        List<string> vbFileList = new List<string>();
        foreach (var ibHash in vbBufFiles.Keys)
        {
            foreach (var vbFile in vbBufFiles[ibHash])
            {
                if (!vbFile.Contains("-vb0="))
                {
                    vbFileList.Add(vbFile);
                }
            }
        }

        return vbFileList;
    }
}
