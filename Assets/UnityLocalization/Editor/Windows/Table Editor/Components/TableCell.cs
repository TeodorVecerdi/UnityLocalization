using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EventSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UnityLocalization {
    public class TableCell : VisualElement {
        public string Text {
            get => text;
            set {
                text = value;
                label.text = text;
            }
        }

        public event Action OnNextCellSelected;
        public event Action<string> OnValueChanged;
        public event Action<TextField> OnBeginEdit;
        public event Action<TextField> OnCancelEdit;

        private readonly Label label;
        private readonly TextField editField;
        private string text;
        private bool isEditing;

        public TableCell() : this(null, false) {
        }

        public TableCell(string text, bool editable) {
            this.text = text;
            AddToClassList("table-cell");

            label = new Label(text);
            label.AddToClassList("table-cell-label");
            Add(label);

            if (editable) {
                this.AddManipulator(new DoubleClickable(OnStartEdit));
                editField = new TextField();
                MakeEditField();
                Add(editField);
            }
        }

        public void BeginEdit() {
            OnStartEdit();
        }

        private void OnFinishEdit(string value) {
            isEditing = false;
            var oldValue = text;
            Text = value;
            editField.AddToClassList("hidden");
            label.RemoveFromClassList("hidden");
            if(!string.Equals(oldValue, value, StringComparison.InvariantCulture))
                OnValueChanged?.Invoke(value);
        }

        private void MakeEditField() {
            editField.AddToClassList("table-cell-edit-field");
            editField.AddToClassList("hidden");
            editField.RegisterCallback(new EventCallback<KeyDownEvent>(OnKeyDown));
            editField.RegisterCallback(new EventCallback<BlurEvent>(OnBlur));
        }

        private void OnStartEdit() {
            if (isEditing) return;
            isEditing = true;
            editField.value = text;
            label.AddToClassList("hidden");
            editField.RemoveFromClassList("hidden");
            schedule.Execute(() => editField[0].Focus());
            OnBeginEdit?.Invoke(editField);
        }
        
        private void OnBlur(BlurEvent evt) {
            var newValue = editField.value;
            OnFinishEdit(newValue);
        }

        private void OnKeyDown(KeyDownEvent evt) {
            if (evt.keyCode == KeyCode.Tab) {
                OnFinishEdit(editField.value);
                OnNextCellSelected?.Invoke();
            } else if (evt.keyCode == KeyCode.Return)
                OnFinishEdit(editField.value);
            else if (evt.keyCode == KeyCode.Escape) {
                OnFinishEdit(Text);
                OnCancelEdit?.Invoke(editField);
            }
        }
    }
}