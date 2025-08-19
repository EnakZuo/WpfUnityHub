using System.Windows.Controls;
using UnityHub.ViewModels;

namespace UnityHub
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = ((App)System.Windows.Application.Current).Resources["SettingsVm"] as SettingsViewModel;
        }
    }
}
