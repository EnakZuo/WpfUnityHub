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

                // 直接序列化模型
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
                                var unityExe = Path.Combine(data.Path, "Editor", "Unity.exe");
                                if (File.Exists(unityExe))
                                {
                                    engines.Add(new UnityEngine
                                    {
                                        Version = data.Version,
                                        Path = data.Path,
                                        IsInstalled = data.IsInstalled
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

    }
}
