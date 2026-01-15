namespace NMC.Model;

public class DXGIFormat
{
    public static Dictionary<string, int> DXGIFormatMaps { get; } = new Dictionary<string, int>()
    {
        { "R32G32B32A32_FLOAT", 16},
        { "R32G32B32A32_UINT", 16},
        { "R32G32B32A32_SINT", 16},
        { "R16G16B16A16_FLOAT", 8},
        { "R16G16B16A16_UINT", 8},
        { "R16G16B16A16_SNORM", 8},
        { "R32G32B32_FLOAT", 12},
        { "R32G32B32_UINT", 12},
        { "R32G32B32_SINT", 12 },
        { "R32G32_FLOAT", 8},
        { "R32G32_UINT", 8},
        { "R16G16_FLOAT", 4},
        { "R16G16_UINT", 4},
        { "R32_FLOAT", 4},
        { "R32_UINT", 4},
        { "R8G8B8A8_UNORM", 4},
        { "R8G8B8A8_SNORM", 4},
    };
}
