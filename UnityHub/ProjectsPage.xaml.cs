using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Wpf.Ui.Controls;
using UnityHub.Models;

namespace UnityHub
{
    public partial class ProjectsPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<UnityProject> Projects { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ProjectsPage()
        {
            InitializeComponent();
            DataContext = this;
            InitializeData();
        }

        private void InitializeData()
        {
            // 首先加载已保存的项目
            Projects = ProjectDataService.LoadProjects();
            
            // 如果没有保存的项目，则扫描默认路径
            if (Projects.Count == 0)
            {
                var defaultPaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Unity Projects"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Unity Projects")
                };

                foreach (var path in defaultPaths)
                {
                    if (Directory.Exists(path))
                    {
                        LoadProjectsFromPath(path);
                    }
                }
                
                // 保存扫描到的项目
                if (Projects.Count > 0)
                {
                    ProjectDataService.SaveProjects(Projects);
                }
            }
        }

        private void LoadProjectsFromPath(string path)
        {
            try
            {
                var projectDirs = Directory.GetDirectories(path);
                foreach (var projectDir in projectDirs)
                {
                    var projectFile = Path.Combine(projectDir, "ProjectSettings", "ProjectVersion.txt");
                    if (File.Exists(projectFile))
                    {
                        var projectName = Path.GetFileName(projectDir);
                        var unityVersion = GetUnityVersionFromProject(projectFile);
                        Projects.Add(new UnityProject
                        {
                            Name = projectName,
                            Path = projectDir,
                            UnityVersion = unityVersion,
                            LastModified = Directory.GetLastWriteTime(projectDir)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading projects from {path}: {ex.Message}");
            }
        }

        private string GetUnityVersionFromProject(string projectFile)
        {
            try
            {
                var lines = File.ReadAllLines(projectFile);
                foreach (var line in lines)
                {
                    if (line.StartsWith("m_EditorVersion:"))
                    {
                        return line.Split(':')[1].Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading project version: {ex.Message}");
            }
            return "Unknown";
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "选择Unity项目文件夹";
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var projectPath = dialog.SelectedPath;
                var projectFile = Path.Combine(projectPath, "ProjectSettings", "ProjectVersion.txt");
                
                if (File.Exists(projectFile))
                {
                    var projectName = Path.GetFileName(projectPath);
                    var unityVersion = GetUnityVersionFromProject(projectFile);
                    
                    Projects.Add(new UnityProject
                    {
                        Name = projectName,
                        Path = projectPath,
                        UnityVersion = unityVersion,
                        LastModified = Directory.GetLastWriteTime(projectPath)
                    });

                    // 保存项目数据
                    ProjectDataService.SaveProjects(Projects);

                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "成功",
                        Content = $"项目 '{projectName}' 已添加"
                    };
                    messageBox.ShowDialogAsync();
                }
                else
                {
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "错误",
                        Content = "所选文件夹不是有效的Unity项目"
                    };
                    messageBox.ShowDialogAsync();
                }
            }
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.DataContext is UnityProject project)
            {
                try
                {
                    var projectFile = Path.Combine(project.Path, $"{project.Name}.unity");
                    if (File.Exists(projectFile))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = projectFile,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        var messageBox = new Wpf.Ui.Controls.MessageBox
                        {
                            Title = "错误",
                            Content = "无法找到项目文件"
                        };
                        messageBox.ShowDialogAsync();
                    }
                }
                catch (Exception ex)
                {
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "错误",
                        Content = $"打开项目时出错: {ex.Message}"
                    };
                    messageBox.ShowDialogAsync();
                }
            }
        }

        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.DataContext is UnityProject project)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = project.Path,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "错误",
                        Content = $"打开资源管理器时出错: {ex.Message}"
                    };
                    messageBox.ShowDialogAsync();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
