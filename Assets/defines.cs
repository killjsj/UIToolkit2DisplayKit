// ============================================================================
// DisplayKit Style Properties - Direct use of UnityEngine.UIElements types.
// No custom wrapper classes — Unity's Style* structs are used directly.
// CSS parsing and IStyle conversion are provided as static utilities.
// ============================================================================

using DisplayKit.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Align = UnityEngine.UIElements.Align;
using DisplayStyle = UnityEngine.UIElements.DisplayStyle;
// Enum aliases — all enums now come from Unity except FontStyle/FontType/CanvasVisibility
using FlexDirection = UnityEngine.UIElements.FlexDirection;
using FontStyle = DisplayKit.Enums.FontStyle;
using Justify = UnityEngine.UIElements.Justify;
using Length = UnityEngine.UIElements.Length;
using LengthUnit = UnityEngine.UIElements.LengthUnit;
using Overflow = UnityEngine.UIElements.Overflow;
using Position = UnityEngine.UIElements.Position;
using Rotate = UnityEngine.UIElements.Rotate;
using Scale = UnityEngine.UIElements.Scale;
using StyleColor = UnityEngine.UIElements.StyleColor;
// Unity's Style* struct aliases — replace our old wrapper classes
using StyleFloat = UnityEngine.UIElements.StyleFloat;
// Primitive type aliases
using StyleKeyword = UnityEngine.UIElements.StyleKeyword;
using StyleLength = UnityEngine.UIElements.StyleLength;
using StyleRotate = UnityEngine.UIElements.StyleRotate;
using StyleScale = UnityEngine.UIElements.StyleScale;
using StyleTextShadow = UnityEngine.UIElements.StyleTextShadow;
using StyleTransformOrigin = UnityEngine.UIElements.StyleTransformOrigin;
using StyleTranslate = UnityEngine.UIElements.StyleTranslate;
using TextOverflow = UnityEngine.UIElements.TextOverflow;
using TextShadow = UnityEngine.UIElements.TextShadow;
using TransformOrigin = UnityEngine.UIElements.TransformOrigin;
using Translate = UnityEngine.UIElements.Translate;
using UIElements = UnityEngine.UIElements;
using Visibility = UnityEngine.UIElements.Visibility;
using WhiteSpace = UnityEngine.UIElements.WhiteSpace;
using Wrap = UnityEngine.UIElements.Wrap;

// ============================================================================
// CSS Parsing Helpers (internal to StyleParser)
// ============================================================================

namespace DisplayKit
{
    /// <summary>Internal CSS value parsing utilities.</summary>
    internal static class CssParse
    {
        public static string StripSuffix(string s, string suffix) =>
            s.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? s.Substring(0, s.Length - suffix.Length) : s;

        public static bool TryParseKeyword(string raw, out StyleKeyword keyword)
        {
            keyword = StyleKeyword.Undefined;
            if (string.IsNullOrEmpty(raw)) return false;
            switch (raw.Trim().ToLowerInvariant())
            {
                case "initial": keyword = StyleKeyword.Initial; return true;
                case "auto": keyword = StyleKeyword.Auto; return true;
                case "none": keyword = StyleKeyword.None; return true;
                case "null": keyword = StyleKeyword.Null; return true;
                default: return false;
            }
        }

        public static StyleFloat ParseFloat(string raw)
        {
            if (TryParseKeyword(raw, out var kw)) return new StyleFloat(kw);
            string num = StripSuffix(raw.Trim(), "px");
            return float.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out float v)
                ? new StyleFloat(v) : new StyleFloat(0f);
        }

        public static StyleColor ParseColor(string raw)
        {
            if (TryParseKeyword(raw, out var kw)) return new StyleColor(kw);
            var t = raw.Trim();
            if (TryNamedColor(t, out Color nc)) return nc;
            var rm = RgbRegex.Match(t);
            if (rm.Success)
            {
                float r = int.Parse(rm.Groups[1].Value) / 255f;
                float g = int.Parse(rm.Groups[2].Value) / 255f;
                float b = int.Parse(rm.Groups[3].Value) / 255f;
                float a = rm.Groups[4].Success ? float.Parse(rm.Groups[4].Value, CultureInfo.InvariantCulture) : 1f;
                return new Color(r, g, b, a);
            }
            var hm = HexRegex.Match(t);
            if (hm.Success) return HexToColor(hm.Groups[1].Value);
            return new StyleColor(StyleKeyword.None);
        }

        private static readonly Regex RgbRegex = new Regex(
            @"rgba?\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+))?\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HexRegex = new Regex(
            @"^#([0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);

        private static bool TryNamedColor(string name, out Color c)
        {
            switch (name.ToLowerInvariant())
            {
                case "red":
                case "green":
                case "blue":
                case "white":
                case "black":
                case "yellow":
                case "cyan":
                case "magenta":
                case "gray":
                case "grey":
                case "clear":
                case "transparent":
                    c = name.ToLowerInvariant() switch
                    {
                        "red" => Color.red,
                        "green" => Color.green,
                        "blue" => Color.blue,
                        "white" => Color.white,
                        "black" => Color.black,
                        "yellow" => Color.yellow,
                        "cyan" => Color.cyan,
                        "magenta" => Color.magenta,
                        "gray" or "grey" => Color.gray,
                        "clear" or "transparent" => Color.clear,
                        _ => default
                    };
                    return true;
                default: c = default; return false;
            }
        }

        private static Color HexToColor(string hex)
        {
            int r, g, b, a = 255;
            switch (hex.Length)
            {
                case 3: r = Hd(hex[0]) * 17; g = Hd(hex[1]) * 17; b = Hd(hex[2]) * 17; break;
                case 4: r = Hd(hex[0]) * 17; g = Hd(hex[1]) * 17; b = Hd(hex[2]) * 17; a = Hd(hex[3]) * 17; break;
                case 6: r = Hd(hex[0]) << 4 | Hd(hex[1]); g = Hd(hex[2]) << 4 | Hd(hex[3]); b = Hd(hex[4]) << 4 | Hd(hex[5]); break;
                case 8: r = Hd(hex[0]) << 4 | Hd(hex[1]); g = Hd(hex[2]) << 4 | Hd(hex[3]); b = Hd(hex[4]) << 4 | Hd(hex[5]); a = Hd(hex[6]) << 4 | Hd(hex[7]); break;
                default: return Color.clear;
            }
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        private static int Hd(char c) => c >= '0' && c <= '9' ? c - '0' : c >= 'a' && c <= 'f' ? c - 'a' + 10 : c >= 'A' && c <= 'F' ? c - 'A' + 10 : 0;

        public static StyleLength ParseLength(string raw)
        {
            if (TryParseKeyword(raw, out var kw)) return new StyleLength(kw);
            var t = raw.Trim();
            if (t.EndsWith("%") && float.TryParse(t.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float pct))
                return new Length(pct, LengthUnit.Percent);
            string n = t.EndsWith("px", StringComparison.OrdinalIgnoreCase) ? t.Substring(0, t.Length - 2) : t;
            return float.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out float px)
                ? new Length(px, LengthUnit.Pixel) : new StyleLength(StyleKeyword.None);
        }

        public static Length ParseLengthRaw(string s)
        {
            s = s.Trim();
            if (s.EndsWith("%") && float.TryParse(s.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                return new Length(v, LengthUnit.Percent);
            string num = StripSuffix(s, "px");
            float.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out float pv);
            return new Length(pv, LengthUnit.Pixel);
        }

        public static StyleScale ParseScale(string raw)
        {
            if (TryParseKeyword(raw, out var kw)) return new StyleScale(kw);
            var parts = raw.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            var nums = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                string p = StripSuffix(StripSuffix(StripSuffix(parts[i].Trim(), "px"), "%"), "deg");
                if (!float.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out nums[i]))
                    return new StyleScale(StyleKeyword.None);
            }
            if (nums.Length == 0) return new StyleScale(StyleKeyword.None);
            float x = nums[0], y = nums.Length > 1 ? nums[1] : x, z = nums.Length > 2 ? nums[2] : 1f;
            return new Scale(new Vector3(x, y, z));
        }

