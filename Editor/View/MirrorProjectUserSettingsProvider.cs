using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.SettingsManagement.Editor;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;

namespace Unity.Project.Mirror.Editor
{

    public class MirrorProjectUserSettingsProvider : SettingsProvider
    {
        const string SettingsPath = "Unity/Mirror Project";

        public MirrorProjectUserSettingsProvider()
          : base(SettingsPath, UnityEditor.SettingsScope.User)
        {
        }


        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new MirrorProjectUserSettingsProvider();
            provider.keywords = new string[] { "mirror", "project", "editor" };
            return provider;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var content = EditorSettingsUtility.CreateSettingsWindow(rootElement, "Mirror Project");

            EditorSettingsUtility.CreateSettingView(content, typeof(MirrorProjectUserSettings));
        }
    }
}