using Dumpify;
using System.IO;
using System.Windows.Controls.Ribbon;

namespace NMC.Helpers;

public class DrawCallCollector(string frameAnalysis)
{
    public List<string>? CollectIBDrawCall(string ibHash)
    {
        List<string>? drawCallList = new List<string>();
        foreach (var file in Directory.GetFiles(frameAnalysis).ToList())
        {
            if (file.Contains($"-ib={ibHash}"))
            {
                string drawCall = Path.GetFileName(file.Split($"-ib={ibHash}")[0]);
                drawCallList.Add(drawCall);
            }
        }

        if (drawCallList.Count <= 0)
        {
            return null;
        }
        
        return RemoveDuplicateDrawCall(drawCallList);
    }

    private List<string> RemoveDuplicateDrawCall(List<string> source)
    {
        List<string> temp = new List<string>();
        foreach (var call in source)
        {
            if(!temp.Contains(call))
            {
                temp.Add(call);
            }
        }

        temp.RemoveRange(1, temp.Count - 1);
        return temp;
    }
}