        public static StyleRotate ParseRotate(string raw)
        {
            if (TryParseKeyword(raw, out var kw)) return new StyleRotate(kw);
            var parts = raw.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return new StyleRotate(StyleKeyword.None);
            string angleStr = parts[parts.Length - 1].Trim();
            if (TryParseAngle(angleStr, out float deg)) return new Rotate(deg);
            return new StyleRotate(StyleKeyword.None);
        }

        private static bool TryParseAngle(string s, out float degrees)
        {
            degrees = 0f;
            if (string.IsNullOrEmpty(s)) return false;
            string lower = s.ToLowerInvariant();
            float factor; string numStr;
            if (lower.EndsWith("grad")) { factor = 0.9f; numStr = lower.Substring(0, lower.Length - 4); }
            else if (lower.EndsWith("turn")) { factor = 360f; numStr = lower.Substring(0, lower.Length - 4); }
            else if (lower.EndsWith("rad")) { factor = 180f / Mathf.PI; numStr = lower.Substring(0, lower.Length - 3); }
            else if (lower.EndsWith("deg")) { factor = 1f; numStr = lower.Substring(0, lower.Length - 3); }
            else { factor = 1f; numStr = lower; }
            if (float.TryParse(numStr.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            { degrees = val * factor; return true; }
            return false;
        }

        public static StyleTranslate ParseTranslate(string raw)
        {
            if (TryParseKeyword(raw, out var kw)) return new StyleTranslate(kw);
            var parts = raw.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return new StyleTranslate(StyleKeyword.None);
            Length x = ParseLengthRaw(parts[0]), y = ParseLengthRaw(parts[1]);
            float z = 0f;
            if (parts.Length >= 3) { string nz = StripSuffix(parts[2].Trim(), "px"); float.TryParse(nz, NumberStyles.Float, CultureInfo.InvariantCulture, out z); }
            return new Translate(x, y, z);
        }

        public static StyleTransformOrigin ParseTransformOrigin(string raw)
        {
            if (TryParseKeyword(raw, out var kw)) return new StyleTransformOrigin(kw);
            var parts = raw.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            float x = ParseOriginPart(parts, 0, 50f), y = ParseOriginPart(parts, 1, 50f), z = 0f;
            if (parts.Length == 1 && TryOriginKeyword(parts[0].Trim(), out float kv, out bool isVert))
            { if (isVert) { x = 50f; y = kv; } else { x = kv; y = 50f; } }
            if (parts.Length >= 3) { string nz = StripSuffix(StripSuffix(parts[2].Trim(), "%"), "px"); float.TryParse(nz, NumberStyles.Float, CultureInfo.InvariantCulture, out z); }
            return new TransformOrigin(x, y, z);
        }

        private static float ParseOriginPart(string[] parts, int idx, float fb)
        {
            if (idx >= parts.Length) return fb;
            string s = parts[idx].Trim();
            if (TryOriginKeyword(s, out float kv, out _)) return kv;
            string n = StripSuffix(StripSuffix(s, "%"), "px");
            return float.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out float fv) ? fv : fb;
        }

        private static bool TryOriginKeyword(string s, out float v, out bool isVert)
        {
            switch (s.Trim().ToLowerInvariant())
            {
                case "top": v = 0f; isVert = true; return true;
                case "bottom": v = 100f; isVert = true; return true;
                case "left": v = 0f; isVert = false; return true;
                case "right": v = 100f; isVert = false; return true;
                case "center": v = 50f; isVert = false; return true;
                default: v = 0f; isVert = false; return false;
            }
        }

        public static StyleTextShadow ParseTextShadow(string raw)
        {
            if (TryParseKeyword(raw, out var kw)) return new StyleTextShadow(kw);
            var parts = raw.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return new StyleTextShadow(StyleKeyword.None);
            Color color = Color.black; Vector2 offset = Vector2.zero; float blur = 0f;
            int ni = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                string p = parts[i];
                if (i == parts.Length - 1 || (i >= 2 && !IsNumPart(p)))
                {
                    var sc = ParseColor(string.Join(" ", parts, i, parts.Length - i));
                    if (sc.keyword == StyleKeyword.Undefined) color = sc.value;
                    break;
                }
                string ns = p.EndsWith("px", StringComparison.OrdinalIgnoreCase) ? p.Substring(0, p.Length - 2) : p;
                if (float.TryParse(ns, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                {
                    switch (ni) { case 0: offset.x = f; break; case 1: offset.y = f; break; case 2: blur = f; break; }
                    ni++;
                }
            }
            return new TextShadow { color = color, offset = offset, blurRadius = blur };
        }

        private static bool IsNumPart(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            string n = s.EndsWith("px", StringComparison.OrdinalIgnoreCase) ? s.Substring(0, s.Length - 2) : s;
            return float.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        public static T ParseEnum<T>(string raw) where T : struct, System.Enum
        {
            if (TryParseKeyword(raw, out var kw) && kw != StyleKeyword.None)
                return default; // caller handles keyword separately for StyleEnum
            if (!string.IsNullOrEmpty(raw))
            {
                var norm = raw.Trim().Replace("-", "");
                if (System.Enum.TryParse(norm, true, out T val)) return val;
            }
            return default;
        }

        public static FontType? ParseFontDef(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            string fontName = null;
            int hash = raw.LastIndexOf('#');
            if (hash >= 0)
            {
                int end = raw.IndexOf(')', hash);
                fontName = end > hash ? raw.Substring(hash + 1, end - hash - 1).Trim() : raw.Substring(hash + 1).Trim();
            }
            else
            {
                int ls = raw.LastIndexOf('/');
                int q = raw.IndexOf('?', ls > 0 ? ls : 0);
                if (ls >= 0) fontName = (q > ls ? raw.Substring(ls + 1, q - ls - 1) : raw.Substring(ls + 1)).Trim();
            }
            if (!string.IsNullOrEmpty(fontName))
            {
                fontName = StripSuffix(StripSuffix(fontName, ".ttf"), ".otf").Trim('"', '\'', ' ', ')');
                if (System.Enum.TryParse(fontName, true, out FontType ft)) return ft;
            }
            return null;
        }
    }
}

// ============================================================================
// Enums (only custom types not present in Unity)
// ============================================================================

namespace DisplayKit.Enums
{
    /// <summary>Font style. Kept because Unity uses "BoldAndItalic" vs CSS "BoldItalic".</summary>
    public enum FontStyle { Normal, Bold, Italic, BoldItalic }

    /// <summary>Available font families in DisplayKit.</summary>
    public enum FontType
    {
        Default, LiberationSans,
        RobotoRegular, RobotoItalic, RobotoBold, RobotoBoldItalic,
        RobotoLight, RobotoLightItalic, RobotoMedium, RobotoMediumItalic,
        RobotoThin, RobotoThinItalic,
        RobotoMonoRegular, RobotoMonoItalic, RobotoMonoBold, RobotoMonoBoldItalic,
        RobotoMonoLight, RobotoMonoLightItalic, RobotoMonoMedium, RobotoMonoMediumItalic,
        RobotoMonoThin, RobotoMonoThinItalic
    }

    public enum CanvasVisibility { Visible, Hidden }
}

// ============================================================================
// Style Data Classes (use Unity's Style* types directly)
// ============================================================================

namespace DisplayKit
{
    public class BackgroundData { public StyleColor Color { get; set; } }

    public class FlexData
    {
        public StyleFloat Grow { get; set; }
        public StyleFloat Shrink { get; set; }
        public StyleLength Basis { get; set; }
        public UIElements.StyleEnum<FlexDirection> Direction { get; set; }
        public UIElements.StyleEnum<Wrap> Wrap { get; set; }
    }

    public class AlignData
    {
        public UIElements.StyleEnum<Align> AlignItems { get; set; }
        public UIElements.StyleEnum<Justify> JustifyContent { get; set; }
        public UIElements.StyleEnum<Align> AlignSelf { get; set; }
        public UIElements.StyleEnum<Align> AlignContent { get; set; }
    }

    public class SizeData
    {
        public StyleLength Width { get; set; }
        public StyleLength Height { get; set; }
        public StyleLength MinWidth { get; set; }
        public StyleLength MinHeight { get; set; }
        public StyleLength MaxWidth { get; set; }
        public StyleLength MaxHeight { get; set; }
    }

    public class SpacingData
    {
        public StyleLength MarginTop { get; set; }
        public StyleLength MarginBottom { get; set; }
        public StyleLength MarginLeft { get; set; }
        public StyleLength MarginRight { get; set; }
        public StyleLength PaddingTop { get; set; }
        public StyleLength PaddingBottom { get; set; }
        public StyleLength PaddingLeft { get; set; }
        public StyleLength PaddingRight { get; set; }
    }

    public class BorderData
    {
        public StyleColor Color { get; set; }
        public StyleColor TopColor { get; set; }
        public StyleColor BottomColor { get; set; }
        public StyleColor LeftColor { get; set; }
        public StyleColor RightColor { get; set; }
        public StyleFloat Width { get; set; }
        public StyleFloat TopWidth { get; set; }
        public StyleFloat BottomWidth { get; set; }
        public StyleFloat LeftWidth { get; set; }
        public StyleFloat RightWidth { get; set; }
        public StyleLength Radius { get; set; }
        public StyleLength TopLeftRadius { get; set; }
        public StyleLength TopRightRadius { get; set; }
        public StyleLength BottomLeftRadius { get; set; }
        public StyleLength BottomRightRadius { get; set; }
    }

    public class PositionData
    {
        public UIElements.StyleEnum<Position> Position { get; set; }
        public StyleLength Top { get; set; }
        public StyleLength Bottom { get; set; }
        public StyleLength Left { get; set; }
        public StyleLength Right { get; set; }
    }

    public class TransformData
    {
        public StyleTranslate Translate { get; set; }
        public StyleScale Scale { get; set; }
        public StyleRotate Rotate { get; set; }
        public StyleTransformOrigin TransformOrigin { get; set; }
    }

    public class DisplayData
    {
        public UIElements.StyleEnum<DisplayStyle> Display { get; set; }
        public UIElements.StyleEnum<Visibility> Visibility { get; set; }
        public StyleFloat Opacity { get; set; }
        public UIElements.StyleEnum<Overflow> Overflow { get; set; }
    }

    public class TextData
    {
        public FontType? Font { get; set; }
        public UIElements.StyleEnum<Enums.FontStyle> FontStyle { get; set; }
        public StyleLength FontSize { get; set; }
        public StyleColor Color { get; set; }
        public TextAnchor? Align { get; set; }
        public UIElements.StyleEnum<WhiteSpace> Wrap { get; set; }
        public UIElements.StyleEnum<TextOverflow> Overflow { get; set; }
        public StyleLength LetterSpacing { get; set; }
        public StyleLength WordSpacing { get; set; }
        public StyleLength ParagraphSpacing { get; set; }
        public StyleFloat OutlineWidth { get; set; }
        public StyleColor OutlineColor { get; set; }
        public StyleTextShadow TextShadow { get; set; }
    }
}

// ============================================================================
// IDisplayStyleTarget
// ============================================================================

namespace DisplayKit
{
    public interface IDisplayStyleTarget
    {
        BackgroundData Background { get; }
        FlexData Flex { get; }
        AlignData Align { get; }
        SizeData Size { get; }
        SpacingData Spacing { get; }
        BorderData Border { get; }
        PositionData Position { get; }
        TransformData Transform { get; }
        DisplayData Display { get; }
        TextData Text { get; }
    }
}

// ============================================================================
// StyleParser — CSS string → direct apply to IDisplayStyleTarget
// ============================================================================

namespace DisplayKit
{
    /// <summary>Parses CSS style strings and directly applies values to DisplayKit elements.</summary>
    public static class StyleParser
    {
        /// <summary>Parse a CSS string and apply all recognised properties to the element.</summary>
        public static void ParseAndApply(string cssStyle, IDisplayStyleTarget element)
        {
            if (string.IsNullOrWhiteSpace(cssStyle) || element == null) return;
            foreach (string decl in cssStyle.Split(';'))
            {
                string t = decl.Trim();
                if (t.Length == 0) continue;
                int ci = t.IndexOf(':');
                if (ci < 0) continue;
                string prop = t.Substring(0, ci).Trim().ToLowerInvariant();
                string val = t.Substring(ci + 1).Trim();
                if (prop.Length == 0 || val.Length == 0) continue;
                Apply(prop, val, element);
            }
        }

        private static void Apply(string prop, string val, IDisplayStyleTarget e)
        {
            switch (prop)
            {
                // Flex
                case "flex-grow": e.Flex.Grow = CssParse.ParseFloat(val); break;
                case "flex-shrink": e.Flex.Shrink = CssParse.ParseFloat(val); break;
                case "flex-basis": e.Flex.Basis = CssParse.ParseLength(val); break;
                case "flex-direction": e.Flex.Direction = new UIElements.StyleEnum<FlexDirection>(CssParse.ParseEnum<FlexDirection>(val)); break;
                case "flex-wrap": e.Flex.Wrap = new UIElements.StyleEnum<Wrap>(CssParse.ParseEnum<Wrap>(val)); break;
                // Align
                case "align-items": e.Align.AlignItems = new UIElements.StyleEnum<Align>(CssParse.ParseEnum<Align>(val)); break;
                case "justify-content": e.Align.JustifyContent = new UIElements.StyleEnum<Justify>(CssParse.ParseEnum<Justify>(val)); break;
                case "align-self": e.Align.AlignSelf = new UIElements.StyleEnum<Align>(CssParse.ParseEnum<Align>(val)); break;
                case "align-content": e.Align.AlignContent = new UIElements.StyleEnum<Align>(CssParse.ParseEnum<Align>(val)); break;
                // Size
                case "width": e.Size.Width = CssParse.ParseLength(val); break;
                case "height": e.Size.Height = CssParse.ParseLength(val); break;
                case "min-width": e.Size.MinWidth = CssParse.ParseLength(val); break;
                case "min-height": e.Size.MinHeight = CssParse.ParseLength(val); break;
                case "max-width": e.Size.MaxWidth = CssParse.ParseLength(val); break;
                case "max-height": e.Size.MaxHeight = CssParse.ParseLength(val); break;
                // Background
                case "background-color": e.Background.Color = CssParse.ParseColor(val); break;
                // Spacing
                case "margin-top": e.Spacing.MarginTop = CssParse.ParseLength(val); break;
                case "margin-bottom": e.Spacing.MarginBottom = CssParse.ParseLength(val); break;
                case "margin-left": e.Spacing.MarginLeft = CssParse.ParseLength(val); break;
                case "margin-right": e.Spacing.MarginRight = CssParse.ParseLength(val); break;
                case "padding-top": e.Spacing.PaddingTop = CssParse.ParseLength(val); break;
                case "padding-bottom": e.Spacing.PaddingBottom = CssParse.ParseLength(val); break;
                case "padding-left": e.Spacing.PaddingLeft = CssParse.ParseLength(val); break;
                case "padding-right": e.Spacing.PaddingRight = CssParse.ParseLength(val); break;
                // Border
                case "border-color": e.Border.Color = CssParse.ParseColor(val); break;
                case "border-top-color": e.Border.TopColor = CssParse.ParseColor(val); break;
                case "border-bottom-color": e.Border.BottomColor = CssParse.ParseColor(val); break;
                case "border-left-color": e.Border.LeftColor = CssParse.ParseColor(val); break;
                case "border-right-color": e.Border.RightColor = CssParse.ParseColor(val); break;
                case "border-width": e.Border.Width = CssParse.ParseFloat(val); break;
                case "border-top-width": e.Border.TopWidth = CssParse.ParseFloat(val); break;
                case "border-bottom-width": e.Border.BottomWidth = CssParse.ParseFloat(val); break;
                case "border-left-width": e.Border.LeftWidth = CssParse.ParseFloat(val); break;
                case "border-right-width": e.Border.RightWidth = CssParse.ParseFloat(val); break;
                case "border-radius": e.Border.Radius = CssParse.ParseLength(val); break;
                case "border-top-left-radius": e.Border.TopLeftRadius = CssParse.ParseLength(val); break;
                case "border-top-right-radius": e.Border.TopRightRadius = CssParse.ParseLength(val); break;
                case "border-bottom-left-radius": e.Border.BottomLeftRadius = CssParse.ParseLength(val); break;
                case "border-bottom-right-radius": e.Border.BottomRightRadius = CssParse.ParseLength(val); break;
                // Position
                case "position": e.Position.Position = new UIElements.StyleEnum<Position>(CssParse.ParseEnum<Position>(val)); break;
                case "top": e.Position.Top = CssParse.ParseLength(val); break;
                case "bottom": e.Position.Bottom = CssParse.ParseLength(val); break;
                case "left": e.Position.Left = CssParse.ParseLength(val); break;
                case "right": e.Position.Right = CssParse.ParseLength(val); break;
                // Transform
                case "translate": e.Transform.Translate = CssParse.ParseTranslate(val); break;
                case "scale": e.Transform.Scale = CssParse.ParseScale(val); break;
                case "rotate": e.Transform.Rotate = CssParse.ParseRotate(val); break;
                case "transform-origin": e.Transform.TransformOrigin = CssParse.ParseTransformOrigin(val); break;
                // Display
                case "display": e.Display.Display = new UIElements.StyleEnum<DisplayStyle>(CssParse.ParseEnum<DisplayStyle>(val)); break;
                case "visibility": e.Display.Visibility = new UIElements.StyleEnum<Visibility>(CssParse.ParseEnum<Visibility>(val)); break;
                case "opacity": e.Display.Opacity = CssParse.ParseFloat(val); break;
                case "overflow": e.Display.Overflow = new UIElements.StyleEnum<Overflow>(CssParse.ParseEnum<Overflow>(val)); break;
                // Text
                case "color": e.Text.Color = CssParse.ParseColor(val); break;
                case "font-size": e.Text.FontSize = CssParse.ParseLength(val); break;
                case "font-style": e.Text.FontStyle = new UIElements.StyleEnum<Enums.FontStyle>(CssParse.ParseEnum<Enums.FontStyle>(val)); break;
                case "white-space": e.Text.Wrap = new UIElements.StyleEnum<WhiteSpace>(CssParse.ParseEnum<WhiteSpace>(val)); break;
                case "text-overflow": e.Text.Overflow = new UIElements.StyleEnum<TextOverflow>(CssParse.ParseEnum<TextOverflow>(val)); break;
                case "letter-spacing": e.Text.LetterSpacing = CssParse.ParseLength(val); break;
                case "word-spacing": e.Text.WordSpacing = CssParse.ParseLength(val); break;
                case "paragraph-spacing": e.Text.ParagraphSpacing = CssParse.ParseLength(val); break;
                case "outline-width": e.Text.OutlineWidth = CssParse.ParseFloat(val); break;
                case "outline-color": e.Text.OutlineColor = CssParse.ParseColor(val); break;
                case "-unity-text-outline-width": e.Text.OutlineWidth = CssParse.ParseFloat(val); break;
                case "-unity-text-outline-color": e.Text.OutlineColor = CssParse.ParseColor(val); break;
                case "-unity-paragraph-spacing": e.Text.ParagraphSpacing = CssParse.ParseLength(val); break;
                case "-unity-font-definition": e.Text.Font = CssParse.ParseFontDef(val); break;
                case "text-shadow": e.Text.TextShadow = CssParse.ParseTextShadow(val); break;
            }
        }
        /// <summary>
        /// Legacy: parse CSS into raw property→value pairs for deferred code generation.
        /// Use <see cref="ParseAndApply"/> for direct application to elements.
        /// </summary>
        public static Dictionary<string, string> Parse(string cssStyle)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(cssStyle)) return result;
            foreach (string decl in cssStyle.Split(';'))
            {
                string t = decl.Trim();
                if (t.Length == 0) continue;
                int ci = t.IndexOf(':');
                if (ci < 0) continue;
                string prop = t.Substring(0, ci).Trim().ToLowerInvariant();
                string val = t.Substring(ci + 1).Trim();
                if (prop.Length > 0 && val.Length > 0)
                    result[prop] = val;
            }
            return result;
        }
    }

