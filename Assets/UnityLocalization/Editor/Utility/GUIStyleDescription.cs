using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

namespace UnityLocalization.Utility {
    public class GUIStyleDescription {
        public TextAnchor? Alignment;
        public Int4? Border;
        public Int4? Margin;
        public Int4? Padding;
        public Int4? Overflow;
        public TextClipping? Clipping;
        public Font? Font;
        public float? FixedWidth;
        public float? FixedHeight;
        public Vector2? ContentOffset;
        public int? FontSize;
        public FontStyle? FontStyle;
        public ImagePosition? ImagePosition;
        public bool? RichText;
        public bool? StretchWidth;
        public bool? StretchHeight;
        public bool? WordWrap;
        public GUIStyleState? Normal;
        public GUIStyleState? Active;
        public GUIStyleState? Focused;
        public GUIStyleState? Hover;
        public GUIStyleState? OnNormal;
        public GUIStyleState? OnActive;
        public GUIStyleState? OnFocused;
        public GUIStyleState? OnHover;
        public Color? TextColor;

        public GUIStyleDescription() {
        }

        public GUIStyleDescription(GUIStyleDescription other) {
            Alignment = other.Alignment;
            Border = other.Border;
            Margin = other.Margin;
            Padding = other.Padding;
            Overflow = other.Overflow;
            Clipping = other.Clipping;
            Font = other.Font;
            FixedWidth = other.FixedWidth;
            FixedHeight = other.FixedHeight;
            ContentOffset = other.ContentOffset;
            FontSize = other.FontSize;
            FontStyle = other.FontStyle;
            ImagePosition = other.ImagePosition;
            RichText = other.RichText;
            StretchWidth = other.StretchWidth;
            StretchHeight = other.StretchHeight;
            WordWrap = other.WordWrap;
            TextColor = other.TextColor;
            Normal = other.Normal != null
                ? new GUIStyleState {background = other.Normal.background, scaledBackgrounds = other.Normal.scaledBackgrounds.ToArray(), textColor = other.Normal.textColor}
                : null;
            Active = other.Active != null
                ? new GUIStyleState {background = other.Active.background, scaledBackgrounds = other.Active.scaledBackgrounds.ToArray(), textColor = other.Active.textColor}
                : null;
            Focused = other.Focused != null
                ? new GUIStyleState {background = other.Focused.background, scaledBackgrounds = other.Focused.scaledBackgrounds.ToArray(), textColor = other.Focused.textColor}
                : null;
            Hover = other.Hover != null
                ? new GUIStyleState {background = other.Hover.background, scaledBackgrounds = other.Hover.scaledBackgrounds.ToArray(), textColor = other.Hover.textColor}
                : null;
            
            OnNormal = other.OnNormal != null
                ? new GUIStyleState {background = other.OnNormal.background, scaledBackgrounds = other.OnNormal.scaledBackgrounds.ToArray(), textColor = other.OnNormal.textColor}
                : null;
            OnActive = other.OnActive != null
                ? new GUIStyleState {background = other.OnActive.background, scaledBackgrounds = other.OnActive.scaledBackgrounds.ToArray(), textColor = other.OnActive.textColor}
                : null;
            OnFocused = other.OnFocused != null
                ? new GUIStyleState {background = other.OnFocused.background, scaledBackgrounds = other.OnFocused.scaledBackgrounds.ToArray(), textColor = other.OnFocused.textColor}
                : null;
            OnHover = other.OnHover != null
                ? new GUIStyleState {background = other.OnHover.background, scaledBackgrounds = other.OnHover.scaledBackgrounds.ToArray(), textColor = other.OnHover.textColor}
                : null;
        }

