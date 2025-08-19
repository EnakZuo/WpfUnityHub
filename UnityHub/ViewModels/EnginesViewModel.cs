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
			// 为旧数据补齐平台标签
			foreach (var e in Engines.Where(e => string.IsNullOrWhiteSpace(e.PlatformsLabel)))
			{
				e.PlatformsLabel = BuildPlatformsLabel(e.Path);
			}
			if (Engines.Count == 0)
			{
				foreach (var path in GetUnityInstallPaths())
				{
					if (Directory.Exists(path))
					{
						var version = GetUnityVersionFromPath(path);
						Engines.Add(CreateEngine(path, version));
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
				var unityExe = ResolveUnityExePath(unityPath);
				if (!string.IsNullOrEmpty(unityExe) && File.Exists(unityExe))
				{
					var versionInfo = FileVersionInfo.GetVersionInfo(unityExe);
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
				var unityExe = ResolveUnityExePath(enginePath);
				if (!string.IsNullOrEmpty(unityExe) && File.Exists(unityExe))
				{
					var version = GetUnityVersionFromPath(enginePath);
					if (!Engines.Any(e => e.Path == enginePath))
					{
						Engines.Add(CreateEngine(enginePath, version));
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
					new Wpf.Ui.Controls.MessageBox { Title = "错误", Content = "所选文件夹下未找到 Unity.exe" }.ShowDialogAsync();
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

		private static UnityEngine CreateEngine(string path, string version)
		{
			var engine = new UnityEngine
			{
				Version = version,
				Path = path
			};
			engine.PlatformsLabel = BuildPlatformsLabel(path);
			return engine;
		}

		private static string BuildPlatformsLabel(string engineRoot)
		{
			try
			{
				var labels = new System.Collections.Generic.List<string>();
				var unityExe = ResolveUnityExePath(engineRoot);
				string playbackRoot = string.Empty;
				if (!string.IsNullOrEmpty(unityExe))
				{
					var current = new DirectoryInfo(Path.GetDirectoryName(unityExe)!);
					for (int i = 0; i < 6 && current != null; i++)
					{
						var candidate = Path.Combine(current.FullName, "Data", "PlaybackEngines");
						if (Directory.Exists(candidate))
						{
							playbackRoot = candidate;
							break;
						}
						current = current.Parent;
					}
				}
				if (string.IsNullOrEmpty(playbackRoot))
				{
					var fallback = Path.Combine(engineRoot, "Editor", "Data", "PlaybackEngines");
					if (Directory.Exists(fallback)) playbackRoot = fallback;
				}
				// 只有解析到 Unity.exe 才加入 Editor 标签
				if (!string.IsNullOrEmpty(unityExe))
				{
					labels.Add("Editor");
				}
				if (!string.IsNullOrEmpty(playbackRoot))
				{
					var map = new (string folder, string label)[]
					{
						("AndroidPlayer", "Android"),
						("iOSSupport", "iOS"),
						("WindowsStandaloneSupport", "Windows"),
						("MacStandaloneSupport", "macOS"),
						("LinuxStandaloneSupport", "Linux"),
						("WebGLSupport", "WebGL"),
						("AppleTVSupport", "tvOS"),
						("PS5Player", "PS5"),
						("PS4Player", "PS4"),
						("XboxOnePlayer", "Xbox One"),
						("XboxSeriesPlayer", "Xbox Series"),
						("Switch", "Switch")
					};

					labels.AddRange(
						map.Where(m => Directory.Exists(Path.Combine(playbackRoot, m.folder)))
							.Select(m => m.label)
					);
				}

				return string.Join("  ", labels);
			}
			catch
			{
				return string.Empty;
			}
		}

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
	}
}

