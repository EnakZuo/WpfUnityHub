using System.Windows.Controls;
using UnityHub.ViewModels;

namespace UnityHub
{
    public partial class EnginesPage : Page
    {
        public EnginesPage()
        {
            InitializeComponent();
            DataContext = ((App)System.Windows.Application.Current).Resources["EnginesVm"] as EnginesViewModel;
        }
    }
}
