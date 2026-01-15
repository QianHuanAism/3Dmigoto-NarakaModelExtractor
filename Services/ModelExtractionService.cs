using NMC.Core;
using NMC.Helpers;
using NMC.Utils.CST;
using NMC.Utils.FMT;
using NMC.Utils.IB;
using NMC.Utils.Others;
using NMC.Utils.VB;
using System.IO;
using System.Windows;

namespace NMC.Services;

public class ModelExtractionService : IModelExtractionService
{
    private VBStrideCollector strideCollector = new VBStrideCollector();
    private VBVertexCountCollector vertexCountCollector = new VBVertexCountCollector();
    private VBValidElementCollector validElementCollector = new VBValidElementCollector();
    private VBInputElementCollector inputElementCollector = new VBInputElementCollector();
    private string? frameAnalysisPath;
    private string? outputPath;
    private string? outputName;

    public ModelExtractionService(string frameAnalysisPath, string? outputPath)
    {
        this.frameAnalysisPath = frameAnalysisPath;
        this.outputPath = outputPath;
    }

    public void Extract(string ibHash, string? alias)
    {
        // 考虑到可能有人在添加了以后又会删除, 所以可能即使通过了断言, 但是其实还是会有空字符串, 所以这里做二次判断
        if (string.IsNullOrEmpty(ibHash))
        {
            return;
        }

        Log.Info($"开始收集DrawIB: {ibHash} 的绘制调用");

        // 用于存储当前 IB 对应的 DrawCall
        Dictionary<string, List<string>> ibDrawCallMap = new Dictionary<string, List<string>>();
        DrawCallCollector drawCallCollector = new DrawCallCollector();
        List<string>? ibDrawCallList = drawCallCollector.CollectIBDrawCall(
            frameAnalysisPath!,
            ibHash
        );
        if (ibDrawCallList == null)
        {
            Log.Info($"{ibHash} 对应的 DrawCall 均为空, 开始收集下一个 DrawIB");
            return;
        }
        ibDrawCallList.ForEach(dc => Log.Info($"绘制调用: {dc}"));
        ibDrawCallMap.Add(ibHash, ibDrawCallList);

        Log.Info($"DrawIB: {ibHash} 的绘制调用收集结束");
        Log.Info("-", 66);

        Log.Info("开始收集绘制调用对应的 IB 文件");
        Dictionary<string, List<string>>? ibTxtFiles = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>>? ibBufFiles = new Dictionary<string, List<string>>();
        IBFileCollector ibFileCollector = new IBFileCollector();
        ibTxtFiles = ibFileCollector.CollectTxtIBFile(frameAnalysisPath!, ibDrawCallMap);
        ibBufFiles = ibFileCollector.CollectBufIBFile(frameAnalysisPath!, ibDrawCallMap);

        if (ibTxtFiles == null || ibBufFiles == null)
        {
            MessageHelper.Show(
                $"未找到绘制调用对应的任何 IB 文件, 提取失败!",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }
        Log.Info("收集绘制调用对应的 IB 文件结束");
        Log.Info("-", 66);

        Log.Info("开始收集绘制调用对应的 VB 文件");

        Dictionary<string, List<string>>? vbTxtFiles = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>>? vbBufFiles = new Dictionary<string, List<string>>();
        VBCollector vbCollector = new VBCollector();
        vbTxtFiles = vbCollector.CollectTxtVBFile(frameAnalysisPath!, ibDrawCallMap);
        vbBufFiles = vbCollector.CollectBufVBFile(frameAnalysisPath!, ibDrawCallMap);
        if (vbTxtFiles == null || vbBufFiles == null)
        {
            MessageHelper.Show(
                $"未找到绘制调用对应的任何 VB 文件, 提取失败!",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }

        Log.Info("收集绘制调用对应的 VB 文件结束");
        Log.Info("-", 66);

        Log.Info($"开始分析当前 DrawIB {ibHash} 对应的 VB 文件");

        Dictionary<string, string> vbStrides = strideCollector.GetStride(
            frameAnalysisPath!,
            vbTxtFiles
        );
        Dictionary<string, string> vbVertexCounts = vertexCountCollector.GetVBVertexCount(
            frameAnalysisPath!,
            vbTxtFiles
        );
        List<Dictionary<string, Dictionary<string, string>>>? vbValidElementList =
            validElementCollector.GetVBValidElementList(frameAnalysisPath!, vbTxtFiles);
        List<Dictionary<string, List<string>>>? vbInputElementList =
            inputElementCollector.GetVBInputElementList(
                frameAnalysisPath!,
                vbTxtFiles,
                vbValidElementList
            );

        Log.Info($"当前 DrawIB {ibHash} 对应的 VB 文件分析结束");
        Log.Info("-", 66);

        Log.Info($"开始分析 \"log.txt\" 文件");

        string logFilePath = Path.Combine(frameAnalysisPath!, "log.txt");
        VBHashCollector vbHashCollector = new VBHashCollector();
        Dictionary<string, string>? drawCallToVBHashMap = vbHashCollector.CollectVB0Hash(
            vbTxtFiles
        );
        LogFileAnalyzer logFileAnalyzer = new LogFileAnalyzer(logFilePath);
        List<string> cstFileList = new List<string>();
        cstFileList = logFileAnalyzer.AnalyzeLog(drawCallToVBHashMap);

        Log.Info($"分析 \"log.txt\" 文件结束");
        Log.Info("-", 66);

        Log.Info("开始分析 CS-T 文件");

        CSTStrideCollector cstStrideCollector = new CSTStrideCollector(frameAnalysisPath!);
        CSTInputElementCollector cstInputElementCollector = new CSTInputElementCollector();
        Dictionary<string, string> cstStrides = cstStrideCollector.GetCSTStride(
            vbVertexCounts,
            cstFileList
        );
        List<Dictionary<string, List<string>>> cstInputElementList =
            cstInputElementCollector.GetCSTInputElementList(cstFileList, cstStrides);
        Log.Info("CS-T 文件分析结束");
        Log.Info("-", 66);

        FmtBuilder fmtBuilder = new FmtBuilder(
            vbStrides,
            cstStrides,
            vbInputElementList,
            cstInputElementList,
            vbBufFiles,
            ibTxtFiles,
            frameAnalysisPath!
        );

        List<string> fmtContentList = fmtBuilder.Build();

        // 写入.fmt文件
        PathHelper pathHelper = new PathHelper(outputPath, ibHash);
        string writePath = pathHelper.GetWritePath();
        FmtWriter fmtWriter = new FmtWriter(writePath, ibHash);
        fmtWriter.Write(fmtContentList);

        IBReplicator ibReplicator = new IBReplicator(frameAnalysisPath!, writePath);
        ibReplicator.CopyIBToOutput(ibBufFiles, alias);

        VBBuilder vbBuilder = new VBBuilder(
            writePath,
            frameAnalysisPath!,
            vbStrides,
            cstStrides,
            vbVertexCounts,
            ibHash
        );
        vbBuilder.BuildAndWriteVB(vbBufFiles, cstFileList, alias);
    }
}
