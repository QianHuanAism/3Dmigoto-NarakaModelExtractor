using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace NMC.Model;

public partial class DrawIB : ObservableObject
{
    [ObservableProperty]
    private string _iBHash = null!;
    [ObservableProperty]
    private string _alias = null!;

    public bool IsValid => string.IsNullOrEmpty(IBHash);
}