        public GUIStyle ToStyle() {
            var style = new GUIStyle(GUIStyle.none);
            style.alignment = Alignment ?? style.alignment;
            style.border = Border ?? style.border;
            style.margin = Margin ?? style.margin;
            style.padding = Padding ?? style.padding;
            style.overflow = Overflow ?? style.overflow;
            style.clipping = Clipping ?? style.clipping;
            style.font = Font != null ? Font : style.font;
            style.fixedWidth = FixedWidth ?? style.fixedWidth;
            style.fixedHeight = FixedHeight ?? style.fixedHeight;
            style.contentOffset = ContentOffset ?? style.contentOffset;
            style.fontSize = FontSize ?? style.fontSize;
            style.fontStyle = FontStyle ?? style.fontStyle;
            style.imagePosition = ImagePosition ?? style.imagePosition;
            style.richText = RichText ?? style.richText;
            style.stretchWidth = StretchWidth ?? style.stretchWidth;
            style.stretchHeight = StretchHeight ?? style.stretchHeight;
            style.wordWrap = WordWrap ?? style.wordWrap;
            style.normal = Normal ?? style.normal;
            style.active = Active ?? style.active;
            style.focused = Focused ?? style.focused;
            style.hover = Hover ?? style.hover;
            style.hover.textColor = TextColor ?? style.hover.textColor;
            style.active.textColor = TextColor ?? style.active.textColor;
            style.normal.textColor = TextColor ?? style.normal.textColor;
            style.focused.textColor = TextColor ?? style.focused.textColor;
            style.onNormal = OnNormal ?? style.onNormal;
            style.onActive = OnActive ?? style.onActive;
            style.onFocused = OnFocused ?? style.onFocused;
            style.onHover = OnHover ?? style.onHover;
            style.onHover.textColor = TextColor ?? style.onHover.textColor;
            style.onActive.textColor = TextColor ?? style.onActive.textColor;
            style.onNormal.textColor = TextColor ?? style.onNormal.textColor;
            style.onFocused.textColor = TextColor ?? style.onFocused.textColor;
            return style;
        }

        public static implicit operator GUIStyleCombine(GUIStyleDescription description) {
            return new GUIStyleCombine(description, false);
        }

        public static GUIStyleCombine operator !(GUIStyleDescription description) {
            return new GUIStyleCombine(description, true);
        }

        public static GUIStyleDescription CombineDesc(params GUIStyleCombine[] combines) {
            Assert.IsTrue(combines.Length > 0);
            var style = combines[0];
            for (var i = 1; i < combines.Length; i++) {
                var comb = combines[i];
                style = ApplyStyles(style, comb);
            }

            return style.Description;
        }
        public static GUIStyle Combine(params GUIStyleCombine[] combines) {
            return CombineDesc(combines).ToStyle();
        }

