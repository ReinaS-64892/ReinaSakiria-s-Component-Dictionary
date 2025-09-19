#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace net.rs64.ReinaSakiriaSComponentDictionary
{
    internal class ComponentDictionary : EditorWindow
    {
        [MenuItem("Tools/ReinaSakiria's ComponentDictionary")]
        public static void ShowWindow()
        {
            GetWindow<ComponentDictionary>();
        }

        const string ComponentDictionaryDirectoryGUID = "903207793af379640b1348f07996cd1c";
        void CreateGUI()
        {
            titleContent = new GUIContent("ReinaSakiria's ComponentDictionary");

            var scroll = new ScrollView();
            scroll.viewDataKey = "CD-RootScroll";
            rootVisualElement.Add(scroll);
            var scrollBody = scroll.Q("unity-content-container");

            var dictionaryRoot = new VisualElement();
            var foldouts = new List<Foldout>();
            Action genDict = () =>
            {
                dictionaryRoot.hierarchy.Clear();
                foldouts.Clear();
                GenereateDictionary(dictionaryRoot, foldouts);
            };
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;

            var reloadButton = new Button(genDict) { text = "辞書を再読み込み" };
            reloadButton.style.flexGrow = 1f;
            header.hierarchy.Add(reloadButton);
            var foldOutContolle = new Button(() => { foreach (var f in foldouts) { f.value = false; } }) { text = "辞書をすべて畳む" };
            foldOutContolle.style.flexGrow = 1f;
            header.hierarchy.Add(foldOutContolle);

            scrollBody.hierarchy.Add(header);

            scrollBody.hierarchy.Add(dictionaryRoot);

            genDict();
        }

        private void GenereateDictionary(VisualElement root, List<Foldout> foldouts)
        {
            var dictionaryRoot = AssetDatabase.GUIDToAssetPath(ComponentDictionaryDirectoryGUID);

            var sections = Directory.EnumerateFiles(dictionaryRoot, "*.json")
                                .Select(File.ReadAllText)
                                .Select(JsonUtility.FromJson<ComponentDictionarySection>)
                                .Where(s => string.IsNullOrWhiteSpace(s.Name) is false)
                                .OrderBy(s => s.SectionOrder)
                                .Select(s => GenereateDictionarySection(s.Name, s, foldouts))
                                .ToArray();

            foreach (var s in sections)
                root.hierarchy.Add(s);
        }

        const float DefaultFontSize = 12f;
        private VisualElement GenereateDictionarySection(string path, ComponentDictionarySection dictSection, List<Foldout> foldouts)
        {
            var sectionRoot = new Foldout();
            sectionRoot.viewDataKey = path;
            sectionRoot.text = dictSection.Name;
            sectionRoot.style.fontSize = DefaultFontSize * 4;
            foldouts.Add(sectionRoot);

            var container = sectionRoot.Q("unity-content");
            container.style.fontSize = DefaultFontSize;

            var description = new Foldout();
            sectionRoot.viewDataKey = path + "/Description";
            description.text = "詳細情報";
            description.style.fontSize = DefaultFontSize * 2;
            container.hierarchy.Add(description);
            foldouts.Add(description);

            var descriptionContent = description.Q("unity-content");
            descriptionContent.style.fontSize = DefaultFontSize;
            var urls = new VisualElement();
            urls.style.flexDirection = FlexDirection.Row;
            descriptionContent.hierarchy.Add(urls);

            if (string.IsNullOrWhiteSpace(dictSection.HomePageURL) is false)
                urls.hierarchy.Add(CretateURLButton("ホームページ", dictSection.HomePageURL));

            if (string.IsNullOrWhiteSpace(dictSection.SourceCodeURL) is false)
                urls.hierarchy.Add(CretateURLButton("ソースコード", dictSection.SourceCodeURL));

            descriptionContent.Add(new TextElement() { text = string.Join("\n", dictSection.Description) });

            foreach (var e in dictSection.DictionaryElements)
            {
                GenereateDictionaryElement(path + $"/{e.Name}", container, e, foldouts);
            }
            return sectionRoot;
        }

        private void GenereateDictionaryElement(string path, VisualElement container, ComponentDictionaryElement e, List<Foldout> foldouts)
        {

            var foldout = new Foldout();
            foldout.viewDataKey = path;
            container.hierarchy.Add(foldout);
            foldouts.Add(foldout);

            var componentInfo = GetComponentsInfo(e.ComponentGUID, e.ComponentFullName);

            var foldoutHeaderContainer = new VisualElement();
            foldout.Q("unity-checkmark").parent.hierarchy.Add(foldoutHeaderContainer);

            foldoutHeaderContainer.style.flexDirection = FlexDirection.Row;
            foldoutHeaderContainer.style.flexGrow = 1f;

            if (componentInfo?.ScriptIcon is not null)
            {
                var image = new Image() { image = componentInfo.ScriptIcon };
                foldoutHeaderContainer.hierarchy.Add(image);
                image.style.height = image.style.width = DefaultFontSize * 2;
                image.style.paddingTop = Length.Auto();
                image.style.paddingBottom = Length.Auto();
            }

            var componentName = new TextElement() { text = e.Name };
            componentName.style.fontSize = DefaultFontSize * 2;
            foldoutHeaderContainer.hierarchy.Add(componentName);

            var padding = new VisualElement();
            padding.style.flexGrow = 1f;
            foldoutHeaderContainer.hierarchy.Add(padding);

            var buttonSize = DefaultFontSize * 1.5f;

            if (componentInfo?.ComponentType is not null)
            {
                var button = new Button();
                button.style.fontSize = buttonSize;
                if (e.CreateGameObject is false)
                {
                    button.clicked += () => Selection.activeGameObject?.AddComponent(componentInfo.ComponentType);
                    button.text = " コンポーネントを追加 ";
                }
                else
                {
                    button.clicked += () => new GameObject(e.Name, componentInfo.ComponentType).transform.SetParent(Selection.activeTransform, false);
                    button.text = " コンポーネントを生成 ";
                }
                foldoutHeaderContainer.hierarchy.Add(button);
            }
            var foldoutContainer = foldout.Q("unity-content");

            var urlContainer = new VisualElement();
            urlContainer.style.flexDirection = FlexDirection.Row;
            foldoutContainer.hierarchy.Add(urlContainer);

            if (string.IsNullOrWhiteSpace(e.TutorialURL) is false)
                urlContainer.hierarchy.Add(CretateURLButton("使い方", e.TutorialURL));
            if (string.IsNullOrWhiteSpace(e.ReferenceURL) is false)
                urlContainer.hierarchy.Add(CretateURLButton("リファレンス", e.ReferenceURL));


            var note = new TextElement() { text = string.Join("\n", e.Description) };
            note.style.marginLeft = DefaultFontSize;
            foldoutContainer.hierarchy.Add(note);

            if (e is MoreNestComponentDictionaryElement mne)
                foreach (var ce in mne.ChildElements)
                    GenereateDictionaryElement(path + $"/{ce.Name}", foldoutContainer, ce, foldouts);
            else if (e is NestComponentDictionaryElement ne)
                foreach (var ce in ne.ChildElements)
                    GenereateDictionaryElement(path + $"/{ce.Name}", foldoutContainer, ce, foldouts);
        }

        private static Button CretateURLButton(string buttonText, string url)
        {
            var urlButton = new Button(() => Application.OpenURL(url)) { text = buttonText };
            urlButton.style.flexGrow = 1;
            return urlButton;
        }

        private ComponentInfo? GetComponentsInfo(string guid, string componentFullName)
        {
            if (string.IsNullOrWhiteSpace(guid) && string.IsNullOrWhiteSpace(componentFullName)) { return null; }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var componentType = Type.GetType(componentFullName);
            if (string.IsNullOrWhiteSpace(path) && componentType is null) { return null; }

            if (string.IsNullOrWhiteSpace(path) is false)
            {
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var type = monoScript.GetClass();
                var icon = AssetPreview.GetMiniThumbnail(monoScript);
                return new(icon, type);
            }
            else if (componentType is not null)
            {
                var type = componentType;
                var icon = AssetPreview.GetMiniTypeThumbnail(type);
                return new(icon, type);
            }
            return null;
        }
        internal class ComponentInfo
        {
            public Texture2D? ScriptIcon;
            public Type ComponentType;

            public ComponentInfo(Texture2D? scriptIcon, Type componentType)
            {
                ScriptIcon = scriptIcon;
                ComponentType = componentType;
            }
        }

    }
}