    // ============================================================================
    // StyleCodeGen — raw CSS property→value dictionary → C# assignment code
    // ============================================================================

    /// <summary>Generates C# code from parsed CSS key-value pairs.</summary>
    public static class StyleCodeGen
    {
        private static readonly Dictionary<string, string> PathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["flex-grow"] = ".Flex.Grow",
            ["flex-shrink"] = ".Flex.Shrink",
            ["flex-basis"] = ".Flex.Basis",
            ["flex-direction"] = ".Flex.Direction",
            ["flex-wrap"] = ".Flex.Wrap",
            ["align-items"] = ".Align.AlignItems",
            ["justify-content"] = ".Align.JustifyContent",
            ["align-self"] = ".Align.AlignSelf",
            ["align-content"] = ".Align.AlignContent",
            ["width"] = ".Size.Width",
            ["height"] = ".Size.Height",
            ["min-width"] = ".Size.MinWidth",
            ["min-height"] = ".Size.MinHeight",
            ["max-width"] = ".Size.MaxWidth",
            ["max-height"] = ".Size.MaxHeight",
            ["background-color"] = ".Background.Color",
            ["margin-top"] = ".Spacing.MarginTop",
            ["margin-bottom"] = ".Spacing.MarginBottom",
            ["margin-left"] = ".Spacing.MarginLeft",
            ["margin-right"] = ".Spacing.MarginRight",
            ["padding-top"] = ".Spacing.PaddingTop",
            ["padding-bottom"] = ".Spacing.PaddingBottom",
            ["padding-left"] = ".Spacing.PaddingLeft",
            ["padding-right"] = ".Spacing.PaddingRight",
            ["border-color"] = ".Border.Color",
            ["border-top-color"] = ".Border.TopColor",
            ["border-bottom-color"] = ".Border.BottomColor",
            ["border-left-color"] = ".Border.LeftColor",
            ["border-right-color"] = ".Border.RightColor",
            ["border-width"] = ".Border.Width",
            ["border-top-width"] = ".Border.TopWidth",
            ["border-bottom-width"] = ".Border.BottomWidth",
            ["border-left-width"] = ".Border.LeftWidth",
            ["border-right-width"] = ".Border.RightWidth",
            ["border-radius"] = ".Border.Radius",
            ["border-top-left-radius"] = ".Border.TopLeftRadius",
            ["border-top-right-radius"] = ".Border.TopRightRadius",
            ["border-bottom-left-radius"] = ".Border.BottomLeftRadius",
            ["border-bottom-right-radius"] = ".Border.BottomRightRadius",
            ["position"] = ".Position.Position",
            ["top"] = ".Position.Top",
            ["bottom"] = ".Position.Bottom",
            ["left"] = ".Position.Left",
            ["right"] = ".Position.Right",
            ["translate"] = ".Transform.Translate",
            ["scale"] = ".Transform.Scale",
            ["rotate"] = ".Transform.Rotate",
            ["transform-origin"] = ".Transform.TransformOrigin",
            ["display"] = ".Display.Display",
            ["visibility"] = ".Display.Visibility",
            ["opacity"] = ".Display.Opacity",
            ["overflow"] = ".Display.Overflow",
            ["color"] = ".Text.Color",
            ["font-size"] = ".Text.FontSize",
            ["font-style"] = ".Text.FontStyle",
            ["white-space"] = ".Text.Wrap",
            ["text-overflow"] = ".Text.Overflow",
            ["letter-spacing"] = ".Text.LetterSpacing",
            ["word-spacing"] = ".Text.WordSpacing",
            ["paragraph-spacing"] = ".Text.ParagraphSpacing",
            ["outline-width"] = ".Text.OutlineWidth",
            ["outline-color"] = ".Text.OutlineColor",
            ["-unity-text-outline-width"] = ".Text.OutlineWidth",
            ["-unity-text-outline-color"] = ".Text.OutlineColor",
            ["-unity-paragraph-spacing"] = ".Text.ParagraphSpacing",
            ["-unity-font-definition"] = ".Text.Font",
            ["text-shadow"] = ".Text.TextShadow",
        };

