using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.SettingsManagement.Editor;

namespace Unity.Project.Mirror.Editor
{

    public class MirrorProjectSettingsProvider : SettingsProvider
    {
        const string SettingsPath = "Unity/Mirror Project";

        public MirrorProjectSettingsProvider()
          : base(SettingsPath, UnityEditor.SettingsScope.Project)
        {
        }


        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new MirrorProjectSettingsProvider();
            provider.keywords = new string[] { "mirror", "project", "editor" };
            return provider;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            VisualElement content= EditorSettingsUtility.CreateSettingsWindow(rootElement, "Mirror Project");
            EditorSettingsUtility.CreateSettingView(content, typeof(MirrorProjectSettings));
        }
    }
}