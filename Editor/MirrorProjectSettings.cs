using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Unity.SettingsManagement;
using Unity.SettingsManagement.Editor;
using SettingsScope = Unity.SettingsManagement.SettingsScope;

namespace Unity.Project.Mirror.Editor
{
    public static class MirrorProjectSettings
    {
        public static readonly string DefaultRootPath = $"UserSettings/MirrorProjects";

        public static readonly string[] DefaultMirrorPaths = new string[]
        {
            "Assets",
            "ProjectSettings",
            "Packages"
        };

        private static Settings settings;

        public static Settings Settings
            => settings ??= new Settings(new PackageSettingRepository(MirrorProjectUtility.GetPackageName(), SettingsScope.EditorProject));




        [InspectorName("Include Path")]
        [PathField(IsElement = true, IsFolder = true)]
        private static Setting<List<string>> includePaths = new(Settings, nameof(IncludePaths), new(DefaultMirrorPaths), SettingsScope.EditorProject);

        public static List<string> IncludePaths
        {
            get => includePaths.Value;
            set => includePaths.SetValue(value, true);
        }


        [InspectorName("Mirror Project")]
        private static Setting<List<MirrorProjectSetting>> projects = new(Settings, nameof(Projects), new(), SettingsScope.EditorProject);

        public static List<MirrorProjectSetting> Projects
        {
            get => projects.Value;
            set => projects.SetValue(value, true);
        }

        internal static string LocalProjectFile = $"Library/MirrorProject";


        //[HideInInspector]
        //private static Setting<bool> isMirrorProject = new(Settings, nameof(IsMirrorProject), false, SettingsScope.EditorUser);

        //public static bool IsMirrorProject
        //{
        //    get => isMirrorProject.Value;
        //    set => isMirrorProject.SetValue(value, true);
        //}

        public static bool IsMirrorProject
        {
            get
            {
                return File.Exists(LocalProjectFile);
            }
        }

        public static MirrorProjectSetting Local
        {
            get
            {
                MirrorProjectSetting local = null;
                try
                {
                    if (File.Exists(LocalProjectFile))
                    {
                        local = JsonUtility.FromJson<MirrorProjectSetting>(File.ReadAllText(LocalProjectFile, Encoding.UTF8));
                    }
                }
                catch
                {
                }
                return local;
            }
        }

        public static string OwnerRootPath
        {
            get
            {
                string ownerRoot;

                var local = Local;
                if (local != null)
                {
                    ownerRoot = local.owner;
                }
                else
                {
                    ownerRoot = Environment.CurrentDirectory;
                }
                return ownerRoot;
            }
        }

    }

}
