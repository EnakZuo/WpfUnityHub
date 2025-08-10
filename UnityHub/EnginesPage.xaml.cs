using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Wpf.Ui.Controls;
using UnityHub.Models;

namespace UnityHub
{
    public partial class EnginesPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<UnityEngine> Engines { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public EnginesPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadEngines();
        }

        private void LoadEngines()
        {
            // 从默认位置加载Unity引擎
            var unityPaths = GetUnityInstallPaths();
            foreach (var path in unityPaths)
            {
                if (Directory.Exists(path))
                {
                    var version = GetUnityVersionFromPath(path);
                    Engines.Add(new UnityEngine
                    {
                        Version = version,
                        Path = path,
                        IsInstalled = true
                    });
                }
            }
        }

        private string GetUnityVersionFromPath(string unityPath)
        {
            try
            {
                var versionFile = Path.Combine(unityPath, "Editor", "Unity.exe");
                if (File.Exists(versionFile))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(versionFile);
                    return versionInfo.ProductVersion;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting Unity version from {unityPath}: {ex.Message}");
            }
            return "Unknown";
        }

        private List<string> GetUnityInstallPaths()
        {
            var paths = new List<string>();
            
            // 检查默认安装路径
            var defaultPaths = new[]
            {
                @"C:\Program Files\Unity\Hub\Editor",
                @"C:\Program Files (x86)\Unity\Hub\Editor"
            };

            foreach (var path in defaultPaths)
            {
                if (Directory.Exists(path))
                {
                    var subDirs = Directory.GetDirectories(path);
                    paths.AddRange(subDirs);
                }
            }

            return paths;
        }

        private void AddEngine_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "选择Unity引擎安装目录";
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var enginePath = dialog.SelectedPath;
                var unityExe = Path.Combine(enginePath, "Editor", "Unity.exe");
                
                if (File.Exists(unityExe))
                {
                    var version = GetUnityVersionFromPath(enginePath);
                    
                    // 检查是否已存在
                    if (!Engines.Any(e => e.Path == enginePath))
                    {
                        Engines.Add(new UnityEngine
                        {
                            Version = version,
                            Path = enginePath,
                            IsInstalled = true
                        });

                        var messageBox = new Wpf.Ui.Controls.MessageBox
                        {
                            Title = "成功",
                            Content = $"Unity引擎 {version} 已添加"
                        };
                        messageBox.Show();
                    }
                    else
                    {
                        var messageBox = new Wpf.Ui.Controls.MessageBox
                        {
                            Title = "提示",
                            Content = "该引擎已存在"
                        };
                        messageBox.Show();
                    }
                }
                else
                {
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "错误",
                        Content = "所选文件夹不是有效的Unity引擎安装目录"
                    };
                    messageBox.Show();
                }
            }
        }

        private void OpenEngineFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.DataContext is UnityEngine engine)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = engine.Path,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "错误",
                        Content = $"打开文件夹时出错: {ex.Message}"
                    };
                    messageBox.Show();
                }
            }
        }

        private void RemoveEngine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.DataContext is UnityEngine engine)
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "确认",
                    Content = $"确定要移除Unity引擎 {engine.Version} 吗？"
                };
                messageBox.Show();
                
                // 由于WPF UI的MessageBox不返回结果，我们直接移除
                Engines.Remove(engine);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
