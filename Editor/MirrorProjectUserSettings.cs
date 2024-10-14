using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.SettingsManagement;

namespace Unity.Project.Mirror.Editor
{
    public static class MirrorProjectUserSettings
    {
        private static Settings settings;

        public static Settings Settings
            => settings ??= new Settings(new PackageSettingRepository(MirrorProjectUtility.GetPackageName(), SettingsScope.EditorUser));

        [PathField(IsFolder = true)]
        private static Setting<string> rootPath = new(Settings, nameof(RootPath), MirrorProjectSettings.DefaultRootPath, SettingsScope.EditorUser);

        public static string RootPath
        {
            get => rootPath.Value;
            set => rootPath.SetValue(value, true);
        }

        [InspectorName("Include Path")]
        [PathField(IsElement = true, IsFolder = true)]
        private static Setting<List<string>> includePaths = new(Settings, nameof(IncludePaths), new(), SettingsScope.EditorUser);

        public static List<string> IncludePaths
        {
            get => includePaths.Value;
            set => includePaths.SetValue(value, true);
        }


        [InspectorName("Exclude Path")]
        private static Setting<string> excludePath = new(Settings, nameof(ExcludePath), null, SettingsScope.EditorUser);

        public static string ExcludePath
        {
            get => excludePath.Value;
            set => excludePath.SetValue(value, true);
        }

        [InspectorName("Mirror Project")]
        private static Setting<List<MirrorProjectSetting>> projects = new(Settings, nameof(Projects), new(), SettingsScope.EditorUser);

        public static List<MirrorProjectSetting> Projects
        {
            get => projects.Value;
            set => projects.SetValue(value, true);
        }



    }


}