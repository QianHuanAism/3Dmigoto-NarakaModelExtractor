using System;
using System.Collections.Generic;
using System.Text;

namespace NMC.Core;

public interface IModelExtractionService
{
    void Extract(string frameAnalysisPath, string ibHash);
}
