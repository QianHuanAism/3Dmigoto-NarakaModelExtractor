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
using System.Windows;

namespace NMC;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<DrawIB> DrawIBList { get; } = new ObservableCollection<DrawIB>();
    private readonly IModelExtractionService _extractionService;
    private readonly IModelExtractionAssertionService _extractionAssertionService;
    public FrameAnalysis FrameAnalysis { get; } = new FrameAnalysis();
    

    public MainViewModel()
    {
        _extractionService = new ModelExtractionService();
        _extractionAssertionService = new ModelExtractionAssertionService();
    }

    [RelayCommand]
    private void ExtractModel()
    {
        // XXX: 仅为测试用例, 实际发布是会注释掉
        ExtractTestCode();

        Log.Info(":: 模型提取开始 ::");

        // 如果断言不通过, 则直接结束提取
        if(!_extractionAssertionService.CanExtract(FrameAnalysis, DrawIBList))
            return;

        foreach (var drawIB in DrawIBList)
        {
            _extractionService.Extract(FrameAnalysis.FrameAnalysisPath, drawIB.IBHash);
        }
    }

    [RelayCommand]
    private void SelectFrameAnalysisFolder()
    {
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        if (folderDialog.ShowDialog() == true)
        {
            FrameAnalysis.FrameAnalysisPath = folderDialog.FolderName;
        }
    }

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

        FrameAnalysis.FrameAnalysisPath = @"E:\XXMI Launcher\GIMI\FrameAnalysis-2026-01-08-203940";
    }
}