        // Property types for code-gen: float, length, color, enum, transform, etc.
        private static readonly HashSet<string> FloatProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "flex-grow","flex-shrink","border-width","border-top-width","border-bottom-width",
          "border-left-width","border-right-width","opacity","outline-width","-unity-text-outline-width" };
        private static readonly HashSet<string> LengthProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "flex-basis","width","height","min-width","min-height","max-width","max-height",
          "margin-top","margin-bottom","margin-left","margin-right",
          "padding-top","padding-bottom","padding-left","padding-right",
          "top","bottom","left","right","border-radius","border-top-left-radius","border-top-right-radius",
          "border-bottom-left-radius","border-bottom-right-radius",
          "font-size","letter-spacing","word-spacing","paragraph-spacing","-unity-paragraph-spacing" };
        private static readonly HashSet<string> ColorProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "background-color","border-color","border-top-color","border-bottom-color",
          "border-left-color","border-right-color","color","outline-color","-unity-text-outline-color" };
        private static readonly HashSet<string> EnumProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "flex-direction","flex-wrap","align-items","justify-content","align-self","align-content",
          "position","display","visibility","overflow","font-style","white-space","text-overflow" };

        /// <summary>Write C# assignments for all non-keyword parsed styles.</summary>
        public static void WriteAssignments(Dictionary<string, string> styles, string varName, StringBuilder sb,string prefix)
        {
            if (styles == null) return;
            foreach (var kv in styles)
            {
                if (CssParse.TryParseKeyword(kv.Value, out _)) continue;
                string path = PathMap.TryGetValue(kv.Key, out var s) ? varName + s : varName + "." + kv.Key;
                string code = GenerateAssignment(kv.Key, kv.Value, path);
                if (code != null)
                    sb.AppendLine(prefix + code);
            }
        }

        private static string GenerateAssignment(string prop, string rawVal, string path)
        {
            if (FloatProps.Contains(prop))
            {
                string n = CssParse.StripSuffix(rawVal.Trim(), "px");
                return float.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out float f)
                    ? $"{path} = {f.ToString(CultureInfo.InvariantCulture)}f;" : null;
            }
            if (LengthProps.Contains(prop))
            {
                var t = rawVal.Trim();
                if (t.EndsWith("%") && float.TryParse(t.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float pct))
                    return $"{path} = Length.Percent({pct.ToString(CultureInfo.InvariantCulture)}f);";
                string n = t.EndsWith("px", StringComparison.OrdinalIgnoreCase) ? t.Substring(0, t.Length - 2) : t;
                return float.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out float px)
                    ? $"{path} = {px.ToString(CultureInfo.InvariantCulture)}f;" : null;
            }
            if (ColorProps.Contains(prop))
                return GenerateColorCode(rawVal, path);
            if (EnumProps.Contains(prop))
                return GenerateEnumCode(prop, rawVal, path);
            // Transform properties
            switch (prop)
            {
                case "translate": return GenerateTranslateCode(rawVal, path);
                case "scale": return GenerateScaleCode(rawVal, path);
                case "rotate": return GenerateRotateCode(rawVal, path);
                case "transform-origin": return GenerateTransformOriginCode(rawVal, path);
                case "text-shadow": return GenerateTextShadowCode(rawVal, path);
                case "-unity-font-definition":
                    var ft = CssParse.ParseFontDef(rawVal);
                    return ft.HasValue ? $"{path} = FontType.{ft.Value};" : null;
            }
            return null;
        }

        private static string GenerateColorCode(string raw, string path)
        {
            var c = CssParse.ParseColor(raw);
            if (c.keyword != StyleKeyword.Undefined) return null;
            return $"{path} = new Color({c.value.r}f, {c.value.g}f, {c.value.b}f, {c.value.a}f);";
        }

        private static string GenerateEnumCode(string prop, string raw, string path)
        {
            string enumType = GetEnumType(prop);
            if (enumType == null || CssParse.TryParseKeyword(raw, out _)) return null;
            // CSS kebab-case → PascalCase enum member: "flex-start" → "FlexStart"
            var norm = raw.Trim().Replace("-", "");
            if (string.IsNullOrEmpty(norm)) return null;
            // Capitalise first letter
            var member = char.ToUpperInvariant(norm[0]) + norm.Substring(1);
            return $"{path} = {enumType}.{member};";
        }

        private static string GetEnumType(string prop) => prop switch
        {
            "flex-direction" => "FlexDirection",
            "flex-wrap" => "Wrap",
            "align-items" or "align-self" or "align-content" => "Align",
            "justify-content" => "Justify",
            "position" => "Position",
            "display" => "DisplayStyle",
            "visibility" => "Visibility",
            "overflow" => "Overflow",
            "font-style" => "FontStyle",
            "white-space" => "WhiteSpace",
            "text-overflow" => "TextOverflow",
            _ => null
        };

        private static string GenerateTranslateCode(string raw, string path)
        {
            var v = CssParse.ParseTranslate(raw);
            if (v.keyword != StyleKeyword.Undefined) return null;
            var t = v.value;
            string x = LengthCode(t.x), y = LengthCode(t.y);
            return t.z != 0f
                ? $"{path} = new Translate({x}, {y}, {t.z.ToString(CultureInfo.InvariantCulture)}f);"
                : $"{path} = new Translate({x}, {y});";
        }

        private static string GenerateScaleCode(string raw, string path)
        {
            var v = CssParse.ParseScale(raw);
            if (v.keyword != StyleKeyword.Undefined) return null;
            var s = v.value.value;
            return $"{path} = new Scale(new Vector3({s.x.ToString(CultureInfo.InvariantCulture)}f, {s.y.ToString(CultureInfo.InvariantCulture)}f, {s.z.ToString(CultureInfo.InvariantCulture)}f));";
        }

        private static string GenerateRotateCode(string raw, string path)
        {
            var v = CssParse.ParseRotate(raw);
            if (v.keyword != StyleKeyword.Undefined) return null;
            return $"{path} = new Rotate({v.value.angle.value.ToString(CultureInfo.InvariantCulture)}f);";
        }

        private static string GenerateTransformOriginCode(string raw, string path)
        {
            var v = CssParse.ParseTransformOrigin(raw);
            if (v.keyword != StyleKeyword.Undefined) return null;
            var o = v.value;
            return $"{path} = new TransformOrigin({o.x}f, {o.y}f, {o.z}f);";
        }

        private static string GenerateTextShadowCode(string raw, string path)
        {
            var v = CssParse.ParseTextShadow(raw);
            if (v.keyword != StyleKeyword.Undefined) return null;
            var ts = v.value;
            return $"{path} = new TextShadow\n{{\n    color = new Color({ts.color.r}f, {ts.color.g}f, {ts.color.b}f, {ts.color.a}f),\n    offset = new Vector2({ts.offset.x}f, {ts.offset.y}f),\n    blurRadius = {ts.blurRadius}f\n}};";
        }

        private static string LengthCode(Length l) =>
            l.unit == LengthUnit.Percent
                ? $"Length.Percent({l.value.ToString(CultureInfo.InvariantCulture)}f)"
                : $"{l.value.ToString(CultureInfo.InvariantCulture)}f";
    }
}

