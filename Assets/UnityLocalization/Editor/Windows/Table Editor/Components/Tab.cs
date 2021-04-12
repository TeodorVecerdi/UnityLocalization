using System;
using UnityEngine.UIElements;
using UnityLocalization.Utility;

namespace UnityLocalization {
    public class Tab : VisualElement {
        public event Action Clicked;

        private string tabName;
        private readonly Label tabLabel;
        public string TabName {
            get => tabName;
            set {
                tabLabel.text = value;
                tabName = value;
            }
        }
        public Tab(string tabName, Action onClick = null) {
            this.tabName = tabName;
            if (onClick != null) Clicked += onClick;
            this.AddManipulator(new Clickable(OnClick));
            
            AddToClassList("tab");

            tabLabel = this.AddGet<Label>(null, "tab-label").Do(self => {
                self.text = tabName;
            });
        }
        
        private void OnClick() {
            Clicked?.Invoke();
        }
    }
}