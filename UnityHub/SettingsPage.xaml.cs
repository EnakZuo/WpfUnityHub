using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using Application = System.Windows.Application;

namespace UnityHub
{
    public partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private string _projectsPath = string.Empty;
        private string _enginesPath = string.Empty;
        private string _theme = "Dark";

        public string ProjectsPath
        {
            get => _projectsPath;
            set
            {
                _projectsPath = value;
                OnPropertyChanged(nameof(ProjectsPath));
            }
        }

        public string EnginesPath
        {
            get => _enginesPath;
            set
            {
                _enginesPath = value;
                OnPropertyChanged(nameof(EnginesPath));
            }
        }

        public string Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                OnPropertyChanged(nameof(Theme));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadSettings();
        }

        private void LoadSettings()
        {
            // 加载默认设置
            ProjectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Unity Projects");
            EnginesPath = @"C:\Program Files\Unity\Hub\Editor";
            Theme = "Dark";

            // 设置默认选中的主题
            foreach (ComboBoxItem item in ThemeComboBox.Items)
            {
                if (item.Tag?.ToString() == Theme)
                {
                    ThemeComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void BrowseProjectsPath_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "选择默认项目路径";
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProjectsPath = dialog.SelectedPath;
            }
        }

        private void BrowseEnginesPath_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "选择默认引擎路径";
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                EnginesPath = dialog.SelectedPath;
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string theme)
            {
                Theme = theme;
                ApplyTheme(theme);
            }
        }

        private void ApplyTheme(string theme)
        {
            try
            {
                ApplicationTheme applicationTheme;
                switch (theme)
                {
                    case "Light":
                        applicationTheme = ApplicationTheme.Light;
                        break;
                    case "Dark":
                        applicationTheme = ApplicationTheme.Dark;
                        break;
                    case "System":
                        applicationTheme = ApplicationTheme.Unknown;
                        break;
                    default:
                        applicationTheme = ApplicationTheme.Dark;
                        break;
                }
                
                // 应用到整个应用程序
                ApplicationThemeManager.Apply(applicationTheme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 这里可以保存设置到配置文件
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "成功",
                    Content = "设置已保存"
                };
                messageBox.Show();
            }
            catch (Exception ex)
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "错误",
                    Content = $"保存设置时出错: {ex.Message}"
                };
                messageBox.Show();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
