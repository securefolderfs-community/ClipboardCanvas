using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Menu
{
    public abstract partial class MenuItemViewModel : ObservableObject
    {
        [ObservableProperty] private IImage? _Icon;
        [ObservableProperty] private string? _Name;
        [ObservableProperty] private string? _Description;
        [ObservableProperty] private ConsoleKeyInfo? _KeyCombination;
    }
}
