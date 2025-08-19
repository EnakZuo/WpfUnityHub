 using System;
 using System.Collections.Generic;
 using System.Collections.ObjectModel;
 using System.IO;
 using System.Linq;
 using CommunityToolkit.Mvvm.ComponentModel;
 using CommunityToolkit.Mvvm.Input;
 using Newtonsoft.Json;
 using UnityHub.Models;
 
 namespace UnityHub.ViewModels
 {
 		public partial class ProjectsViewModel : ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<UnityProject> projects = new();

		[ObservableProperty]
		private ObservableCollection<UnityEngine> availableEngines = new();

		public ProjectsViewModel()
		{
			InitializeData();
		}
 
 				private void InitializeData()
		{
			Projects = ProjectDataService.LoadProjects();
			AvailableEngines = ProjectDataService.LoadEngines();

			// 为已加载的项目设置默认引擎版本（如果尚未设置）
			foreach (var project in Projects)
			{
				if (string.IsNullOrEmpty(project.SelectedEngineVersion))
				{
					SetDefaultEngineVersion(project);
				}
			}

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
 												var newProject = new UnityProject
						{
							Name = projectName,
							Path = projectDir,
							UnityVersion = unityVersion,
							LastModified = Directory.GetLastWriteTime(projectDir)
						};
						
						// 设置默认选中的引擎版本
						SetDefaultEngineVersion(newProject);
						Projects.Add(newProject);
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
 										var newProject = new UnityProject
					{
						Name = projectName,
						Path = projectPath,
						UnityVersion = unityVersion,
						LastModified = Directory.GetLastWriteTime(projectPath)
					};
					
					// 设置默认选中的引擎版本
					SetDefaultEngineVersion(newProject);
					Projects.Add(newProject);
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
		private async void OpenProject(UnityProject? project)
		{
			if (project == null) return;
			try
			{
				// 加载引擎列表
				var engines = ProjectDataService.LoadEngines();
				
				// 优先使用用户选择的引擎版本，如果没有选择则使用项目默认版本
				var targetVersion = !string.IsNullOrEmpty(project.SelectedEngineVersion) 
					? project.SelectedEngineVersion 
					: project.UnityVersion;
				
				// 查找匹配的Unity版本（支持带changeset hash的版本号）
				var matchingEngine = engines.FirstOrDefault(e => 
					IsVersionMatch(e.Version, targetVersion));
				
				if (matchingEngine == null)
				{
					await new Wpf.Ui.Controls.MessageBox 
					{ 
						Title = "错误", 
						Content = $"未找到Unity版本 {targetVersion} 对应的引擎。\n请在引擎页面添加对应版本的Unity引擎。" 
					}.ShowDialogAsync();
					return;
				}

				// 查找Unity.exe路径
				var unityExePath = ResolveUnityExePath(matchingEngine.Path);
				if (string.IsNullOrEmpty(unityExePath) || !File.Exists(unityExePath))
				{
					await new Wpf.Ui.Controls.MessageBox 
					{ 
						Title = "错误", 
						Content = $"在引擎路径 {matchingEngine.Path} 下未找到 Unity.exe 文件。" 
					}.ShowDialogAsync();
					return;
				}

				// 使用Unity.exe -projectPath打开项目
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = unityExePath,
					Arguments = $"-projectPath \"{project.Path}\"",
					UseShellExecute = false
				});
			}
			catch (Exception ex)
			{
				await new Wpf.Ui.Controls.MessageBox 
				{ 
					Title = "错误", 
					Content = $"打开项目时出错: {ex.Message}" 
				}.ShowDialogAsync();
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

		[RelayCommand]
		private async void ImportProjects()
		{
			try
			{
				var userName = Environment.UserName;
				var unityHubProjectsPath = $@"C:\Users\{userName}\AppData\Roaming\UnityHub\projects-v1.json";
				
				if (!File.Exists(unityHubProjectsPath))
				{
					await new Wpf.Ui.Controls.MessageBox 
					{ 
						Title = "提示", 
						Content = "未找到Unity Hub项目配置文件。请确保Unity Hub已安装并至少添加过一个项目。" 
					}.ShowDialogAsync();
					return;
				}

				var jsonContent = await File.ReadAllTextAsync(unityHubProjectsPath);
				
				// 解析Unity Hub的实际JSON格式
				var hubData = JsonConvert.DeserializeObject<UnityHubData>(jsonContent);
				
				if (hubData?.data == null || hubData.data.Count == 0)
				{
					await new Wpf.Ui.Controls.MessageBox 
					{ 
						Title = "提示", 
						Content = "Unity Hub配置文件中没有找到项目。" 
					}.ShowDialogAsync();
					return;
				}

				int importedCount = 0;
				int skippedCount = 0;

				foreach (var kvp in hubData.data)
				{
					var projectPath = kvp.Key; // 项目路径作为键
					var hubProject = kvp.Value; // 项目详细信息作为值
					
					if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
					{
						skippedCount++;
						continue;
					}

					// 检查项目是否已存在
					if (Projects.Any(p => p.Path.Equals(projectPath, StringComparison.OrdinalIgnoreCase)))
					{
						skippedCount++;
						continue;
					}

					// 验证是否为有效的Unity项目
					var projectSettingsPath = Path.Combine(projectPath, "ProjectSettings", "ProjectVersion.txt");
					if (!File.Exists(projectSettingsPath))
					{
						skippedCount++;
						continue;
					}

					// 优先使用Unity Hub中的信息，如果没有则从文件系统获取
					var projectName = !string.IsNullOrEmpty(hubProject.title) ? hubProject.title : Path.GetFileName(projectPath);
					var unityVersion = !string.IsNullOrEmpty(hubProject.version) ? hubProject.version : GetUnityVersionFromProject(projectSettingsPath);
					
					// 转换Unix毫秒时间戳到DateTime
					var lastModified = DateTime.UnixEpoch.AddMilliseconds(hubProject.lastModified);
					
					var newProject = new UnityProject
					{
						Name = projectName,
						Path = projectPath,
						UnityVersion = unityVersion,
						LastModified = lastModified
					};
					
					// 设置默认选中的引擎版本
					SetDefaultEngineVersion(newProject);
					Projects.Add(newProject);
					
					importedCount++;
				}

				if (importedCount > 0)
				{
					ProjectDataService.SaveProjects(Projects);
				}

				await new Wpf.Ui.Controls.MessageBox 
				{ 
					Title = "导入完成", 
					Content = $"成功导入 {importedCount} 个项目，跳过 {skippedCount} 个项目（已存在或无效）。" 
				}.ShowDialogAsync();
			}
			catch (Exception ex)
			{
				await new Wpf.Ui.Controls.MessageBox 
				{ 
					Title = "错误", 
					Content = $"导入项目时出错: {ex.Message}" 
				}.ShowDialogAsync();
			}
		}

		// 为项目设置默认的引擎版本
		private void SetDefaultEngineVersion(UnityProject project)
		{
			// 查找匹配项目Unity版本的引擎
			var matchingEngine = AvailableEngines.FirstOrDefault(e => 
				IsVersionMatch(e.Version, project.UnityVersion));
			
			if (matchingEngine != null)
			{
				project.SelectedEngineVersion = matchingEngine.Version;
			}
		}

		// 版本匹配辅助方法，支持带changeset hash的版本号
		private static bool IsVersionMatch(string engineVersion, string projectVersion)
		{
			if (string.IsNullOrEmpty(engineVersion) || string.IsNullOrEmpty(projectVersion))
				return false;

			// 完全匹配
			if (engineVersion.Equals(projectVersion, StringComparison.OrdinalIgnoreCase))
				return true;

			// 提取主版本号（去掉changeset hash）
			var engineMainVersion = ExtractMainVersion(engineVersion);
			var projectMainVersion = ExtractMainVersion(projectVersion);

			return engineMainVersion.Equals(projectMainVersion, StringComparison.OrdinalIgnoreCase);
		}

		// 提取主版本号（去掉changeset hash）
		private static string ExtractMainVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
				return string.Empty;

			// 查找下划线位置，Unity版本格式通常是: 2022.3.14f1_eff2de9070d8
			var underscoreIndex = version.IndexOf('_');
			if (underscoreIndex > 0)
			{
				return version.Substring(0, underscoreIndex);
			}

			return version;
		}

		// 解析Unity.exe路径的辅助方法
		private static string? ResolveUnityExePath(string root)
		{
			// 常见位置快速检查
			var direct = Path.Combine(root, "Unity.exe");
			if (File.Exists(direct)) return direct;
			var editor = Path.Combine(root, "Editor", "Unity.exe");
			if (File.Exists(editor)) return editor;
			var editorX64 = Path.Combine(root, "Editor", "x64", "Unity.exe");
			if (File.Exists(editorX64)) return editorX64;
			var editorX64Release = Path.Combine(root, "Editor", "x64", "Release", "Unity.exe");
			if (File.Exists(editorX64Release)) return editorX64Release;
			var editorX86 = Path.Combine(root, "Editor", "x86", "Unity.exe");
			if (File.Exists(editorX86)) return editorX86;

			// 宽松搜索（限制为首次命中）
			try
			{
				var files = Directory.EnumerateFiles(root, "Unity.exe", SearchOption.AllDirectories);
				foreach (var f in files)
				{
					return f;
				}
			}
			catch { }

			return null;
		}

		// Unity Hub项目配置文件的数据模型
		private class UnityHubData
		{
			public string schema_version { get; set; } = string.Empty;
			public Dictionary<string, UnityHubProject> data { get; set; } = new Dictionary<string, UnityHubProject>();
		}

		private class UnityHubProject
		{
			public string title { get; set; } = string.Empty;
			public long lastModified { get; set; }
			public bool isCustomEditor { get; set; }
			public string path { get; set; } = string.Empty;
			public string containingFolderPath { get; set; } = string.Empty;
			public string version { get; set; } = string.Empty;
			public string architecture { get; set; } = string.Empty;
			public string changeset { get; set; } = string.Empty;
			public bool isFavorite { get; set; }
			public string cloudProjectId { get; set; } = string.Empty;
			public string organizationId { get; set; } = string.Empty;
			public string localProjectId { get; set; } = string.Empty;
			public bool cloudEnabled { get; set; }
			public string projectName { get; set; } = string.Empty;
		}
	}
}

