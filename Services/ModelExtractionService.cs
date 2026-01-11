using NMC.Core;
using NMC.Helpers;
using NMC.Model;
using NMC.Utils;
using System.Windows;

namespace NMC.Services;

public class ModelExtractionService : IModelExtractionService
{
    private Dictionary<string, List<string>> _ibDrawCalls = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>>? _vbFiles = new Dictionary<string, List<string>>();

    public void Extract(string frameAnalysisPath, string ibHash)
    {
        Log.Info(":: 开始收集 DrawIB 的绘制调用 ::");
        DrawCallCollector drawCallCollector = new DrawCallCollector(frameAnalysisPath);
        Log.Info($"当前DrawIB: {ibHash}");
        List<string>? ibDrawCall = drawCallCollector.CollectIBDrawCall(ibHash);
        if (ibDrawCall == null)
        {
            Log.Info($"IB: {ibHash} 对应的 DrawCall 均为空, 继续收集下一个 IB");
            return;
        }

        if (!_ibDrawCalls.ContainsKey(ibHash))
        {
            ibDrawCall.ForEach(d => Log.Info($"绘制调用: {d}"));
            _ibDrawCalls.Add(ibHash, ibDrawCall);
        }

        if (_ibDrawCalls.Count <= 0)
        {
            MessageHelper.Show("未找到任何 IB 对应的 DrawCall 提取失败!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Err(":: 未找到任何 IB 对应的 DrawCall 提取失败! ::" + Environment.NewLine + "请检查 \"FrameAnalysis\" 文件夹是否存在 \'******-ib=********\' 一类的文件");
            return;
        }
        Log.Info(":: DrawIB 的绘制调用收集结束 ::");
        Log.Info("=", 92);

        Log.Info(":: 开始收集绘制调用对应的 VB 文件 ::");
        VBCollector vbCollector = new VBCollector(frameAnalysisPath, _ibDrawCalls);
        _vbFiles = vbCollector.CollectVBFile();
        if (_vbFiles == null)
        {
            MessageHelper.Show("未找到绘制调用对应的任何 VB 文件 提取失败!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Err(":: 未找到绘制调用对应的任何 VB 文件 提取失败! ::" + Environment.NewLine + "请检查 \"FrameAnalysis\" 文件夹是否存在 \'******-vb{number}=********\' 一类的文件");
            return;
        }
        Log.Info(":: 收集绘制调用对应的 VB 文件结束 ::");
        Log.Info("=", 92);
        
        Log.Info(":: 开始分析当前 DrawIB 对应的 VB 文件 ::");
        VBAnalyzer vbAnalyzer = new VBAnalyzer(frameAnalysisPath, _vbFiles);
        vbAnalyzer.Analyze();
        Log.Info(":: 当前 DrawIB 对应的 VB 文件分析结束 ::");
        Log.Info("=", 92);
    }
}
