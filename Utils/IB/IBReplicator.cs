using System.IO;

namespace NMC.Utils.IB;

public class IBReplicator
{
    private string frameAnalysisPath;

    private string copyPath;

    public IBReplicator(string frameAnalysisPath, string ibPath)
    {
        this.frameAnalysisPath = frameAnalysisPath;
        this.copyPath = ibPath;
    }

    public void CopyIBToOutput(Dictionary<string, List<string>> ibBufFiles, string? alias)
    {
        foreach (var ibHash in ibBufFiles.Keys)
        {
            foreach (var ibBufFile in ibBufFiles[ibHash])
            {
                string sourcePath = Path.Combine(frameAnalysisPath, ibBufFile);
                string targetPath = "";
                if (!string.IsNullOrEmpty(alias))
                {
                    targetPath = Path.Combine(
                        copyPath,
                        ibBufFile.Replace(ibBufFile, $"{alias}_{ibHash}.ib")
                    );
                }
                else
                {
                    targetPath = Path.Combine(
                        copyPath,
                        ibBufFile.Replace(ibBufFile, $"{ibHash}.ib")
                    );
                }

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                File.Copy(sourcePath, targetPath);
            }
        }
    }
}
