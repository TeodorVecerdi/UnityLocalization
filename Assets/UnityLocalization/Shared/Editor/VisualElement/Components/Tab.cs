using System;
using UnityEngine.UIElements;

namespace UnityLocalization.Shared {
    public class Tab : VisualElement {
        public event Action Clicked;
        public event Action DeleteClicked;

        private string tabName;
        private readonly Label tabLabel;
        private readonly Button deleteButton;
        public string TabName {
            get => tabName;
            set {
                tabLabel.text = value;
                tabName = value;
            }
        }
        public Tab(string tabName, Action onClick = null, Action onDeleteClicked = null) {
            this.tabName = tabName;
            if (onClick != null) Clicked += onClick;
            if (onDeleteClicked != null) DeleteClicked += onDeleteClicked;
            
            this.AddManipulator(new Clickable(OnClick));
            
            AddToClassList("tab");

            tabLabel = this.AddGet<Label>(null, "tab-label").Do(self => {
                self.text = tabName;
            });
            deleteButton = this.AddGet<Button>(null, "tab-delete-button").Do(self => {
                self.clicked += OnDelete;
            });
        }
        
        private void OnClick() {
            Clicked?.Invoke();
        }

        private void OnDelete() {
            DeleteClicked?.Invoke();
        }
    }
}