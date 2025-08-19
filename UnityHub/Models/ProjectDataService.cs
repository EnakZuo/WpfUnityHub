using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace UnityHub.Models
{
    public class ProjectDataService
    {
        private static readonly string DataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VHub"
        );
        
        private static readonly string ProjectsFile = Path.Combine(DataDirectory, "projects.json");
        private static readonly string EnginesFile = Path.Combine(DataDirectory, "engines.json");
        private static readonly string SettingsFile = Path.Combine(DataDirectory, "settings.json");

        public static void SaveProjects(ObservableCollection<UnityProject> projects)
        {
            try
            {
                // 确保目录存在
                Directory.CreateDirectory(DataDirectory);

                // 直接序列化模型
                var json = JsonConvert.SerializeObject(projects, Formatting.Indented);
                
                File.WriteAllText(ProjectsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存项目数据时出错: {ex.Message}");
            }
        }

        public static ObservableCollection<UnityProject> LoadProjects()
        {
            var projects = new ObservableCollection<UnityProject>();

            try
            {
                if (File.Exists(ProjectsFile))
                {
                    var json = File.ReadAllText(ProjectsFile);
                    var projectData = JsonConvert.DeserializeObject<List<UnityProject>>(json);

                    if (projectData != null)
                    {
                        foreach (var data in projectData)
                        {
                            // 验证项目路径是否仍然存在
                            if (Directory.Exists(data.Path))
                            {
                                var projectFile = Path.Combine(data.Path, "ProjectSettings", "ProjectVersion.txt");
                                if (File.Exists(projectFile))
                                {
                                    projects.Add(new UnityProject
                                    {
                                        Name = data.Name,
                                        Path = data.Path,
                                        UnityVersion = data.UnityVersion,
                                        LastModified = data.LastModified
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载项目数据时出错: {ex.Message}");
            }

            return projects;
        }

        public static void SaveSettings(Dictionary<string, string> settings)
        {
            try
            {
                Directory.CreateDirectory(DataDirectory);
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置时出错: {ex.Message}");
            }
        }

        public static Dictionary<string, string> LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置时出错: {ex.Message}");
            }

            return new Dictionary<string, string>();
        }

        public static void SaveEngines(ObservableCollection<UnityEngine> engines)
        {
            try
            {
                Directory.CreateDirectory(DataDirectory);

                // 直接序列化模型（包含 PlatformsLabel）
                var json = JsonConvert.SerializeObject(engines, Formatting.Indented);
                
                File.WriteAllText(EnginesFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存引擎数据时出错: {ex.Message}");
            }
        }

        public static ObservableCollection<UnityEngine> LoadEngines()
        {
            var engines = new ObservableCollection<UnityEngine>();

            try
            {
                if (File.Exists(EnginesFile))
                {
                    var json = File.ReadAllText(EnginesFile);
                    var engineData = JsonConvert.DeserializeObject<List<UnityEngine>>(json);

                    if (engineData != null)
                    {
                        foreach (var data in engineData)
                        {
                            // 验证引擎路径是否仍然存在
                            if (Directory.Exists(data.Path))
                            {
                                var unityExe = ResolveUnityExePath(data.Path);
                                if (!string.IsNullOrEmpty(unityExe) && File.Exists(unityExe))
                                {
                                    // 直接恢复（PlatformsLabel 如果为空，ViewModel 首次会补齐）
                                    engines.Add(new UnityEngine
                                    {
                                        Version = data.Version,
                                        Path = data.Path,
                                        PlatformsLabel = data.PlatformsLabel
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载引擎数据时出错: {ex.Message}");
            }

            return engines;
        }

        private static string? ResolveUnityExePath(string root)
        {
            try
            {
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

                foreach (var f in Directory.EnumerateFiles(root, "Unity.exe", SearchOption.AllDirectories))
                {
                    return f;
                }
            }
            catch { }

            return null;
        }
    }
}
