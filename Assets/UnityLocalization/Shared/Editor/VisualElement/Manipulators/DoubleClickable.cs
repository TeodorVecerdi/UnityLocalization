using System;
using UnityEngine.UIElements;

namespace UnityLocalization {
    public class DoubleClickable : Clickable {
        public DoubleClickable(Action handler) : base(handler) {
            activators.Clear();
            activators.Add(new ManipulatorActivationFilter {
                button = MouseButton.LeftMouse,
                clickCount = 2
            });
        }
        
        public DoubleClickable(Action<EventBase> handler) : base(handler) {
            activators.Clear();
            activators.Add(new ManipulatorActivationFilter {
                button = MouseButton.LeftMouse,
                clickCount = 2
            });
        }
    }
}