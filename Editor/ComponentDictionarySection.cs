#nullable enable
using System;

namespace net.rs64.ReinaSakiriaSComponentDictionary
{
    [Serializable]
    internal class ComponentDictionarySection
    {
        public string Name = "";

        public string PackageID = "";
        public string HomePageURL = "";
        public string SourceCodeURL = "";

        public int SectionOrder = 99;
        public string[] Description = new string[0];
        public MoreNestComponentDictionaryElement[] DictionaryElements = new MoreNestComponentDictionaryElement[0];
    }
    [Serializable]
    internal class ComponentDictionaryElement
    {
        public string Name = "";

        public string TutorialURL = "";
        public string ReferenceURL = "";

        public string ComponentGUID = "";
        public string ComponentFullName = "";
        public bool CreateGameObject = false;


        public string[] Description = new string[0];
    }
    [Serializable]
    internal class NestComponentDictionaryElement : ComponentDictionaryElement
    {
        public ComponentDictionaryElement[] ChildElements = new ComponentDictionaryElement[0];
    }
    [Serializable]
    internal class MoreNestComponentDictionaryElement : ComponentDictionaryElement
    {
        public NestComponentDictionaryElement[] ChildElements = new NestComponentDictionaryElement[0];
    }
}
