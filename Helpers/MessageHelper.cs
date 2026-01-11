using NMC.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace NMC.Helpers;

public class MessageHelper
{
    public static MessageBoxResult Show(string message, string title = "信息", 
        MessageBoxButton messageBoxButton = MessageBoxButton.OK, MessageBoxImage messageBoxImage = MessageBoxImage.Information)
    {
        return MessageBox.Show(message, title, messageBoxButton, messageBoxImage);
    }
}
