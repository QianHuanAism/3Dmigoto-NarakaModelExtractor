using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dumpify;
using Microsoft.Win32;
using NMC.Model;
using NMC.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace NMC;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<DrawIB> DrawIBList { get; } = new ObservableCollection<DrawIB>();
    public FrameAnalysis FrameAnalysis { get; } = new FrameAnalysis();
    private Dictionary<string, List<string>> _ibDrawCalls = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>>? _vbFiles = new Dictionary<string, List<string>>();

    [RelayCommand]
    void ExtractModel()
    {
        Log.Info(":: 模型提取开始 ::");

        if (DrawIBList.Count <= 0)
        {
            DrawIBList.Add(new DrawIB
            {
                IBHash = "ef4cce2d",
                Alias = "TestModelBody"
            });

            DrawIBList.Add(new DrawIB
            {
                IBHash = "f35cf9af",
                Alias = "TestModelHair"
            });
        }

        FrameAnalysis.FrameAnalysisPath = @"E:\XXMI Launcher\GIMI\FrameAnalysis-2026-01-08-203940";

        if (DrawIBList.Count == 0)
        {
            Log.Info("DrawIB为空, 提取结束!");
            MessageBox.Show("DrawIB不能为空!!!");
            return;
        }

        foreach (DrawIB drawIB in DrawIBList)
        {
            if (string.IsNullOrEmpty(drawIB.IBHash))
            {
                Log.Info("DrawIB为空, 提取结束!");
                MessageBox.Show("DrawIB不能为空!!!");
                return;
            }
        }

        if (string.IsNullOrEmpty(FrameAnalysis.FrameAnalysisPath) || !Directory.Exists(FrameAnalysis.FrameAnalysisPath))
        {
            Log.Info("FrameAnalysisPath为空或不存在, 提取结束!");
            MessageBox.Show("请先选择FrameAnalysis文件夹!!!");
            return;
        }

        if (!Path.GetFileName(FrameAnalysis.FrameAnalysisPath.Trim()).StartsWith("FrameAnalysis-"))
        {
            Log.Info("选择的 FrameAnalysis 不符合规范，提取结束!");
            MessageBox.Show("请选择正确的FrameAnalysis文件夹!!!" + Environment.NewLine + " - FrameAnalysis文件夹名必须以FrameAnalysis-开头");
            return;
        }

        Log.Info(":: 开始收集 DrawIB 的绘制调用 ::");
        DrawCallCollector drawCallCollector = new DrawCallCollector(FrameAnalysis.FrameAnalysisPath);
        foreach (var drawIB in DrawIBList)
        {
            Log.Info($"当前DrawIB: {drawIB.IBHash}");
            List<string>? ibDrawCall = drawCallCollector.CollectIBDrawCall(drawIB.IBHash);
            if (ibDrawCall == null)
            {
                Log.Info($"IB: {drawIB.IBHash} 对应的 DrawCall 均为空, 继续收集下一个 IB");
                continue;
            }

            if (!_ibDrawCalls.ContainsKey(drawIB.IBHash))
            {
                ibDrawCall.ForEach(d => Log.Info($"绘制调用: {d}"));
                _ibDrawCalls.Add(drawIB.IBHash, ibDrawCall);
            }
        }

        if (_ibDrawCalls.Count <= 0)
        {
            MessageBox.Show("未找到任何 IB 对应的 DrawCall 提取失败!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Err(":: 未找到任何 IB 对应的 DrawCall 提取失败! ::" + Environment.NewLine + "请检查 \"FrameAnalysis\" 文件夹是否存在 \'******-ib=********\' 一类的文件");
            return;
        }
        Log.Info(":: DrawIB 的绘制调用收集结束 ::");

        Log.Info(":: 开始收集绘制调用对应的 VB 文件 ::");
        VBCollector vbCollector = new VBCollector(FrameAnalysis.FrameAnalysisPath, _ibDrawCalls);
        _vbFiles = vbCollector.CollectVBFile();
        if (_vbFiles == null)
        {
            MessageBox.Show("未找到绘制调用对应的任何 VB 文件 提取失败!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Err(":: 未找到绘制调用对应的任何 VB 文件 提取失败! ::" + Environment.NewLine + "请检查 \"FrameAnalysis\" 文件夹是否存在 \'******-vb{number}=********\' 一类的文件");
            return;
        }
        Log.Info(":: 收集绘制调用对应的 VB 文件结束 ::");
        Log.Info(":: 开始分析 VB 文件::");
        VBAnalyzer vbAnalyzer = new VBAnalyzer(FrameAnalysis.FrameAnalysisPath, _vbFiles, DrawIBList);
        // TODO: 现在的想法是先从 vertex-data 开始读取，每读取一行就格式化一下字符串以语义名称为Key，数据为Value存入字典内，
        //       如果继续读取的语义的数据已经存在于字典，那就可以结束读取了，这样就拿到了正确的语义名称，再通过正确的语义去解析语义格式，
        //       但是这样会有一个小bug，如果遇到数据完全没有重复的 vb 文件，就会导致一直读取到文件末尾，虽然不怎么影响，但是还是需要解决此问题
        //       解决方案：可以在读取时用字典去存，每次都做两个判断，先判断语义值是否存在，再判断语义名是否存在，如果有语义名相同，值不同，也会停止读取
        vbAnalyzer.Analyze();
    }

    [RelayCommand]
    void SelectFrameAnalysisFolder()
    {
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        if (folderDialog.ShowDialog() == true)
        {
            FrameAnalysis.FrameAnalysisPath = folderDialog.FolderName;
        }
    }
}
