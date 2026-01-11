using NMC.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NMC.Core;

public interface IModelExtractionAssertionService
{
    bool CanExtract(FrameAnalysis frameAnalysis, ObservableCollection<DrawIB> drawIBList);
}
