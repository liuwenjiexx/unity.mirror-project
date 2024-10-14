using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Unity.SettingsManagement;
using Unity.SettingsManagement.Editor;
using Debug = UnityEngine.Debug;

namespace Unity.Project.Mirror.Editor
{
    public static class MirrorProjectUtility
    {
        #region UnityPackage

        private static string PackageName = "com.yanmonet.project.mirror";

        static Dictionary<string, string> unityPackageDirectories = new Dictionary<string, string>();

        public static string GetPackageName()
        {
            return PackageName;
        }

        public static string GetUnityPackageDirectory()
        {
            return GetUnityPackageDirectory(GetPackageName());
        }

        //2021/4/13
        internal static string GetUnityPackageDirectory(string packageName)
        {
            if (!unityPackageDirectories.TryGetValue(packageName, out var path))
            {
                var tmp = Path.Combine("Packages", packageName);
                if (Directory.Exists(tmp) && File.Exists(Path.Combine(tmp, "package.json")))
                {
                    path = tmp;
                }

                if (path == null)
                {
                    foreach (var dir in Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories))
                    {
                        if (string.Equals(Path.GetFileName(dir), packageName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (File.Exists(Path.Combine(dir, "package.json")))
                            {
                                path = dir;
                                break;
                            }
                        }
                    }
                }

                if (path == null)
                {
                    foreach (var pkgPath in Directory.GetFiles("Assets", "package.json", SearchOption.AllDirectories))
                    {
                        try
                        {
                            if (JsonUtility.FromJson<_UnityPackage>(File.ReadAllText(pkgPath, System.Text.Encoding.UTF8)).name == packageName)
                            {
                                path = Path.GetDirectoryName(pkgPath);
                                break;
                            }
                        }
                        catch { }
                    }
                }

                if (path != null)
                {
                    path = path.Replace('\\', '/');
                }
                unityPackageDirectories[packageName] = path;
            }
            return path;
        }

        [Serializable]
        class _UnityPackage
        {
            public string name;
        }

        #endregion
        internal static string GetUSSPath(string uss, Type type = null)
        {
            string dir = GetUnityPackageDirectory();
            if (string.IsNullOrEmpty(dir))
                return null;
            return $"{dir}/Editor/USS/{uss}.uss";
        }

        internal static string GetUXMLPath(string uxml, Type type = null)
        {
            string dir = GetUnityPackageDirectory();
            return $"{dir}/Editor/UXML/{uxml}.uxml";
        }


        public static string GetMirrorProjectPath(MirrorProjectSetting project)
        {
            if (string.IsNullOrEmpty(project.name))
                throw new Exception("Project name null");

            string ownerRoot = MirrorProjectSettings.OwnerRootPath;

            string projectPath;
            if (!string.IsNullOrEmpty(project.path))
            {
                projectPath = project.path;
            }
            else
            {
                projectPath = MirrorProjectUserSettings.RootPath;
            }

            if (string.IsNullOrEmpty(projectPath))
            {
                projectPath = MirrorProjectSettings.DefaultRootPath;
            }

            projectPath = Path.Combine(projectPath, project.name);
            projectPath = Path.Combine(ownerRoot, projectPath);

            return projectPath;
        }


        public static string CreateMirrorProject(MirrorProjectSetting project)
        {
            string ownerRoot = MirrorProjectSettings.OwnerRootPath;

            if (string.IsNullOrEmpty(project.name))
                throw new Exception("Project name null");

            string projectPath = GetMirrorProjectPath(project);

            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }

            List<string> includePaths = new();

            Regex excludeRegex = null;
            if (!string.IsNullOrEmpty(MirrorProjectUserSettings.ExcludePath))
            {
                excludeRegex = new Regex(MirrorProjectUserSettings.ExcludePath, RegexOptions.IgnoreCase);
            }
            foreach (var path in MirrorProjectSettings.IncludePaths
                .Concat(MirrorProjectUserSettings.IncludePaths))
            {
                if (string.IsNullOrEmpty(path))
                    continue;
                string normalPath = path.NormalPath();
                if (includePaths.Contains(normalPath))
                    continue;
                if (excludeRegex != null && excludeRegex.IsMatch(normalPath))
                {
                    continue;
                }
                includePaths.Add(normalPath);
            }

            foreach (var path in includePaths)
            {
                string targetPath = Path.Combine(ownerRoot, path);
                string sourcePath = Path.Combine(projectPath, path);
                if (Directory.Exists(targetPath))
                {
                    CommandUtility.CreateLinkDir(sourcePath, targetPath);
                }
            }
            return projectPath;
        }

        public static bool IsMirrorProject(MirrorProjectSetting project)
        {
            if (Environment.CurrentDirectory.NormalPath() == GetMirrorProjectPath(project).NormalPath())
            {
                return true;
            }
            return false;
        }

