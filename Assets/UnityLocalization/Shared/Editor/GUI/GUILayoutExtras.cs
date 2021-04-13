using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityLocalization.Shared {
    public static class GUILayoutExtras {
        public static T Selection<T>(T selected, List<T> elements, Func<T, bool, int, bool> drawAndClick) where T : IEquatable<T> {
            for (var i = 0; i < elements.Count; i++) {
                if (drawAndClick(elements[i], elements[i].Equals(selected), i)) return elements[i];
            }

            return selected;
        }
    }
}