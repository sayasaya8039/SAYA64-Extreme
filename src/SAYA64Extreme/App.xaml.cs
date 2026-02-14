using System.Windows;
using SAYA64Extreme.Services;

namespace SAYA64Extreme;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        LanguageService.Initialize();
    }
}

