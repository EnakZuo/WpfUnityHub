using System.Windows.Controls;
using UnityHub.ViewModels;

namespace UnityHub
{
    public partial class ProjectsPage : Page
    {
        public ProjectsPage()
        {
            InitializeComponent();
            DataContext = new ProjectsViewModel();
        }
    }
}
