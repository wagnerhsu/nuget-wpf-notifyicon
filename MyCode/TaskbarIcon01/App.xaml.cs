using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TaskbarIcon01
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        TaskbarIcon taskbarIcon;
        protected override void OnStartup(StartupEventArgs e)
        {
            taskbarIcon = (TaskbarIcon)FindResource("myTaskbarIcon");
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            taskbarIcon.Dispose();
        }
    }
}
