using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NMC.Model;

public partial class FrameAnalysis : ObservableObject
{
    [ObservableProperty]
    private string? _frameAnalysisPath;

    public bool IsValid => string.IsNullOrEmpty(FrameAnalysisPath) && Directory.Exists(FrameAnalysisPath);
}