namespace DisplayKit
{
    /// <summary>Copies values directly from Unity's IStyle to DisplayKit data classes.</summary>
    public static class StyleIStyleConverter
    {
        public static void Apply(UIElements.IStyle s, IDisplayStyleTarget e)
        {
            if (s == null || e == null) return;
            ApplyFlex(s, e.Flex);
            ApplyAlign(s, e.Align);
            ApplySize(s, e.Size);
            ApplyBackground(s, e.Background);
            ApplySpacing(s, e.Spacing);
            ApplyBorder(s, e.Border);
            ApplyPosition(s, e.Position);
            ApplyTransform(s, e.Transform);
            ApplyDisplay(s, e.Display);
            ApplyText(s, e.Text);
        }

        private static void ApplyFlex(UIElements.IStyle s, FlexData d) { if (d == null) return; d.Grow = s.flexGrow; d.Shrink = s.flexShrink; d.Basis = s.flexBasis; d.Direction = s.flexDirection; d.Wrap = s.flexWrap; }
        private static void ApplyAlign(UIElements.IStyle s, AlignData d) { if (d == null) return; d.AlignItems = s.alignItems; d.JustifyContent = s.justifyContent; d.AlignSelf = s.alignSelf; d.AlignContent = s.alignContent; }
        private static void ApplySize(UIElements.IStyle s, SizeData d) { if (d == null) return; d.Width = s.width; d.Height = s.height; d.MinWidth = s.minWidth; d.MinHeight = s.minHeight; d.MaxWidth = s.maxWidth; d.MaxHeight = s.maxHeight; }
        private static void ApplyBackground(UIElements.IStyle s, BackgroundData d) { if (d == null) return; d.Color = s.backgroundColor; }
        private static void ApplySpacing(UIElements.IStyle s, SpacingData d) { if (d == null) return; d.MarginTop = s.marginTop; d.MarginBottom = s.marginBottom; d.MarginLeft = s.marginLeft; d.MarginRight = s.marginRight; d.PaddingTop = s.paddingTop; d.PaddingBottom = s.paddingBottom; d.PaddingLeft = s.paddingLeft; d.PaddingRight = s.paddingRight; }

