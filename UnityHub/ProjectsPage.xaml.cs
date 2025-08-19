using System.Windows.Controls;
using UnityHub.ViewModels;

namespace UnityHub
{
    public partial class ProjectsPage : Page
    {
        public ProjectsPage()
        {
            InitializeComponent();
            DataContext = ((App)System.Windows.Application.Current).Resources["ProjectsVm"] as ProjectsViewModel;
        }
    }
}
