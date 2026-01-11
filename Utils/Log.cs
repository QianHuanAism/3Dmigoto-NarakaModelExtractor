using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NMC.Utils;

public static class Log
{
    public static List<string> LogList { get; } = new List<string>();
    public static string TimeStamp { get; } = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

    public static void Info(string content)
    {
        LogList.Add($"[{TimeStamp} INFO] {content}");
    }
    public static void Info(string content, int count)
    {
        StringBuilder res = new StringBuilder();
        for (int i = 0; i <= count; i++)
        {
            res.Append(content);
        }

        LogList.Add($"[{TimeStamp} INFO] {res.ToString()}");
    }

    public static void Err(string content)
    {
        LogList.Add($"[{TimeStamp} ERROR] {content}");
    }
}
