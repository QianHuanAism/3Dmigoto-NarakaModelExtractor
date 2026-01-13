using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dumpify;
using Microsoft.Win32;
using NMC.Core;
using NMC.Helpers;
using NMC.Model;
using NMC.Services;
using NMC.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Policy;
using System.Windows;

namespace NMC;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<DrawIB> DrawIBList { get; } = new ObservableCollection<DrawIB>();
    public FrameAnalysis FrameAnalysis { get; } = new FrameAnalysis();
    private IModelExtractionService? _extractionService;
    private IModelExtractionAssertionService? _extractionAssertionService;

    /// <summary>
    /// 提取模型
    /// </summary>
    [RelayCommand]
    private void ExtractModel()
    {
        // XXX: 仅为测试用例, 实际发布时会注释掉
        ExtractTestCode();
        Log.Info("-", 66);
        Log.Info("模型提取开始");
        Log.Info($"FrameAnalysis文件夹路径: {FrameAnalysis.FrameAnalysisPath}");

        _extractionAssertionService = new ModelExtractionAssertionService();
        // 如果断言不通过, 则直接结束提取
        if (!_extractionAssertionService.CanExtract(FrameAnalysis, DrawIBList))
        {
            Log.Info("模型提取失败, 提取逻辑结束");
            return;
        }
        
        _extractionService = new ModelExtractionService(FrameAnalysis.FrameAnalysisPath!);
        foreach (var drawIB in DrawIBList)
        {
            Log.Info($"当前DrawIB: {drawIB.IBHash}");
            _extractionService.Extract(drawIB.IBHash);
        }
    }

    /// <summary>
    /// 选择 FrameAnalysis 文件夹
    /// </summary>
    [RelayCommand]
    private void SelectFrameAnalysisFolder()
    {
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        if (folderDialog.ShowDialog() == true)
        {
            FrameAnalysis.FrameAnalysisPath = folderDialog.FolderName;
        }
    }

    // 测试代码
    private void ExtractTestCode()
    {
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

        FrameAnalysis.FrameAnalysisPath = @"E:\XXMI Launcher\GIMI\FrameAnalysis-2026-01-12-163352";
    }
}
