//#define TEST

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NMC.Core;
using NMC.Helpers;
using NMC.Model;
using NMC.Services;
using System.Collections.ObjectModel;

namespace NMC;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<DrawIB> DrawIBList { get; } = new ObservableCollection<DrawIB>();
    public FrameAnalysis FrameAnalysis { get; } = new FrameAnalysis();
    public Output Output { get; } = new Output();
    private IModelExtractionService? _extractionService;
    private IModelExtractionAssertionService? _extractionAssertionService;
    private TestHelper testHelper = new TestHelper();

    [RelayCommand]
    private void ExtractModel()
    {
#if TEST
        FrameAnalysis.FrameAnalysisPath = testHelper.ExtractTest(DrawIBList);
        Output.OutputPath = "E:\\XXMI Launcher\\GIMI\\Model\\LanMangTest";
#endif
        Log.Info("-", 66);
        Log.Info("模型提取开始");
        Log.Info($"FrameAnalysis文件夹路径: {FrameAnalysis.FrameAnalysisPath}");

        _extractionAssertionService = new ModelExtractionAssertionService();
        // 如果断言不通过, 则直接结束提取
        if (!_extractionAssertionService.CanExtract(FrameAnalysis, DrawIBList, Output))
        {
            Log.Info("模型提取失败, 提取逻辑结束");
            Log.Info("-", 66);
            return;
        }

        _extractionService = new ModelExtractionService(FrameAnalysis.FrameAnalysisPath!, Output.OutputPath);
        foreach (var drawIB in DrawIBList)
        {
            Log.Info($"当前DrawIB: {drawIB.IBHash}");
            _extractionService.Extract(drawIB.IBHash, drawIB.Alias);
        }
    }

    [RelayCommand]
    private void ClearDrawIBList()
    {
        if (DrawIBList.Count > 0)
        {
            DrawIBList.Clear();
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

    [RelayCommand]
    private void SelectOutputFolder()
    {
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        if (folderDialog.ShowDialog() == true)
        {
            Output.OutputPath = folderDialog.FolderName;
        }
    }
}