        private static void ApplyBorder(UIElements.IStyle s, BorderData d)
        {
            if (d == null) return;
            d.Color = s.borderTopColor; d.TopColor = s.borderTopColor; d.BottomColor = s.borderBottomColor;
            d.LeftColor = s.borderLeftColor; d.RightColor = s.borderRightColor;
            d.Width = s.borderTopWidth; d.TopWidth = s.borderTopWidth; d.BottomWidth = s.borderBottomWidth;
            d.LeftWidth = s.borderLeftWidth; d.RightWidth = s.borderRightWidth;
            d.Radius = s.borderTopLeftRadius; d.TopLeftRadius = s.borderTopLeftRadius;
            d.TopRightRadius = s.borderTopRightRadius; d.BottomLeftRadius = s.borderBottomLeftRadius;
            d.BottomRightRadius = s.borderBottomRightRadius;
        }

        private static void ApplyPosition(UIElements.IStyle s, PositionData d) { if (d == null) return; d.Position = s.position; d.Top = s.top; d.Bottom = s.bottom; d.Left = s.left; d.Right = s.right; }
        private static void ApplyTransform(UIElements.IStyle s, TransformData d) { if (d == null) return; d.Translate = s.translate; d.Scale = s.scale; d.Rotate = s.rotate; d.TransformOrigin = s.transformOrigin; }
        private static void ApplyDisplay(UIElements.IStyle s, DisplayData d) { if (d == null) return; d.Display = s.display; d.Visibility = s.visibility; d.Opacity = s.opacity; d.Overflow = s.overflow; }

