using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;

using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.Services
{
    public interface IApplicationService
    {
        bool IsWindowActivated { get; }

        string AppVersion { get; }

        bool IsInRestrictedAccessMode { get; set; }

        List<AppLanguageModel> AppLanguages { get; }

        AppLanguageModel AppLanguage { get; set; }

        AppLanguageModel CurrentAppLanguage { get; }

        IntPtr GetHwnd(Window wnd);
    }
}
