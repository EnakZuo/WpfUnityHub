 using System;
 using System.Collections.Generic;
 using System.IO;
 using CommunityToolkit.Mvvm.ComponentModel;
 using CommunityToolkit.Mvvm.Input;
 using UnityHub.Models;
 using Wpf.Ui.Appearance;
 
 namespace UnityHub.ViewModels
 {
 	public partial class SettingsViewModel : ObservableObject
 	{
 		[ObservableProperty]
 		private string projectsPath = string.Empty;
 
 		[ObservableProperty]
 		private string enginesPath = string.Empty;
 
 		[ObservableProperty]
 		private string theme = "Dark";
 
 		public SettingsViewModel()
 		{
 			LoadSettings();
 		}
 
 		private void LoadSettings()
 		{
 			var settings = ProjectDataService.LoadSettings();
 			ProjectsPath = settings.ContainsKey("ProjectsPath")
 				? settings["ProjectsPath"]
 				: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Unity Projects");
 			EnginesPath = settings.ContainsKey("EnginesPath")
 				? settings["EnginesPath"]
 				: @"C:\\Program Files\\Unity\\Hub\\Editor";
 			Theme = settings.ContainsKey("Theme") ? settings["Theme"] : "Dark";
 			ApplyTheme(Theme);
 		}
 
 		[RelayCommand]
 		private void BrowseProjectsPath()
 		{
 			using var dialog = new System.Windows.Forms.FolderBrowserDialog();
 			dialog.Description = "选择默认项目路径";
 			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
 			{
 				ProjectsPath = dialog.SelectedPath;
 			}
 		}
 
 		[RelayCommand]
 		private void BrowseEnginesPath()
 		{
 			using var dialog = new System.Windows.Forms.FolderBrowserDialog();
 			dialog.Description = "选择默认引擎路径";
 			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
 			{
 				EnginesPath = dialog.SelectedPath;
 			}
 		}
 
 		[RelayCommand]
 		private void SaveSettings()
 		{
 			var settings = new Dictionary<string, string>
 			{
 				["ProjectsPath"] = ProjectsPath,
 				["EnginesPath"] = EnginesPath,
 				["Theme"] = Theme
 			};
 			ProjectDataService.SaveSettings(settings);
 			new Wpf.Ui.Controls.MessageBox { Title = "成功", Content = "设置已保存" }.ShowDialogAsync();
 		}
 
 		partial void OnThemeChanged(string value)
 		{
 			ApplyTheme(value);
 		}
 
 		private static void ApplyTheme(string theme)
 		{
 			ApplicationTheme applicationTheme = theme switch
 			{
 				"Light" => ApplicationTheme.Light,
 				"Dark" => ApplicationTheme.Dark,
 				"System" => ApplicationTheme.Unknown,
 				_ => ApplicationTheme.Dark
 			};
 			ApplicationThemeManager.Apply(applicationTheme);
 		}
 	}
 }