        public static string OpenMirrorProject(MirrorProjectSetting project)
        {
            string ownerRoot;


            if (IsMirrorProject(project))
            {
                string message = $"当前工程 [{project.name}] 已经打开";
                EditorUtility.DisplayDialog("工程已打开", message, "确定");
                return null;
            }

            if (MirrorProjectSettings.Local != null)
            {
                ownerRoot = MirrorProjectSettings.Local.owner;
            }
            else
            {
                ownerRoot = Environment.CurrentDirectory;
            }

            string projectPath = CreateMirrorProject(project);
            projectPath = Path.GetFullPath(projectPath);
            string args = $"-projectPath \"{projectPath}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = projectPath;
            startInfo.FileName = EditorApplication.applicationPath;

            //string platform = project.platform;
            //if (!string.IsNullOrEmpty(platform))
            //{
            //    args += $" -mirror-platform \"{platform}\"";
            //}

            //args += $" -mirror-owner \"{Environment.CurrentDirectory}\"";

            //args += " -executeMethod Yanmonet.Project.Mirror.Editor.MirrorProjectUtility.InitiazlieProject";
            //args += " -executeMethod Yanmonet.Project.Mirror.Editor.MirrorProjectUtility.InitiazlieProject2";

            if (!string.IsNullOrEmpty(project.arguments))
            {
                args += $" {project.arguments}";
            }
            startInfo.Arguments = args;


            var local = new MirrorProjectSetting()
            {
                name = project.name,
                path = projectPath,
                owner = ownerRoot,
                platform = project.platform,
                arguments = project.arguments,
                description = project.description,
            };

            string settingFile = Path.Combine(projectPath, MirrorProjectSettings.LocalProjectFile);
            Directory.CreateDirectory(Path.GetDirectoryName(settingFile));
            File.WriteAllText(settingFile, JsonUtility.ToJson(local, true));


            Process.Start(startInfo);

            return projectPath;
        }

        public static Dictionary<string, string> GetLinkDirOrFiles(string dir)
        {
            Dictionary<string, string> dic = new();
            SearchOption option;
            option = SearchOption.TopDirectoryOnly;
            //foreach (var _dir in Directory.GetDirectories(dir, "*", option))
            //{
            //    string linkTarget = GetLinkTarget(_dir);
            //    if (linkTarget != null)
            //    {
            //        dic[Path.Combine(dir, _dir)] = Path.Combine(dir, linkTarget);
            //    }
            //}

            foreach (var item in CommandUtility.GetLinkDirOrFiles(dir))
            {
                dic[item.Key] = item.Value;
            }

            foreach (var file in Directory.GetDirectories(dir, "*", option))
            {
                string linkTarget = GetLinkTarget(file);
                if (linkTarget != null)
                {
                    dic[Path.Combine(dir, file)] = Path.Combine(dir, linkTarget);
                }
            }

            return dic;
        }

        public static string GetLinkTarget(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                if ((dirInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {

                }
            }
            else
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    if ((fileInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {

                    }
                }
            }

            return null;
        }
        static string FirstLaunchLockFile = $"Temp/FirstLaunch";

        static bool IsFirstLaunch
        {
            get
            {
                return !File.Exists(FirstLaunchLockFile);
            }
        }




        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            EditorApplication.quitting += EditorApplication_quitting;


            MirrorProjectSetting local = null;

            if (MirrorProjectSettings.IsMirrorProject)
            {
                local = MirrorProjectSettings.Local ?? new MirrorProjectSetting();

                if (local.arguments != null)
                {
                    SettingsUtility.ParseCommandLineArgs(local.arguments);
                }
            }

            if (!IsFirstLaunch)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                PlayerPrefs.SetInt(FirstLaunchLockFile, 0);
                if (!File.Exists(FirstLaunchLockFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FirstLaunchLockFile));
                    File.Create(FirstLaunchLockFile);
                }
            };

            if (!MirrorProjectSettings.IsMirrorProject)
                return;


            //Debug.Log("Arguments: " + string.Join(", ", args));


            if (!string.IsNullOrEmpty(local.platform))
            {
                NamedBuildTarget namedBuildTarget = EditorSettingsUtility.SupportedNamedBuildTargets.FirstOrDefault(o => o.TargetName == local.platform);
                if (!string.IsNullOrEmpty(namedBuildTarget.TargetName))
                {
                    if (EditorUserBuildSettings.selectedBuildTargetGroup != namedBuildTarget.ToBuildTargetGroup())
                    {
                        EditorUserBuildSettings.selectedBuildTargetGroup = namedBuildTarget.ToBuildTargetGroup();
                    }
                    if (local.platform == NamedBuildTarget.Server.TargetName)
                    {
                        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
                    }
                }
            }

        }

        private static void EditorApplication_quitting()
        {
            if (File.Exists(FirstLaunchLockFile))
            {
                File.Delete(FirstLaunchLockFile);
            }
        }
    }
}