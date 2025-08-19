 using System;
 using System.Collections.ObjectModel;
 using System.IO;
 using System.Linq;
 using CommunityToolkit.Mvvm.ComponentModel;
 using CommunityToolkit.Mvvm.Input;
 using UnityHub.Models;
 
 namespace UnityHub.ViewModels
 {
 	public partial class ProjectsViewModel : ObservableObject
 	{
 		[ObservableProperty]
 		private ObservableCollection<UnityProject> projects = new();
 
 		public ProjectsViewModel()
 		{
 			InitializeData();
 		}
 
 		private void InitializeData()
 		{
 			Projects = ProjectDataService.LoadProjects();
 
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
 
 		[RelayCommand]
 		private void AddProject()
 		{
 			using var dialog = new System.Windows.Forms.FolderBrowserDialog();
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
 					ProjectDataService.SaveProjects(Projects);
 					new Wpf.Ui.Controls.MessageBox { Title = "成功", Content = $"项目 '{projectName}' 已添加" }.ShowDialogAsync();
 				}
 				else
 				{
 					new Wpf.Ui.Controls.MessageBox { Title = "错误", Content = "所选文件夹不是有效的Unity项目" }.ShowDialogAsync();
 				}
 			}
 		}
 
 		[RelayCommand]
 		private void OpenProject(UnityProject? project)
 		{
 			if (project == null) return;
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
 					new Wpf.Ui.Controls.MessageBox { Title = "错误", Content = "无法找到项目文件" }.ShowDialogAsync();
 				}
 			}
 			catch (Exception ex)
 			{
 				new Wpf.Ui.Controls.MessageBox { Title = "错误", Content = $"打开项目时出错: {ex.Message}" }.ShowDialogAsync();
 			}
 		}
 
 		[RelayCommand]
 		private void ShowInExplorer(UnityProject? project)
 		{
 			if (project == null) return;
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
 				new Wpf.Ui.Controls.MessageBox { Title = "错误", Content = $"打开资源管理器时出错: {ex.Message}" }.ShowDialogAsync();
 			}
 		}
 	}
 }

