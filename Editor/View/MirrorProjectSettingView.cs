using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.SettingsManagement;
using Unity.SettingsManagement.Editor;

namespace Unity.Project.Mirror.Editor
{
    [CustomInputView(typeof(MirrorProjectSetting))]
    class MirrorProjectSettingView : InputView
    {

        string EmptyPlatform = "(None)";
        VisualElement view;
        PathField pathField;
        private MirrorProjectSetting value;
        public override VisualElement CreateView()
        {
            VisualElement view = new VisualElement();
            view.AddToClassList("mirror-project-field");
            TextField nameField = new TextField();
            nameField.AddToClassList("mirror-project-field__name");
            nameField.label = "Name";
            nameField.isDelayed = true;
            nameField.RegisterValueChangedCallback(e =>
            {
                value.name = e.newValue;
                OnValueChanged(value);
            });
            view.Add(nameField);

            var platforms = EditorSettingsUtility.SupportedNamedBuildTargets.Select(o => o.TargetName).ToList();
            platforms.Insert(0, EmptyPlatform);
            PopupField<string> platformField = new PopupField<string>(platforms, -1);
            platformField.AddToClassList("mirror-project-field__platform");
            platformField.label = "Platform";
            platformField.RegisterValueChangedCallback(e =>
            {
                string value2 = e.newValue;
                if (value2 == EmptyPlatform)
                {
                    value2 = string.Empty;
                }
                if (value2 != value.platform)
                {
                    value.platform = value2;
                    OnValueChanged(value);
                }
            });
            view.Add(platformField);

            pathField = new PathField();
            pathField.DisplayName = "Path";
            var pathView = pathField.CreateView();
            pathView.AddToClassList("mirror-project-field__path");
            pathField.ValueChanged += (newValue) =>
            {
                value.path = newValue as string;
                OnValueChanged(value);
            };
            view.Add(pathView);

            TextField argsField = new TextField();
            argsField.AddToClassList("mirror-project-field__args");
            argsField.label = "Command Line Arguments";
            argsField.tooltip = "UnityEditor Command Line Arguments";
            argsField.isDelayed = true;
            argsField.RegisterValueChangedCallback(e =>
            {
                value.arguments = e.newValue;
                OnValueChanged(value);
            });
            view.Add(argsField);

            TextField descField = new TextField();
            descField.AddToClassList("mirror-project-field__desc");
            descField.label = "Description";
            descField.isDelayed = true;
            descField.multiline = true;
            descField.RegisterValueChangedCallback(e =>
            {
                value.description = e.newValue;
                OnValueChanged(value);
            });
            view.Add(descField);

            Button button = new Button();
            button.AddToClassList("mirror-project-field__open-project");
            button.text = "Open Project";
            button.clicked += () =>
            {
                MirrorProjectUtility.OpenMirrorProject(value);
            };
            view.Add(button);

            //view.AddToClassList("unity-base-field");
            //view.AddToClassList("platform-field");

            //Label label = new Label();
            //label.AddToClassList("unity-base-field__label");
            //label.text = DisplayName;
            //view.Add(label);

            //VisualElement input = new VisualElement();
            //input.AddToClassList("unity-base-field__input");
            //Toggle anyField = new Toggle();
            //anyField.AddToClassList("platform-field__any");
            //anyField.text = "Any";
            //anyField.RegisterValueChangedCallback(e =>
            //{
            //    if (value.isAny != e.newValue)
            //    {
            //        value.isAny = e.newValue;
            //        if (e.newValue)
            //        {
            //            value.exclude = null;
            //        }
            //        else
            //        {
            //            IncludeAllPlatform();
            //        }
            //        OnValueChanged(value);
            //        SetValue(value);
            //    }
            //});
            //input.Add(anyField);
            //VisualElement platformsContainer = new VisualElement();
            //platformsContainer.AddToClassList("platform-field__platforms");

            //input.Add(platformsContainer);

            //view.Add(input);

            this.view = view;
            return view;
        }

        public override void SetValue(object newValue)
        {
            if (newValue == null)
            {
                newValue = new MirrorProjectSetting()
                {
                    platform = EditorSettingsUtility.CurrentNamedBuildTarget.TargetName,
                };
            }

            value = newValue as MirrorProjectSetting;

            if (view != null)
            {
                var nameField = view.Q<TextField>(className: "mirror-project-field__name");
                var argsField = view.Q<TextField>(className: "mirror-project-field__args");
                var descField = view.Q<TextField>(className: "mirror-project-field__desc");
                var platformField = view.Q<PopupField<string>>(className: "mirror-project-field__platform");

                nameField.SetValueWithoutNotify(value.name);
                pathField.SetValue(value.path);
                argsField.SetValueWithoutNotify(value.arguments);
                descField.SetValueWithoutNotify(value.description);
                if (string.IsNullOrEmpty(value.platform))
                {
                    platformField.SetValueWithoutNotify(EmptyPlatform);
                }
                else
                {
                    platformField.SetValueWithoutNotify(value.platform);
                }
            }
        }
    }
}