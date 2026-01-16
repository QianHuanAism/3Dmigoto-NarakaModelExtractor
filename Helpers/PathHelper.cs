using System.IO;

namespace NMC.Helpers;

public class PathHelper
{
    private string writePath;
    private string ibHash;

    public PathHelper(string writePath, string ibHash)
    {
        this.writePath = writePath;
        this.ibHash = ibHash;
    }

    public static string CurrentDirectory { get; } = Directory.GetCurrentDirectory();

    public string GetWritePath()
    {
        CreateLackingDirecotory(writePath);

        return writePath;
    }

    private void CreateLackingDirecotory(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }
}
