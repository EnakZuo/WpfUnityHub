using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using Button = Wpf.Ui.Controls.Button;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace UnityHub
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            // ApplicationThemeManager.Apply(this);
            
            // 使用Loaded事件确保NavigationView已初始化
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置默认页面
            NavigationView.Navigate(typeof(ProjectsPage));
        }

        private void NavigationView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("NavigationView SelectionChanged event fired");
            
            if (NavigationView.SelectedItem is NavigationViewItem selectedItem)
            {
                System.Diagnostics.Debug.WriteLine($"Selected item: {selectedItem.Content}");
                
                if (selectedItem.TargetPageType != null)
                {
                    try
                    {
                        NavigationView.Navigate(selectedItem.TargetPageType);
                        System.Diagnostics.Debug.WriteLine($"Navigated to: {selectedItem.TargetPageType.Name}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                    }
                }
            }
        }
    }

    public class UnityProject : INotifyPropertyChanged
    {
        private string _name;
        private string _path;
        private string _unityVersion;
        private DateTime _lastModified;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public string UnityVersion
        {
            get => _unityVersion;
            set
            {
                _unityVersion = value;
                OnPropertyChanged(nameof(UnityVersion));
            }
        }

        public DateTime LastModified
        {
            get => _lastModified;
            set
            {
                _lastModified = value;
                OnPropertyChanged(nameof(LastModified));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UnityEngine : INotifyPropertyChanged
    {
        private string _version;
        private string _path;
        private bool _isInstalled;

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                _isInstalled = value;
                OnPropertyChanged(nameof(IsInstalled));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
