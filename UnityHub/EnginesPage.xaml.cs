using System.Windows.Controls;
using UnityHub.ViewModels;

namespace UnityHub
{
    public partial class EnginesPage : Page
    {
        public EnginesPage()
        {
            InitializeComponent();
            DataContext = new EnginesViewModel();
        }
    }
}