        private static GUIStyleCombine ApplyStyles(GUIStyleCombine target, GUIStyleCombine other) {
            var result = new GUIStyleCombine(new GUIStyleDescription(target.Description), target.Important);
            if (target.Description.Alignment == null) result.Description.Alignment = other.Description.Alignment;
            else if (other.Important && other.Description.Alignment != null) result.Description.Alignment = other.Description.Alignment;
            result.Description.Border = Int4.Combine(target.Description.Border, other.Description.Border, other.Important);
            result.Description.Margin = Int4.Combine(target.Description.Margin, other.Description.Margin, other.Important);
            result.Description.Padding = Int4.Combine(target.Description.Padding, other.Description.Padding, other.Important);
            result.Description.Overflow = Int4.Combine(target.Description.Overflow, other.Description.Overflow, other.Important);
            if (target.Description.Clipping == null) result.Description.Clipping = other.Description.Clipping;
            else if (other.Important && other.Description.Clipping != null) result.Description.Clipping = other.Description.Clipping;
            if (target.Description.Font == null) result.Description.Font = other.Description.Font;
            else if (other.Important && other.Description.Font != null) result.Description.Font = other.Description.Font;
            if (target.Description.FixedWidth == null) result.Description.FixedWidth = other.Description.FixedWidth;
            else if (other.Important && other.Description.FixedWidth != null) result.Description.FixedWidth = other.Description.FixedWidth;
            if (target.Description.FixedHeight == null) result.Description.FixedHeight = other.Description.FixedHeight;
            else if (other.Important && other.Description.FixedHeight != null) result.Description.FixedHeight = other.Description.FixedHeight;
            if (target.Description.ContentOffset == null) result.Description.ContentOffset = other.Description.ContentOffset;
            else if (other.Important && other.Description.ContentOffset != null) result.Description.ContentOffset = other.Description.ContentOffset;
            if (target.Description.FontSize == null) result.Description.FontSize = other.Description.FontSize;
            else if (other.Important && other.Description.FontSize != null) result.Description.FontSize = other.Description.FontSize;
            if (target.Description.FontStyle == null) result.Description.FontStyle = other.Description.FontStyle;
            else if (other.Important && other.Description.FontStyle != null) result.Description.FontStyle = other.Description.FontStyle;
            if (target.Description.ImagePosition == null) result.Description.ImagePosition = other.Description.ImagePosition;
            else if (other.Important && other.Description.ImagePosition != null) result.Description.ImagePosition = other.Description.ImagePosition;
            if (target.Description.RichText == null) result.Description.RichText = other.Description.RichText;
            else if (other.Important && other.Description.RichText != null) result.Description.RichText = other.Description.RichText;
            if (target.Description.StretchWidth == null) result.Description.StretchWidth = other.Description.StretchWidth;
            else if (other.Important && other.Description.StretchWidth != null) result.Description.StretchWidth = other.Description.StretchWidth;
            if (target.Description.StretchHeight == null) result.Description.StretchHeight = other.Description.StretchHeight;
            else if (other.Important && other.Description.StretchHeight != null) result.Description.StretchHeight = other.Description.StretchHeight;
            if (target.Description.WordWrap == null) result.Description.WordWrap = other.Description.WordWrap;
            else if (other.Important && other.Description.WordWrap != null) result.Description.WordWrap = other.Description.WordWrap;
            if (target.Description.Normal == null) result.Description.Normal = other.Description.Normal;
            else if (other.Important && other.Description.Normal != null) result.Description.Normal = other.Description.Normal;
            if (target.Description.Active == null) result.Description.Active = other.Description.Active;
            else if (other.Important && other.Description.Active != null) result.Description.Active = other.Description.Active;
            if (target.Description.Focused == null) result.Description.Focused = other.Description.Focused;
            else if (other.Important && other.Description.Focused != null) result.Description.Focused = other.Description.Focused;
            if (target.Description.Hover == null) result.Description.Hover = other.Description.Hover;
            else if (other.Important && other.Description.Hover != null) result.Description.Hover = other.Description.Hover;
            if (target.Description.OnNormal == null) result.Description.OnNormal = other.Description.OnNormal;
            else if (other.Important && other.Description.OnNormal != null) result.Description.OnNormal = other.Description.OnNormal;
            if (target.Description.OnActive == null) result.Description.OnActive = other.Description.OnActive;
            else if (other.Important && other.Description.OnActive != null) result.Description.OnActive = other.Description.OnActive;
            if (target.Description.OnFocused == null) result.Description.OnFocused = other.Description.OnFocused;
            else if (other.Important && other.Description.OnFocused != null) result.Description.OnFocused = other.Description.OnFocused;
            if (target.Description.OnHover == null) result.Description.OnHover = other.Description.OnHover;
            else if (other.Important && other.Description.OnHover != null) result.Description.OnHover = other.Description.OnHover;
            if (target.Description.TextColor == null) result.Description.TextColor = other.Description.TextColor;
            else if (other.Important && other.Description.TextColor != null) result.Description.TextColor = other.Description.TextColor;
            return result;
        }
    }

    public class GUIStyleCombine {
        public readonly GUIStyleDescription Description;
        public readonly bool Important;

        internal GUIStyleCombine([NotNull] GUIStyleDescription description, bool important) {
            Description = description;
            Important = important;
        }
    }

    public class Int4 {
        public int? left;
        public int? right;
        public int? top;
        public int? bottom;

        public static implicit operator RectOffset(Int4 int4) {
            return new RectOffset(int4.left ?? 0, int4.right ?? 0, int4.top ?? 0, int4.bottom ?? 0);
        }

        public static Int4? Combine(Int4? left, Int4? right, bool important) {
            if (left == null) {
                return right ?? null;
            }
            if (right == null) return left;
            
            var result = new Int4 {left = left.left, right = left.right, top = left.top, bottom = left.bottom};
            
            if (left.left == null) result.left = right.left; else if (important && right.left != null) result.left = right.left;
            if (left.right == null) result.right = right.right; else if (important && right.right != null) result.right = right.right;
            if (left.top == null) result.top = right.top; else if (important && right.top != null) result.top = right.top;
            if (left.bottom == null) result.bottom = right.bottom; else if (important && right.bottom != null) result.bottom = right.bottom;
            return result;
        }
    }
}