        private static void ApplyText(UIElements.IStyle s, TextData d)
        {
            if (d == null) return;
            d.Color = s.color; d.FontSize = s.fontSize;
            // Unity FontStyle → our FontStyle
            var ufs = s.unityFontStyleAndWeight;
            if (ufs.keyword != StyleKeyword.Undefined)
                d.FontStyle = new UIElements.StyleEnum<Enums.FontStyle>(ufs.keyword);
            else
            {
                var fs = ufs.value switch
                {
                    UnityEngine.FontStyle.BoldAndItalic => Enums.FontStyle.BoldItalic,
                    UnityEngine.FontStyle.Bold => Enums.FontStyle.Bold,
                    UnityEngine.FontStyle.Italic => Enums.FontStyle.Italic,
                    _ => Enums.FontStyle.Normal
                };
                d.FontStyle = new UIElements.StyleEnum<Enums.FontStyle>(fs);
            }
            d.Wrap = s.whiteSpace; d.Overflow = s.textOverflow;
            d.LetterSpacing = s.letterSpacing; d.WordSpacing = s.wordSpacing;
            d.ParagraphSpacing = s.unityParagraphSpacing;
            d.OutlineWidth = s.unityTextOutlineWidth; d.OutlineColor = s.unityTextOutlineColor;
            d.TextShadow = s.textShadow;
            var fd = s.unityFontDefinition;
            if (fd.keyword == StyleKeyword.Undefined && fd.value.font != null)
            {
                string fn = fd.value.font.name;
                System.Enum.TryParse(fn.Replace(" ", "").Replace("-", ""), true, out Enums.FontType ft);
                d.Font = ft;
            }
        }

