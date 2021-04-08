using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityLocalization.Utility {
    public static class VisualElementFactory {
        public static T Create<T>(string name = null, params string[] classNames) where T : VisualElement, new() {
            var t = new T {name = name};
            foreach (var className in classNames) {
                t.AddToClassList(className);
            }
            return t;
        }

        public static VisualElement NthChild(this VisualElement element, int n) {
            return element[n];
        }
        public static T NthChild<T>(this VisualElement element, int n) where T : VisualElement {
            return element.NthChild(n) as T;
        }

        public static VisualElement NthParent(this VisualElement element, int n) {
            var parent = element;
            for (var i = 0; i < n; i++) {
                parent = parent.parent;
            }
            return parent;
        }
        public static T NthParent<T>(this VisualElement element, int n) where T : VisualElement {
            return element.NthParent(n) as T;
        }

        public static T Add<T, T2>(this T element, string name = null, params string[] classNames) where T : VisualElement where T2 : VisualElement, new() {
            var t2 = Create<T2>(name, classNames);
            element.Add(t2);
            return element;
        }
        public static T AddGet<T>(this VisualElement element, string name = null, params string[] classNames) where T : VisualElement, new() {
            var t = Create<T>(name, classNames);
            element.Add(t);
            return t;
        }

        public static T AddGet<T>(this VisualElement element, T other) where T : VisualElement {
            element.Add(other);
            return other;
        }

        public static T Do<T>(this T element, Action action) where T : VisualElement {
            action?.Invoke();
            return element;
        }
        
        public static T Do<T>(this T element, Action<T> action) where T : VisualElement {
            action?.Invoke(element);
            return element;
        }
        
        public static TextField TextFieldWithPlaceholder(string name, string label, string value, string placeholderText, string placeholderName) {
            return Create<TextField>(name).Do(self => {
                self.label = label;
                self.value = value;
                self.AddToClassList("textFieldWithPlaceholder");
            }).Q("unity-text-input").AddGet<Label>(placeholderName, "placeholderLabel").Do(self => {
                self.text = placeholderText;
                if(!string.IsNullOrEmpty(value)) self.AddToClassList("hidden");
            }).NthParent<TextField>(2).Do(self => {
                var placeholderLabel = self.Q<Label>(placeholderName, "placeholderLabel");
                self.RegisterValueChangedCallback(evt => {
                    if (string.IsNullOrEmpty(evt.newValue)) placeholderLabel.RemoveFromClassList("hidden");
                    else placeholderLabel.AddToClassList("hidden");
                });
            });
        }

        public static Foldout Foldout(string name, string title, string boundPropertyName, UnityEngine.Object boundObject, bool isClosed, params string[] classNames) {
            return Create<Foldout>(name, classNames).Do(self => {
                self.text = title;
                self.AddToClassList("section-foldout");
                if (!isClosed) self.AddToClassList("foldout-open");
                self.BindProperty(new SerializedObject(boundObject).FindProperty(boundPropertyName));
                self.RegisterValueChangedCallback(evt => {
                    if (evt.newValue) self.AddToClassList("foldout-open");
                    else self.RemoveFromClassList("foldout-open");
                });
            });
        }
    }
}