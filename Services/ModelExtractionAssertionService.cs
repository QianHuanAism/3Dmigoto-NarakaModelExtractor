using Dumpify;
using NMC.Core;
using NMC.Helpers;
using NMC.Model;
using NMC.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace NMC.Services;

public class ModelExtractionAssertionService : IModelExtractionAssertionService
{
    /// <summary>
    /// 判断能否提取模型的断言
    /// </summary>
    /// <param name="frameAnalysis"><see cref="FrameAnalysis"/>, 存储了FrameAnalysis文件夹的路径以及判断文件夹是否存在</param>
    /// <param name="drawIBList"><see cref="ObservableCollection{DrawIB}"/>存储了用户输入的DrawIB</param>
    /// <returns>返回一个<see langword="bool"/>值, 如果返回值为<see langword="true"/>, 则表示断言通过可以提取, 反之亦然</returns>
    public bool CanExtract(FrameAnalysis frameAnalysis, ObservableCollection<DrawIB> drawIBList)
    {
        if (drawIBList.Count <= 0)
        {
            Log.Info("DrawIB为空, 提取结束!");
            MessageHelper.Show("DrawIB不能为空!");
            return false;
        }

        foreach (DrawIB drawIB in drawIBList)
        {
            if (drawIB.IsValid)
            {
                Log.Info("DrawIB为空, 提取结束!");
                MessageHelper.Show("DrawIB不能为空!");
                return false;
            }
        }

         if (frameAnalysis.IsValid)
        {
            MessageHelper.Show("请先选择FrameAnalysis文件夹!");
            Log.Info(" FrameAnalysisPath 为空或不存在, 提取结束!");
            return false;
        }

        if (!Path.GetFileName(frameAnalysis.FrameAnalysisPath!.Trim()).StartsWith("FrameAnalysis-"))
        {
            MessageHelper.Show($"请选择正确的FrameAnalysis文件夹!{Environment.NewLine} * FrameAnalysis文件夹名必须以\'FrameAnalysis-\'开头");
            Log.Info("选择的 FrameAnalysis 文件夹不符合规范，提取结束!");
            return false;
        }

        return true;
    }
}
