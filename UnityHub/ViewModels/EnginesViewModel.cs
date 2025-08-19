 using System;
 using System.Collections.ObjectModel;
 using System.Diagnostics;
 using System.IO;
 using System.Linq;
 using CommunityToolkit.Mvvm.ComponentModel;
 using CommunityToolkit.Mvvm.Input;
 using UnityHub.Models;
 
 namespace UnityHub.ViewModels
 {
 	public partial class EnginesViewModel : ObservableObject
 	{
 		[ObservableProperty]
 		private ObservableCollection<UnityEngine> engines = new();
 
 		public EnginesViewModel()
 		{
 			LoadEngines();
 		}
 
 		private void LoadEngines()
 		{
 			Engines = ProjectDataService.LoadEngines();
 			if (Engines.Count == 0)
 			{
 				foreach (var path in GetUnityInstallPaths())
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
 				if (Engines.Count > 0)
 				{
 					ProjectDataService.SaveEngines(Engines);
 				}
 			}
 		}
 
 		private static string GetUnityVersionFromPath(string unityPath)
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
 
 		private static string[] GetUnityInstallPaths()
 		{
 			var defaultParent = new[]
 			{
 				@"C:\\Program Files\\Unity\\Hub\\Editor",
 				@"C:\\Program Files (x86)\\Unity\\Hub\\Editor"
 			};
 			return defaultParent.Where(Directory.Exists)
 				.SelectMany(Directory.GetDirectories)
 				.ToArray();
 		}
 
 		[RelayCommand]
 		private void AddEngine()
 		{
 			using var dialog = new System.Windows.Forms.FolderBrowserDialog();
 			dialog.Description = "选择Unity引擎安装目录";
 			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
 			{
 				var enginePath = dialog.SelectedPath;
 				var unityExe = Path.Combine(enginePath, "Editor", "Unity.exe");
 				if (File.Exists(unityExe))
 				{
 					var version = GetUnityVersionFromPath(enginePath);
 					if (!Engines.Any(e => e.Path == enginePath))
 					{
 						Engines.Add(new UnityEngine { Version = version, Path = enginePath, IsInstalled = true });
 						ProjectDataService.SaveEngines(Engines);
 						new Wpf.Ui.Controls.MessageBox { Title = "成功", Content = $"Unity引擎 {version} 已添加" }.ShowDialogAsync();
 					}
 					else
 					{
 						new Wpf.Ui.Controls.MessageBox { Title = "提示", Content = "该引擎已存在" }.ShowDialogAsync();
 					}
 				}
 				else
 				{
 					new Wpf.Ui.Controls.MessageBox { Title = "错误", Content = "所选文件夹不是有效的Unity引擎安装目录" }.ShowDialogAsync();
 				}
 			}
 		}
 
 		[RelayCommand]
 		private void OpenEngineFolder(UnityEngine? engine)
 		{
 			if (engine == null) return;
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
 				new Wpf.Ui.Controls.MessageBox { Title = "错误", Content = $"打开文件夹时出错: {ex.Message}" }.ShowDialogAsync();
 			}
 		}
 
 		[RelayCommand]
 		private void RemoveEngine(UnityEngine? engine)
 		{
 			if (engine == null) return;
 			Engines.Remove(engine);
 			ProjectDataService.SaveEngines(Engines);
 		}
 	}
 }

