using Dumpify;
using NMC.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace NMC.Helpers;

public class VBAnalyzer(string frameAnalysis, Dictionary<string, List<string>> vbFiles)
{
    private VBStrideCollector strideCollector = new VBStrideCollector(frameAnalysis, vbFiles);
    private VBVertexCountCollector vertexCountCollector = new VBVertexCountCollector(frameAnalysis, vbFiles);
    private VBSemanticCollector semanticCollector = new VBSemanticCollector(frameAnalysis, vbFiles);
    private VBSemanticBlockCollector semanticBlockCollector = new VBSemanticBlockCollector(frameAnalysis, vbFiles);

    public void Analyze()
    {
        var strides = strideCollector.GetStride();
        var vertexCounts = vertexCountCollector.GetVBVertexCount();
        var semanticList = semanticCollector.GetValidSemantic();
        var semanticBlockList = semanticBlockCollector.GetValidSemanticBlock(semanticList);
    }
}
