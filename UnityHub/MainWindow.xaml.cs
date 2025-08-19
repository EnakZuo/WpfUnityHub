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
using UnityHub.Views;

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
}
