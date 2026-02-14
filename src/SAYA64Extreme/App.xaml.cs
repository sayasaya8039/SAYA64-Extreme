using System.Windows;
using SAYA64Extreme.Services;
using Application = System.Windows.Application;

namespace SAYA64Extreme;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeService.Initialize();
        LanguageService.Initialize();
    }
}

