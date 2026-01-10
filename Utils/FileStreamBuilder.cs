using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Text;

namespace NMC.Utils;

public record FileStreamBuilder(FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.ReadWrite)
{
    public FileStream GetFileStream(string filePath)
    {
        return new FileStream(filePath, fileMode, fileAccess);
    }
}