        /// <summary>Convert Unity IStyle to raw CSS property→value dictionary (for code-gen).</summary>
        public static Dictionary<string, string> ToDictionary(UIElements.IStyle s)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (s == null) return d;
            Add(d, "flex-grow", s.flexGrow);
            Add(d, "flex-shrink", s.flexShrink);
            Add(d, "flex-basis", s.flexBasis);
            Add(d, "flex-direction", s.flexDirection);
            Add(d, "flex-wrap", s.flexWrap);
            Add(d, "align-items", s.alignItems);
            Add(d, "justify-content", s.justifyContent);
            Add(d, "align-self", s.alignSelf);
            Add(d, "align-content", s.alignContent);
            Add(d, "width", s.width);
            Add(d, "height", s.height);
            Add(d, "min-width", s.minWidth);
            Add(d, "min-height", s.minHeight);
            Add(d, "max-width", s.maxWidth);
            Add(d, "max-height", s.maxHeight);
            Add(d, "background-color", s.backgroundColor);
            Add(d, "margin-top", s.marginTop);
            Add(d, "margin-bottom", s.marginBottom);
            Add(d, "margin-left", s.marginLeft);
            Add(d, "margin-right", s.marginRight);
            Add(d, "padding-top", s.paddingTop);
            Add(d, "padding-bottom", s.paddingBottom);
            Add(d, "padding-left", s.paddingLeft);
            Add(d, "padding-right", s.paddingRight);
            Add(d, "border-top-color", s.borderTopColor);
            Add(d, "border-bottom-color", s.borderBottomColor);
            Add(d, "border-left-color", s.borderLeftColor);
            Add(d, "border-right-color", s.borderRightColor);
            Add(d, "border-top-width", s.borderTopWidth);
            Add(d, "border-bottom-width", s.borderBottomWidth);
            Add(d, "border-left-width", s.borderLeftWidth);
            Add(d, "border-right-width", s.borderRightWidth);
            Add(d, "border-top-left-radius", s.borderTopLeftRadius);
            Add(d, "border-top-right-radius", s.borderTopRightRadius);
            Add(d, "border-bottom-left-radius", s.borderBottomLeftRadius);
            Add(d, "border-bottom-right-radius", s.borderBottomRightRadius);
            Add(d, "position", s.position);
            Add(d, "top", s.top);
            Add(d, "bottom", s.bottom);
            Add(d, "left", s.left);
            Add(d, "right", s.right);
            Add(d, "translate", s.translate);
            Add(d, "scale", s.scale);
            Add(d, "rotate", s.rotate);
            Add(d, "transform-origin", s.transformOrigin);
            Add(d, "display", s.display);
            Add(d, "visibility", s.visibility);
            Add(d, "opacity", s.opacity);
            Add(d, "overflow", s.overflow);
            Add(d, "color", s.color);
            Add(d, "font-size", s.fontSize);
            Add(d, "white-space", s.whiteSpace);
            Add(d, "text-overflow", s.textOverflow);
            Add(d, "letter-spacing", s.letterSpacing);
            Add(d, "word-spacing", s.wordSpacing);
            return d;
        }

        /// <summary>Read computed styles from IResolvedStyle (captures all UXML+CSS values, no keyword info).</summary>
        public static Dictionary<string, string> ToDictionary(UIElements.IResolvedStyle rs)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (rs == null) return d;

            // Float (from StyleFloat.value)
            AddF(d, "flex-grow", rs.flexGrow);
            AddF(d, "flex-shrink", rs.flexShrink);
            AddF(d, "opacity", rs.opacity);

            // Lengths (from StyleFloat.value)
            AddF(d, "width", rs.width);
            AddF(d, "height", rs.height);
            AddF(d, "min-width", rs.minWidth);
            AddF(d, "min-height", rs.minHeight);
            AddF(d, "max-width", rs.maxWidth);
            AddF(d, "max-height", rs.maxHeight);
            AddF(d, "flex-basis", rs.flexBasis);
            AddF(d, "margin-top", rs.marginTop);
            AddF(d, "margin-bottom", rs.marginBottom);
            AddF(d, "margin-left", rs.marginLeft);
            AddF(d, "margin-right", rs.marginRight);
            AddF(d, "padding-top", rs.paddingTop);
            AddF(d, "padding-bottom", rs.paddingBottom);
            AddF(d, "padding-left", rs.paddingLeft);
            AddF(d, "padding-right", rs.paddingRight);
            AddF(d, "top", rs.top);
            AddF(d, "bottom", rs.bottom);
            AddF(d, "left", rs.left);
            AddF(d, "right", rs.right);
            AddF(d, "border-top-left-radius", rs.borderTopLeftRadius);
            AddF(d, "border-top-right-radius", rs.borderTopRightRadius);
            AddF(d, "border-bottom-left-radius", rs.borderBottomLeftRadius);
            AddF(d, "border-bottom-right-radius", rs.borderBottomRightRadius);
            AddF(d, "font-size", rs.fontSize);
            AddF(d, "letter-spacing", rs.letterSpacing);
            AddF(d, "word-spacing", rs.wordSpacing);
            AddF(d, "unityParagraphSpacing", rs.unityParagraphSpacing);
            AddF(d, "border-top-width", rs.borderTopWidth);
            AddF(d, "border-bottom-width", rs.borderBottomWidth);
            AddF(d, "border-left-width", rs.borderLeftWidth);
            AddF(d, "border-right-width", rs.borderRightWidth);

            // Colors (from StyleColor.value)
            AddC(d, "background-color", rs.backgroundColor);
            AddC(d, "border-top-color", rs.borderTopColor);
            AddC(d, "border-bottom-color", rs.borderBottomColor);
            AddC(d, "border-left-color", rs.borderLeftColor);
            AddC(d, "border-right-color", rs.borderRightColor);
            AddC(d, "color", rs.color);

            return d;
        }

        private static void AddF(Dictionary<string, string> d, string prop, StyleFloat v)
        { if (v.keyword == StyleKeyword.Undefined && !float.IsNaN(v.value) && v.value != 0f) d[prop] = $"{v.value.ToString(CultureInfo.InvariantCulture)}px"; }
        private static void AddC(Dictionary<string, string> d, string prop, StyleColor v)
        { if (v.keyword == StyleKeyword.Undefined && v.value != Color.clear && v.value.a > 0f) d[prop] = $"rgba({v.value.r * 255:F0},{v.value.g * 255:F0},{v.value.b * 255:F0},{v.value.a})"; }

        private static void Add(Dictionary<string, string> d, string prop, StyleFloat v)
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = v.value.ToString(CultureInfo.InvariantCulture); }
        private static void Add(Dictionary<string, string> d, string prop, StyleLength v)
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = v.value.ToString(); }
        private static void Add(Dictionary<string, string> d, string prop, StyleColor v)
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = $"rgba({v.value.r * 255:F0},{v.value.g * 255:F0},{v.value.b * 255:F0},{v.value.a})"; }
        private static void Add<T>(Dictionary<string, string> d, string prop, UIElements.StyleEnum<T> v) where T : struct, System.Enum
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = v.value.ToString(); }
        private static void Add(Dictionary<string, string> d, string prop, StyleTranslate v)
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = v.value.ToString(); }
        private static void Add(Dictionary<string, string> d, string prop, StyleScale v)
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = v.value.value.ToString(); }
        private static void Add(Dictionary<string, string> d, string prop, StyleRotate v)
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = $"{v.value.angle.value}deg"; }
        private static void Add(Dictionary<string, string> d, string prop, StyleTransformOrigin v)
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = $"{v.value.x} {v.value.y} {v.value.z}"; }
        private static void Add(Dictionary<string, string> d, string prop, StyleTextShadow v)
        { if (v.keyword == StyleKeyword.Undefined) d[prop] = $"{v.value.offset.x}px {v.value.offset.y}px {v.value.blurRadius}px rgba({v.value.color.r * 255:F0},{v.value.color.g * 255:F0},{v.value.color.b * 255:F0},{v.value.color.a})"; }
    }
}
