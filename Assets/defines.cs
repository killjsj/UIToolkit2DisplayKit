// AIGC,Model:Deepseek-v4-pro


// ============================================================================
// DisplayKit Style Properties - Auto-generated from StyleProperties.md
// No UnityEngine.UIElements references allowed.
// All Style types inherit from BaseStyle.
// ============================================================================

using DisplayKit.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using FontStyle = DisplayKit.Enums.FontStyle;

// ============================================================================
// Keyword
// ============================================================================

namespace DisplayKit
{
    /// <summary>Represents a style keyword for unset/initial values.</summary>
    public enum StyleKeyword
    {
        Undefined,
        Null,
        Auto,
        None,
        Initial
    }
}

// ============================================================================
// Primitive Types (replace UnityEngine.UIElements types)
// ============================================================================

namespace DisplayKit
{
    /// <summary>Unit type for length values.</summary>
    public enum LengthUnit
    {
        Pixel,
        Percent
    }

    /// <summary>Represents a length value (pixel or percent).</summary>
    public struct Length
    {
        public float value;
        public LengthUnit unit;

        public Length(float value, LengthUnit unit)
        {
            this.value = value;
            this.unit = unit;
        }

        public static Length Percent(float value) => new Length(value, LengthUnit.Percent);

        public static implicit operator Length(float pixelValue) => new Length(pixelValue, LengthUnit.Pixel);

        public override string ToString() => unit == LengthUnit.Percent ? $"{value}%" : $"{value}px";
    }

    /// <summary>Represents a scale transformation.</summary>
    public struct Scale
    {
        public Vector3 value;

        public Scale(Vector3 value) { this.value = value; }

        public static implicit operator Scale(Vector3 value) => new Scale(value);

        public override string ToString() => value.ToString();
    }

    /// <summary>Represents a rotation transformation in degrees.</summary>
    public struct Rotate
    {
        public float angle;

        public Rotate(float angle) { this.angle = angle; }

        public static implicit operator Rotate(float angle) => new Rotate(angle);

        public override string ToString() => $"{angle}deg";
    }

    /// <summary>Represents a translation offset.</summary>
    public struct Translate
    {
        public Vector2 value;

        public Translate(Vector2 value) { this.value = value; }

        public static implicit operator Translate(Vector2 value) => new Translate(value);

        public override string ToString() => value.ToString();
    }

    /// <summary>Represents a transform origin point.</summary>
    public struct TransformOrigin
    {
        public float x;
        public float y;
        public float z;

        public TransformOrigin(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() => $"({x}, {y}, {z})";
    }

    /// <summary>Represents a text shadow effect.</summary>
    public struct TextShadow
    {
        public Color color;
        public Vector2 offset;
        public float blurRadius;

        public TextShadow(StyleKeyword keyword)
        {
            color = Color.clear;
            offset = Vector2.zero;
            blurRadius = 0f;
        }

        public override string ToString() => $"Shadow(color={color}, offset={offset}, blur={blurRadius})";
    }
}

// ============================================================================
// BaseStyle - Abstract base class for all style wrappers with CSS parsing
// ============================================================================

namespace DisplayKit
{
    /// <summary>
    /// Abstract base class for all style value wrappers.
    /// Stores the raw CSS string and supports keyword-based styles.
    /// Parses input like "200px", "rgb(0,0,0)", "center", "flex-start", "initial" etc.
    /// </summary>
    public abstract class BaseStyle
    {
        /// <summary>The raw CSS string value (e.g. "200px", "rgb(0,0,0)", "center").</summary>
        public string RawValue { get; set; }

        /// <summary>The CSS property name this style belongs to (e.g. "flex-grow", "background-color").</summary>
        public string CssPropertyName { get; set; }

        /// <summary>The style keyword, or <see cref="StyleKeyword.Undefined"/> when a concrete value is set.</summary>
        public StyleKeyword Keyword { get; protected set; }

        /// <summary>Whether this style represents a keyword rather than a concrete value.</summary>
        public bool IsKeyword => Keyword != StyleKeyword.Undefined;

        /// <summary>Creates an empty (undefined) style.</summary>
        protected BaseStyle()
        {
            Keyword = StyleKeyword.Undefined;
        }

        /// <summary>Creates a style from a raw CSS string and parses it immediately.</summary>
        protected BaseStyle(string rawValue)
        {
            RawValue = rawValue?.Trim();
            Parse();
        }

        /// <summary>Creates a keyword-only style.</summary>
        protected BaseStyle(StyleKeyword keyword)
        {
            Keyword = keyword;
            RawValue = keyword.ToString().ToLowerInvariant();
        }

        /// <summary>Parse <see cref="RawValue"/> into concrete value or keyword.</summary>
        protected abstract void Parse();

        // ---- CSS → DisplayKit path mapping ----

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
            ["text-shadow"] = ".Text.TextShadow",
        };

        /// <summary>Resolve the full DisplayKit property path (e.g. "element" + ".Flex.Grow").</summary>
        protected string FullPath(string baseVarName)
        {
            if (!string.IsNullOrEmpty(CssPropertyName) && PathMap.TryGetValue(CssPropertyName, out var suffix))
                return baseVarName + suffix;
            return baseVarName; // fallback: use as-is
        }

        /// <summary>
        /// Generate C# assignment code using the CSS→DisplayKit path mapping.
        /// <c>style.ToCode("element", sb)</c> outputs <c>element.Flex.Grow = 1f;</c>
        /// </summary>
        public abstract void ToCode(string targetVarName, StringBuilder sb);

        // ---- keyword helpers ----

        protected static bool TryParseKeyword(string raw, out StyleKeyword keyword)
        {
            if (string.IsNullOrEmpty(raw))
            {
                keyword = StyleKeyword.Undefined;
                return false;
            }

            switch (raw.Trim().ToLowerInvariant())
            {
                case "initial":
                    keyword = StyleKeyword.Initial;
                    return true;
                case "auto":
                    keyword = StyleKeyword.Auto;
                    return true;
                case "none":
                    keyword = StyleKeyword.None;
                    return true;
                case "null":
                    keyword = StyleKeyword.Null;
                    return true;
                default:
                    keyword = StyleKeyword.Undefined;
                    return false;
            }
        }

        /// <summary>Returns the raw CSS string.</summary>
        public override string ToString() => RawValue ?? string.Empty;
    }
}

// ============================================================================
// Style Wrapper Classes (inherit BaseStyle, support CSS string parsing)
// ============================================================================

namespace DisplayKit
{
    /// <summary>
    /// Wrapper for float style values.
    /// Parses "1", "0.5", "initial", "auto", etc.
    /// </summary>
    public class StyleFloat : BaseStyle
    {
        public float Value { get; set; }

        public StyleFloat() : base() { }
        public StyleFloat(float value) : base() { Value = value; RawValue = value.ToString(CultureInfo.InvariantCulture); }
        public StyleFloat(string rawValue) : base(rawValue) { }
        public StyleFloat(StyleKeyword keyword) : base(keyword) { }

        protected override void Parse()
        {
            if (TryParseKeyword(RawValue, out var kw))
            {
                Keyword = kw;
                return;
            }
            if (float.TryParse(RawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            {
                Value = val;
                Keyword = StyleKeyword.Undefined;
                return;
            }
            Value = 0f;
        }

        public static implicit operator StyleFloat(float value) => new StyleFloat(value);
        public static implicit operator StyleFloat(string rawValue) => new StyleFloat(rawValue);
        public static implicit operator StyleFloat(StyleKeyword keyword) => new StyleFloat(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            sb.AppendLine($"{FullPath(targetVarName)} = {Value.ToString(CultureInfo.InvariantCulture)}f;");
        }
    }

    /// <summary>
    /// Wrapper for Color style values.
    /// Parses "red", "rgb(51,51,51)", "rgba(0,0,0,0.5)", "#333", "#333333", "#333333AA", etc.
    /// </summary>
    public class StyleColor : BaseStyle
    {
        private static readonly Regex RgbRegex = new Regex(
            @"rgba?\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+))?\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex HexRegex = new Regex(
            @"^#([0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$",
            RegexOptions.Compiled);

        public Color Value { get; set; }

        public StyleColor() : base() { Value = Color.clear; }
        public StyleColor(Color value) : base() { Value = value; RawValue = ToCssString(value); }
        public StyleColor(string rawValue) : base(rawValue) { }
        public StyleColor(StyleKeyword keyword) : base(keyword) { Value = Color.clear; }

        protected override void Parse()
        {
            if (TryParseKeyword(RawValue, out var kw))
            {
                Keyword = kw;
                return;
            }

            var trimmed = RawValue.Trim();

            if (TryParseNamedColor(trimmed, out Color namedColor))
            {
                Value = namedColor;
                Keyword = StyleKeyword.Undefined;
                return;
            }

            var rgbMatch = RgbRegex.Match(trimmed);
            if (rgbMatch.Success)
            {
                float r = int.Parse(rgbMatch.Groups[1].Value) / 255f;
                float g = int.Parse(rgbMatch.Groups[2].Value) / 255f;
                float b = int.Parse(rgbMatch.Groups[3].Value) / 255f;
                float a = rgbMatch.Groups[4].Success
                    ? float.Parse(rgbMatch.Groups[4].Value, CultureInfo.InvariantCulture)
                    : 1f;
                Value = new Color(r, g, b, a);
                Keyword = StyleKeyword.Undefined;
                return;
            }

            var hexMatch = HexRegex.Match(trimmed);
            if (hexMatch.Success)
            {
                Value = ParseHexColor(hexMatch.Groups[1].Value);
                Keyword = StyleKeyword.Undefined;
                return;
            }

            Value = Color.clear;
        }

        private static bool TryParseNamedColor(string name, out Color color)
        {
            switch (name.ToLowerInvariant())
            {
                case "red": color = Color.red; return true;
                case "green": color = Color.green; return true;
                case "blue": color = Color.blue; return true;
                case "white": color = Color.white; return true;
                case "black": color = Color.black; return true;
                case "yellow": color = Color.yellow; return true;
                case "cyan": color = Color.cyan; return true;
                case "magenta": color = Color.magenta; return true;
                case "gray":
                case "grey": color = Color.gray; return true;
                case "clear":
                case "transparent": color = Color.clear; return true;
                default: color = default; return false;
            }
        }

        private static Color ParseHexColor(string hex)
        {
            int r, g, b, a = 255;
            switch (hex.Length)
            {
                case 3: r = HexD(hex[0]) * 17; g = HexD(hex[1]) * 17; b = HexD(hex[2]) * 17; break;
                case 4: r = HexD(hex[0]) * 17; g = HexD(hex[1]) * 17; b = HexD(hex[2]) * 17; a = HexD(hex[3]) * 17; break;
                case 6: r = HexD(hex[0]) << 4 | HexD(hex[1]); g = HexD(hex[2]) << 4 | HexD(hex[3]); b = HexD(hex[4]) << 4 | HexD(hex[5]); break;
                case 8: r = HexD(hex[0]) << 4 | HexD(hex[1]); g = HexD(hex[2]) << 4 | HexD(hex[3]); b = HexD(hex[4]) << 4 | HexD(hex[5]); a = HexD(hex[6]) << 4 | HexD(hex[7]); break;
                default: return Color.clear;
            }
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        private static int HexD(char c) =>
            c >= '0' && c <= '9' ? c - '0' :
            c >= 'a' && c <= 'f' ? c - 'a' + 10 :
            c >= 'A' && c <= 'F' ? c - 'A' + 10 : 0;

        private static string ToCssString(Color c) =>
            $"rgba({Mathf.RoundToInt(c.r * 255)},{Mathf.RoundToInt(c.g * 255)},{Mathf.RoundToInt(c.b * 255)},{c.a.ToString(CultureInfo.InvariantCulture)})";

        public static implicit operator StyleColor(Color value) => new StyleColor(value);
        public static implicit operator StyleColor(string rawValue) => new StyleColor(rawValue);
        public static implicit operator StyleColor(StyleKeyword keyword) => new StyleColor(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            sb.AppendLine($"{FullPath(targetVarName)} = new Color({Value.r}f, {Value.g}f, {Value.b}f, {Value.a}f);");
        }
    }

    /// <summary>
    /// Wrapper for Length style values.
    /// Parses "200px", "50%", "auto", "200" (defaults to px), etc.
    /// </summary>
    public class StyleLength : BaseStyle
    {
        public Length Value { get; set; }

        public StyleLength() : base() { }
        public StyleLength(Length value) : base() { Value = value; RawValue = value.ToString(); }
        public StyleLength(string rawValue) : base(rawValue) { }
        public StyleLength(StyleKeyword keyword) : base(keyword) { }

        protected override void Parse()
        {
            if (TryParseKeyword(RawValue, out var kw))
            {
                Keyword = kw;
                return;
            }

            var trimmed = RawValue.Trim();

            if (trimmed.EndsWith("%"))
            {
                if (float.TryParse(trimmed.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out float pct))
                {
                    Value = new Length(pct, LengthUnit.Percent);
                    Keyword = StyleKeyword.Undefined;
                    return;
                }
            }

            string numPart = trimmed.EndsWith("px", StringComparison.OrdinalIgnoreCase)
                ? trimmed.Substring(0, trimmed.Length - 2)
                : trimmed;

            if (float.TryParse(numPart, NumberStyles.Float, CultureInfo.InvariantCulture, out float px))
            {
                Value = new Length(px, LengthUnit.Pixel);
                Keyword = StyleKeyword.Undefined;
                return;
            }

            Value = default;
        }

        public static implicit operator StyleLength(Length value) => new StyleLength(value);
        public static implicit operator StyleLength(float pixelValue) => new StyleLength(new Length(pixelValue, LengthUnit.Pixel));
        public static implicit operator StyleLength(string rawValue) => new StyleLength(rawValue);
        public static implicit operator StyleLength(StyleKeyword keyword) => new StyleLength(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            if (Value.unit == LengthUnit.Percent)
                sb.AppendLine($"{FullPath(targetVarName)} = Length.Percent({Value.value.ToString(CultureInfo.InvariantCulture)}f);");
            else
                sb.AppendLine($"{FullPath(targetVarName)} = {Value.value.ToString(CultureInfo.InvariantCulture)}f;");
        }
    }

    /// <summary>
    /// Generic wrapper for enum style values.
    /// Parses CSS kebab-case ("flex-start" → FlexStart, "space-between" → SpaceBetween, "nowrap" → NoWrap)
    /// or direct enum names. Falls back to keyword if no enum match.
    /// </summary>
    public class StyleEnum<T> : BaseStyle where T : struct, System.Enum
    {
        public T Value { get; set; }

        public StyleEnum() : base() { Value = default; }
        public StyleEnum(T value) : base()
        {
            Value = value;
            RawValue = value.ToString().ToLowerInvariant();
        }
        public StyleEnum(string rawValue) : base(rawValue) { }
        public StyleEnum(StyleKeyword keyword) : base(keyword) { Value = default; }

        protected override void Parse()
        {
            // Try as typed enum value first (handles "none" for DisplayStyle.None before keyword)
            if (TryParseEnum(RawValue, out T enumVal))
            {
                Value = enumVal;
                Keyword = StyleKeyword.Undefined;
                return;
            }

            if (TryParseKeyword(RawValue, out var kw))
            {
                Keyword = kw;
                return;
            }

            Value = default;
        }

        /// <summary>
        /// Converts a kebab-case CSS value (e.g. "flex-start", "space-between", "nowrap")
        /// by removing hyphens and doing case-insensitive Enum.TryParse.
        /// </summary>
        private static bool TryParseEnum(string raw, out T result)
        {
            if (string.IsNullOrEmpty(raw))
            {
                result = default;
                return false;
            }

            var normalized = raw.Trim().Replace("-", "");
            return Enum.TryParse(normalized, true, out result);
        }

        public static implicit operator StyleEnum<T>(T value) => new StyleEnum<T>(value);
        public static implicit operator StyleEnum<T>(string rawValue) => new StyleEnum<T>(rawValue);
        public static implicit operator StyleEnum<T>(StyleKeyword keyword) => new StyleEnum<T>(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            sb.AppendLine($"{FullPath(targetVarName)} = {typeof(T).Name}.{Value};");
        }
    }

    /// <summary>Wrapper for Translate style values.</summary>
    public class StyleTranslate : BaseStyle
    {
        public Translate Value { get; set; }

        public StyleTranslate() : base() { }
        public StyleTranslate(Translate value) : base() { Value = value; RawValue = value.ToString(); }
        public StyleTranslate(string rawValue) : base(rawValue) { }
        public StyleTranslate(StyleKeyword keyword) : base(keyword) { }

        protected override void Parse()
        {
            if (TryParseKeyword(RawValue, out var kw)) { Keyword = kw; return; }
        }

        public static implicit operator StyleTranslate(Translate value) => new StyleTranslate(value);
        public static implicit operator StyleTranslate(string rawValue) => new StyleTranslate(rawValue);
        public static implicit operator StyleTranslate(StyleKeyword keyword) => new StyleTranslate(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            sb.AppendLine($"{FullPath(targetVarName)} = new Vector2({Value.value.x}f, {Value.value.y}f);");
        }
    }

    /// <summary>Wrapper for Scale style values.</summary>
    public class StyleScale : BaseStyle
    {
        public Scale Value { get; set; }

        public StyleScale() : base() { }
        public StyleScale(Scale value) : base() { Value = value; RawValue = value.ToString(); }
        public StyleScale(string rawValue) : base(rawValue) { }
        public StyleScale(StyleKeyword keyword) : base(keyword) { }

        protected override void Parse()
        {
            if (TryParseKeyword(RawValue, out var kw)) { Keyword = kw; return; }
        }

        public static implicit operator StyleScale(Scale value) => new StyleScale(value);
        public static implicit operator StyleScale(string rawValue) => new StyleScale(rawValue);
        public static implicit operator StyleScale(StyleKeyword keyword) => new StyleScale(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            sb.AppendLine($"{FullPath(targetVarName)} = new Vector3({Value.value.x}f, {Value.value.y}f, {Value.value.z}f);");
        }
    }

    /// <summary>Wrapper for Rotate style values.</summary>
    public class StyleRotate : BaseStyle
    {
        public Rotate Value { get; set; }

        public StyleRotate() : base() { }
        public StyleRotate(Rotate value) : base() { Value = value; RawValue = value.ToString(); }
        public StyleRotate(string rawValue) : base(rawValue) { }
        public StyleRotate(StyleKeyword keyword) : base(keyword) { }

        protected override void Parse()
        {
            if (TryParseKeyword(RawValue, out var kw)) { Keyword = kw; return; }
        }

        public static implicit operator StyleRotate(Rotate value) => new StyleRotate(value);
        public static implicit operator StyleRotate(string rawValue) => new StyleRotate(rawValue);
        public static implicit operator StyleRotate(StyleKeyword keyword) => new StyleRotate(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            sb.AppendLine($"{FullPath(targetVarName)} = {Value.angle}f;");
        }
    }

    /// <summary>Wrapper for TransformOrigin style values.</summary>
    public class StyleTransformOrigin : BaseStyle
    {
        public TransformOrigin Value { get; set; }

        public StyleTransformOrigin() : base() { }
        public StyleTransformOrigin(TransformOrigin value) : base() { Value = value; RawValue = value.ToString(); }
        public StyleTransformOrigin(string rawValue) : base(rawValue) { }
        public StyleTransformOrigin(StyleKeyword keyword) : base(keyword) { }

        protected override void Parse()
        {
            if (TryParseKeyword(RawValue, out var kw)) { Keyword = kw; return; }
        }

        public static implicit operator StyleTransformOrigin(TransformOrigin value) => new StyleTransformOrigin(value);
        public static implicit operator StyleTransformOrigin(string rawValue) => new StyleTransformOrigin(rawValue);
        public static implicit operator StyleTransformOrigin(StyleKeyword keyword) => new StyleTransformOrigin(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            sb.AppendLine($"{FullPath(targetVarName)} = new TransformOrigin({Value.x}f, {Value.y}f, {Value.z}f);");
        }
    }

    /// <summary>Wrapper for TextShadow style values.</summary>
    public class StyleTextShadow : BaseStyle
    {
        public TextShadow Value { get; set; }

        public StyleTextShadow() : base() { }
        public StyleTextShadow(TextShadow value) : base() { Value = value; RawValue = value.ToString(); }
        public StyleTextShadow(string rawValue) : base(rawValue) { }
        public StyleTextShadow(StyleKeyword keyword) : base(keyword) { }

        protected override void Parse()
        {
            if (TryParseKeyword(RawValue, out var kw)) { Keyword = kw; return; }

            // CSS text-shadow: <offset-x> <offset-y> <blur-radius>? <color>?
            // e.g. "12px 12px 1px rgb(255, 0, 0)" or "2px 2px 4px black"
            var parts = RawValue.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return;

            var shadow = new TextShadow { color = Color.black, offset = Vector2.zero, blurRadius = 0f };

            int numIndex = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                string p = parts[i];
                if (i == parts.Length - 1 || (i >= 2 && !IsNumericPart(p)))
                {
                    var sc = new StyleColor(string.Join(" ", parts, i, parts.Length - i));
                    if (!sc.IsKeyword) shadow.color = sc.Value;
                    break;
                }
                string numStr = p.EndsWith("px", StringComparison.OrdinalIgnoreCase)
                    ? p.Substring(0, p.Length - 2) : p;
                if (float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                {
                    switch (numIndex)
                    {
                        case 0: shadow.offset.x = f; break;
                        case 1: shadow.offset.y = f; break;
                        case 2: shadow.blurRadius = f; break;
                    }
                    numIndex++;
                }
            }

            Value = shadow;
            Keyword = StyleKeyword.Undefined;
        }

        private static bool IsNumericPart(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            string num = s.EndsWith("px", StringComparison.OrdinalIgnoreCase)
                ? s.Substring(0, s.Length - 2) : s;
            return float.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        public static implicit operator StyleTextShadow(TextShadow value) => new StyleTextShadow(value);
        public static implicit operator StyleTextShadow(string rawValue) => new StyleTextShadow(rawValue);
        public static implicit operator StyleTextShadow(StyleKeyword keyword) => new StyleTextShadow(keyword);

        public override void ToCode(string targetVarName, StringBuilder sb)
        {
            if (IsKeyword) return;
            sb.AppendLine($"{FullPath(targetVarName)} = new TextShadow");
            sb.AppendLine("{");
            sb.AppendLine($"    color = new Color({Value.color.r}f, {Value.color.g}f, {Value.color.b}f, {Value.color.a}f),");
            sb.AppendLine($"    offset = new Vector2({Value.offset.x}f, {Value.offset.y}f),");
            sb.AppendLine($"    blurRadius = {Value.blurRadius}f");
            sb.AppendLine("};");
        }
    }
}

// ============================================================================
// Enums
// ============================================================================

namespace DisplayKit.Enums
{
    /// <summary>Flexbox layout direction.</summary>
    public enum FlexDirection
    {
        Row,
        Column,
        RowReverse,
        ColumnReverse
    }

    /// <summary>Flexbox wrap mode.</summary>
    public enum Wrap
    {
        NoWrap,
        Wrap
    }

    /// <summary>Cross-axis alignment for flex children.</summary>
    public enum Align
    {
        FlexStart,
        Center,
        FlexEnd,
        Stretch
    }

    /// <summary>Main-axis justification for flex children.</summary>
    public enum Justify
    {
        FlexStart,
        Center,
        FlexEnd,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly
    }

    /// <summary>Positioning mode.</summary>
    public enum Position
    {
        Relative,
        Absolute
    }

    /// <summary>Display style mode.</summary>
    public enum DisplayStyle
    {
        Flex,
        None
    }

    /// <summary>Element visibility.</summary>
    public enum Visibility
    {
        Visible,
        Hidden
    }

    /// <summary>Overflow handling mode.</summary>
    public enum Overflow
    {
        Visible,
        Hidden,
        Scroll
    }

    /// <summary>Font style (weight and slant).</summary>
    public enum FontStyle
    {
        Normal,
        Bold,
        Italic,
        BoldItalic
    }

    /// <summary>White space / text wrapping mode.</summary>
    public enum WhiteSpace
    {
        Normal,
        NoWrap,
        Pre,
        PreWrap
    }

    /// <summary>Text overflow handling.</summary>
    public enum TextOverflow
    {
        Visible,
        Clip,
        Ellipsis
    }

    /// <summary>Available font families in DisplayKit.</summary>
    public enum FontType
    {
        // -- Default --
        Default,
        LiberationSans,

        // -- Roboto --
        RobotoRegular,
        RobotoItalic,
        RobotoBold,
        RobotoBoldItalic,
        RobotoLight,
        RobotoLightItalic,
        RobotoMedium,
        RobotoMediumItalic,
        RobotoThin,
        RobotoThinItalic,

        // -- Roboto Mono --
        RobotoMonoRegular,
        RobotoMonoItalic,
        RobotoMonoBold,
        RobotoMonoBoldItalic,
        RobotoMonoLight,
        RobotoMonoLightItalic,
        RobotoMonoMedium,
        RobotoMonoMediumItalic,
        RobotoMonoThin,
        RobotoMonoThinItalic
    }

    /// <summary>Canvas default visibility mode.</summary>
    public enum CanvasVisibility
    {
        Visible,
        Hidden
    }
}

// ============================================================================
// Style Data Classes
// ============================================================================

namespace DisplayKit
{
    /// <summary>Controls the element's background appearance.</summary>
    public class BackgroundData
    {
        /// <summary>Background color.</summary>
        public StyleColor Color { get; set; }
    }

    /// <summary>Controls flexbox layout behavior.</summary>
    public class FlexData
    {
        /// <summary>How much element grows relative to siblings.</summary>
        public StyleFloat Grow { get; set; }

        /// <summary>How much element shrinks relative to siblings.</summary>
        public StyleFloat Shrink { get; set; }

        /// <summary>Initial size before grow/shrink.</summary>
        public StyleLength Basis { get; set; }

        /// <summary>Layout direction (Row, Column, RowReverse, ColumnReverse).</summary>
        public StyleEnum<FlexDirection> Direction { get; set; }

        /// <summary>Whether items wrap to multiple lines (Wrap, NoWrap).</summary>
        public StyleEnum<Wrap> Wrap { get; set; }
    }

    /// <summary>Controls alignment of children and self.</summary>
    public class AlignData
    {
        /// <summary>Cross-axis alignment of children.</summary>
        public StyleEnum<Align> AlignItems { get; set; }

        /// <summary>Main-axis distribution of children.</summary>
        public StyleEnum<Justify> JustifyContent { get; set; }

        /// <summary>Override this element's cross-axis alignment.</summary>
        public StyleEnum<Align> AlignSelf { get; set; }

        /// <summary>Multi-line content alignment when wrapping.</summary>
        public StyleEnum<Align> AlignContent { get; set; }
    }

    /// <summary>Controls element dimensions.</summary>
    public class SizeData
    {
        /// <summary>Element width.</summary>
        public StyleLength Width { get; set; }

        /// <summary>Element height.</summary>
        public StyleLength Height { get; set; }

        /// <summary>Minimum width.</summary>
        public StyleLength MinWidth { get; set; }

        /// <summary>Minimum height.</summary>
        public StyleLength MinHeight { get; set; }

        /// <summary>Maximum width.</summary>
        public StyleLength MaxWidth { get; set; }

        /// <summary>Maximum height.</summary>
        public StyleLength MaxHeight { get; set; }
    }

    /// <summary>Controls padding and margin.</summary>
    public class SpacingData
    {
        /// <summary>Top margin.</summary>
        public StyleLength MarginTop { get; set; }

        /// <summary>Bottom margin.</summary>
        public StyleLength MarginBottom { get; set; }

        /// <summary>Left margin.</summary>
        public StyleLength MarginLeft { get; set; }

        /// <summary>Right margin.</summary>
        public StyleLength MarginRight { get; set; }

        /// <summary>Top padding.</summary>
        public StyleLength PaddingTop { get; set; }

        /// <summary>Bottom padding.</summary>
        public StyleLength PaddingBottom { get; set; }

        /// <summary>Left padding.</summary>
        public StyleLength PaddingLeft { get; set; }

        /// <summary>Right padding.</summary>
        public StyleLength PaddingRight { get; set; }
    }

    /// <summary>Controls border appearance.</summary>
    public class BorderData
    {
        /// <summary>All border colors (sets all sides).</summary>
        public StyleColor Color { get; set; }

        /// <summary>Top border color.</summary>
        public StyleColor TopColor { get; set; }

        /// <summary>Bottom border color.</summary>
        public StyleColor BottomColor { get; set; }

        /// <summary>Left border color.</summary>
        public StyleColor LeftColor { get; set; }

        /// <summary>Right border color.</summary>
        public StyleColor RightColor { get; set; }

        /// <summary>All border widths (sets all sides).</summary>
        public StyleFloat Width { get; set; }

        /// <summary>Top border width.</summary>
        public StyleFloat TopWidth { get; set; }

        /// <summary>Bottom border width.</summary>
        public StyleFloat BottomWidth { get; set; }

        /// <summary>Left border width.</summary>
        public StyleFloat LeftWidth { get; set; }

        /// <summary>Right border width.</summary>
        public StyleFloat RightWidth { get; set; }

        /// <summary>All corner radii (sets all corners).</summary>
        public StyleLength Radius { get; set; }

        /// <summary>Top-left corner radius.</summary>
        public StyleLength TopLeftRadius { get; set; }

        /// <summary>Top-right corner radius.</summary>
        public StyleLength TopRightRadius { get; set; }

        /// <summary>Bottom-left corner radius.</summary>
        public StyleLength BottomLeftRadius { get; set; }

        /// <summary>Bottom-right corner radius.</summary>
        public StyleLength BottomRightRadius { get; set; }
    }

    /// <summary>Controls element positioning.</summary>
    public class PositionData
    {
        /// <summary>Position mode (Absolute, Relative).</summary>
        public StyleEnum<Enums.Position> Position { get; set; }

        /// <summary>Top offset.</summary>
        public StyleLength Top { get; set; }

        /// <summary>Bottom offset.</summary>
        public StyleLength Bottom { get; set; }

        /// <summary>Left offset.</summary>
        public StyleLength Left { get; set; }

        /// <summary>Right offset.</summary>
        public StyleLength Right { get; set; }
    }

    /// <summary>Controls element transformations.</summary>
    public class TransformData
    {
        /// <summary>Translation offset.</summary>
        public StyleTranslate Translate { get; set; }

        /// <summary>Scale factor.</summary>
        public StyleScale Scale { get; set; }

        /// <summary>Rotation angle.</summary>
        public StyleRotate Rotate { get; set; }

        /// <summary>Transform origin point.</summary>
        public StyleTransformOrigin TransformOrigin { get; set; }
    }

    /// <summary>Controls element visibility and rendering.</summary>
    public class DisplayData
    {
        /// <summary>Display mode (Flex, None).</summary>
        public StyleEnum<DisplayStyle> Display { get; set; }

        /// <summary>Visibility (Visible, Hidden).</summary>
        public StyleEnum<Visibility> Visibility { get; set; }

        /// <summary>Opacity (0-1).</summary>
        public StyleFloat Opacity { get; set; }

        /// <summary>Overflow handling (Visible, Hidden, Scroll).</summary>
        public StyleEnum<Overflow> Overflow { get; set; }
    }

    /// <summary>Text-specific styling (only applies to DisplayText elements).</summary>
    public class TextData
    {
        /// <summary>Font family.</summary>
        public FontType? Font { get; set; }

        /// <summary>Font style (Normal, Italic, Bold, BoldItalic).</summary>
        public StyleEnum<FontStyle> FontStyle { get; set; }

        /// <summary>Font size.</summary>
        public StyleLength FontSize { get; set; }

        /// <summary>Text color.</summary>
        public StyleColor Color { get; set; }

        /// <summary>Text alignment (uses UnityEngine.TextAnchor).</summary>
        public TextAnchor? Align { get; set; }

        /// <summary>Text wrapping mode.</summary>
        public StyleEnum<WhiteSpace> Wrap { get; set; }

        /// <summary>Text overflow handling.</summary>
        public StyleEnum<TextOverflow> Overflow { get; set; }

        /// <summary>Letter spacing.</summary>
        public StyleLength LetterSpacing { get; set; }

        /// <summary>Word spacing.</summary>
        public StyleLength WordSpacing { get; set; }

        /// <summary>Paragraph spacing.</summary>
        public StyleLength ParagraphSpacing { get; set; }

        /// <summary>Text outline width.</summary>
        public StyleFloat OutlineWidth { get; set; }

        /// <summary>Text outline color.</summary>
        public StyleColor OutlineColor { get; set; }

        /// <summary>Text shadow effect.</summary>
        public StyleTextShadow TextShadow { get; set; }
    }
}

// ============================================================================
// IDisplayStyleTarget - Interface for elements exposing DisplayKit data classes
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
// StyleParser - CSS string → Dictionary<string, BaseStyle>
// ============================================================================

namespace DisplayKit
{
    public static class StyleParser
    {
        private static readonly Dictionary<string, Func<string, BaseStyle>> Registry =
            new Dictionary<string, Func<string, BaseStyle>>(StringComparer.OrdinalIgnoreCase)
            {
                ["flex-grow"] = v => (StyleFloat)v,
                ["flex-shrink"] = v => (StyleFloat)v,
                ["flex-basis"] = v => (StyleLength)v,
                ["flex-direction"] = v => (StyleEnum<FlexDirection>)v,
                ["flex-wrap"] = v => (StyleEnum<Wrap>)v,
                ["align-items"] = v => (StyleEnum<Align>)v,
                ["justify-content"] = v => (StyleEnum<Justify>)v,
                ["align-self"] = v => (StyleEnum<Align>)v,
                ["align-content"] = v => (StyleEnum<Align>)v,
                ["width"] = v => (StyleLength)v,
                ["height"] = v => (StyleLength)v,
                ["min-width"] = v => (StyleLength)v,
                ["min-height"] = v => (StyleLength)v,
                ["max-width"] = v => (StyleLength)v,
                ["max-height"] = v => (StyleLength)v,
                ["background-color"] = v => (StyleColor)v,
                ["margin-top"] = v => (StyleLength)v,
                ["margin-bottom"] = v => (StyleLength)v,
                ["margin-left"] = v => (StyleLength)v,
                ["margin-right"] = v => (StyleLength)v,
                ["padding-top"] = v => (StyleLength)v,
                ["padding-bottom"] = v => (StyleLength)v,
                ["padding-left"] = v => (StyleLength)v,
                ["padding-right"] = v => (StyleLength)v,
                ["border-color"] = v => (StyleColor)v,
                ["border-top-color"] = v => (StyleColor)v,
                ["border-bottom-color"] = v => (StyleColor)v,
                ["border-left-color"] = v => (StyleColor)v,
                ["border-right-color"] = v => (StyleColor)v,
                ["border-width"] = v => (StyleFloat)v,
                ["border-top-width"] = v => (StyleFloat)v,
                ["border-bottom-width"] = v => (StyleFloat)v,
                ["border-left-width"] = v => (StyleFloat)v,
                ["border-right-width"] = v => (StyleFloat)v,
                ["border-radius"] = v => (StyleLength)v,
                ["border-top-left-radius"] = v => (StyleLength)v,
                ["border-top-right-radius"] = v => (StyleLength)v,
                ["border-bottom-left-radius"] = v => (StyleLength)v,
                ["border-bottom-right-radius"] = v => (StyleLength)v,
                ["position"] = v => (StyleEnum<Enums.Position>)v,
                ["top"] = v => (StyleLength)v,
                ["bottom"] = v => (StyleLength)v,
                ["left"] = v => (StyleLength)v,
                ["right"] = v => (StyleLength)v,
                ["translate"] = v => (StyleTranslate)v,
                ["scale"] = v => (StyleScale)v,
                ["rotate"] = v => (StyleRotate)v,
                ["transform-origin"] = v => (StyleTransformOrigin)v,
                ["display"] = v => (StyleEnum<DisplayStyle>)v,
                ["visibility"] = v => (StyleEnum<Visibility>)v,
                ["opacity"] = v => (StyleFloat)v,
                ["overflow"] = v => (StyleEnum<Overflow>)v,
                ["color"] = v => (StyleColor)v,
                ["font-size"] = v => (StyleLength)v,
                ["font-style"] = v => (StyleEnum<FontStyle>)v,
                ["white-space"] = v => (StyleEnum<WhiteSpace>)v,
                ["text-overflow"] = v => (StyleEnum<TextOverflow>)v,
                ["letter-spacing"] = v => (StyleLength)v,
                ["word-spacing"] = v => (StyleLength)v,
                ["paragraph-spacing"] = v => (StyleLength)v,
                ["outline-width"] = v => (StyleFloat)v,
                ["outline-color"] = v => (StyleColor)v,
                ["text-shadow"] = v => (StyleTextShadow)v,
            };

        public static Dictionary<string, BaseStyle> Parse(string cssStyle)
        {
            var result = new Dictionary<string, BaseStyle>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(cssStyle)) return result;

            foreach (string decl in cssStyle.Split(';'))
            {
                string t = decl.Trim();
                if (t.Length == 0) continue;
                int ci = t.IndexOf(':');
                if (ci < 0) continue;
                string prop = t.Substring(0, ci).Trim().ToLowerInvariant();
                string val = t.Substring(ci + 1).Trim();
                if (prop.Length == 0 || val.Length == 0) continue;
                if (Registry.TryGetValue(prop, out var factory))
                {
                    var style = factory(val);
                    style.CssPropertyName = prop;
                    result[prop] = style;
                }
            }
            return result;
        }
    }
}

// ============================================================================
// StyleMapper - Dictionary<string, BaseStyle> → DisplayKit data classes
// ============================================================================

namespace DisplayKit
{
    public static class StyleMapper
    {
        public static void Apply(Dictionary<string, BaseStyle> s, FlexData t)
        {
            if (t == null) return;
            if (Try(s, "flex-grow", out StyleFloat fg)) t.Grow = fg;
            if (Try(s, "flex-shrink", out StyleFloat fs)) t.Shrink = fs;
            if (Try(s, "flex-basis", out StyleLength fb)) t.Basis = fb;
            if (Try(s, "flex-direction", out StyleEnum<FlexDirection> fd)) t.Direction = fd;
            if (Try(s, "flex-wrap", out StyleEnum<Wrap> fw)) t.Wrap = fw;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, AlignData t)
        {
            if (t == null) return;
            if (Try(s, "align-items", out StyleEnum<Align> ai)) t.AlignItems = ai;
            if (Try(s, "justify-content", out StyleEnum<Justify> jc)) t.JustifyContent = jc;
            if (Try(s, "align-self", out StyleEnum<Align> af)) t.AlignSelf = af;
            if (Try(s, "align-content", out StyleEnum<Align> ac)) t.AlignContent = ac;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, SizeData t)
        {
            if (t == null) return;
            if (Try(s, "width", out StyleLength w)) t.Width = w;
            if (Try(s, "height", out StyleLength h)) t.Height = h;
            if (Try(s, "min-width", out StyleLength mw)) t.MinWidth = mw;
            if (Try(s, "min-height", out StyleLength mh)) t.MinHeight = mh;
            if (Try(s, "max-width", out StyleLength xw)) t.MaxWidth = xw;
            if (Try(s, "max-height", out StyleLength xh)) t.MaxHeight = xh;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, BackgroundData t)
        {
            if (t == null) return;
            if (Try(s, "background-color", out StyleColor c)) t.Color = c;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, SpacingData t)
        {
            if (t == null) return;
            if (Try(s, "margin-top", out StyleLength mt)) t.MarginTop = mt;
            if (Try(s, "margin-bottom", out StyleLength mb)) t.MarginBottom = mb;
            if (Try(s, "margin-left", out StyleLength ml)) t.MarginLeft = ml;
            if (Try(s, "margin-right", out StyleLength mr)) t.MarginRight = mr;
            if (Try(s, "padding-top", out StyleLength pt)) t.PaddingTop = pt;
            if (Try(s, "padding-bottom", out StyleLength pb)) t.PaddingBottom = pb;
            if (Try(s, "padding-left", out StyleLength pl)) t.PaddingLeft = pl;
            if (Try(s, "padding-right", out StyleLength pr)) t.PaddingRight = pr;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, BorderData t)
        {
            if (t == null) return;
            if (Try(s, "border-color", out StyleColor c)) t.Color = c;
            if (Try(s, "border-top-color", out StyleColor tc)) t.TopColor = tc;
            if (Try(s, "border-bottom-color", out StyleColor bc)) t.BottomColor = bc;
            if (Try(s, "border-left-color", out StyleColor lc)) t.LeftColor = lc;
            if (Try(s, "border-right-color", out StyleColor rc)) t.RightColor = rc;
            if (Try(s, "border-width", out StyleFloat w)) t.Width = w;
            if (Try(s, "border-top-width", out StyleFloat tw)) t.TopWidth = tw;
            if (Try(s, "border-bottom-width", out StyleFloat bw)) t.BottomWidth = bw;
            if (Try(s, "border-left-width", out StyleFloat lw)) t.LeftWidth = lw;
            if (Try(s, "border-right-width", out StyleFloat rw)) t.RightWidth = rw;
            if (Try(s, "border-radius", out StyleLength r)) t.Radius = r;
            if (Try(s, "border-top-left-radius", out StyleLength tlr)) t.TopLeftRadius = tlr;
            if (Try(s, "border-top-right-radius", out StyleLength trr)) t.TopRightRadius = trr;
            if (Try(s, "border-bottom-left-radius", out StyleLength blr)) t.BottomLeftRadius = blr;
            if (Try(s, "border-bottom-right-radius", out StyleLength brr)) t.BottomRightRadius = brr;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, PositionData t)
        {
            if (t == null) return;
            if (Try(s, "position", out StyleEnum<Enums.Position> p)) t.Position = p;
            if (Try(s, "top", out StyleLength tp)) t.Top = tp;
            if (Try(s, "bottom", out StyleLength bt)) t.Bottom = bt;
            if (Try(s, "left", out StyleLength lf)) t.Left = lf;
            if (Try(s, "right", out StyleLength rt)) t.Right = rt;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, TransformData t)
        {
            if (t == null) return;
            if (Try(s, "translate", out StyleTranslate tl)) t.Translate = tl;
            if (Try(s, "scale", out StyleScale sc)) t.Scale = sc;
            if (Try(s, "rotate", out StyleRotate ro)) t.Rotate = ro;
            if (Try(s, "transform-origin", out StyleTransformOrigin to)) t.TransformOrigin = to;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, DisplayData t)
        {
            if (t == null) return;
            if (Try(s, "display", out StyleEnum<DisplayStyle> d)) t.Display = d;
            if (Try(s, "visibility", out StyleEnum<Visibility> v)) t.Visibility = v;
            if (Try(s, "opacity", out StyleFloat o)) t.Opacity = o;
            if (Try(s, "overflow", out StyleEnum<Overflow> ov)) t.Overflow = ov;
        }

        public static void Apply(Dictionary<string, BaseStyle> s, TextData t)
        {
            if (t == null) return;
            if (Try(s, "color", out StyleColor c)) t.Color = c;
            if (Try(s, "font-size", out StyleLength fs)) t.FontSize = fs;
            if (Try(s, "font-style", out StyleEnum<FontStyle> fst)) t.FontStyle = fst;
            if (Try(s, "white-space", out StyleEnum<WhiteSpace> ws)) t.Wrap = ws;
            if (Try(s, "text-overflow", out StyleEnum<TextOverflow> to)) t.Overflow = to;
            if (Try(s, "letter-spacing", out StyleLength ls)) t.LetterSpacing = ls;
            if (Try(s, "word-spacing", out StyleLength wos)) t.WordSpacing = wos;
            if (Try(s, "paragraph-spacing", out StyleLength ps)) t.ParagraphSpacing = ps;
            if (Try(s, "outline-width", out StyleFloat ow)) t.OutlineWidth = ow;
            if (Try(s, "outline-color", out StyleColor oc)) t.OutlineColor = oc;
            if (Try(s, "text-shadow", out StyleTextShadow ts)) t.TextShadow = ts;
        }

        /// <summary>Apply all matching styles to every data class of the element.</summary>
        public static void ApplyAll(Dictionary<string, BaseStyle> styles, IDisplayStyleTarget element)
        {
            if (element == null || styles == null || styles.Count == 0) return;
            Apply(styles, element.Flex);
            Apply(styles, element.Align);
            Apply(styles, element.Size);
            Apply(styles, element.Background);
            Apply(styles, element.Spacing);
            Apply(styles, element.Border);
            Apply(styles, element.Position);
            Apply(styles, element.Transform);
            Apply(styles, element.Display);
            Apply(styles, element.Text);
        }

        /// <summary>One-liner: parse CSS string and apply to element directly.</summary>
        public static void ParseAndApply(string cssStyle, IDisplayStyleTarget element)
        {
            ApplyAll(StyleParser.Parse(cssStyle), element);
        }

        private static bool Try<T>(Dictionary<string, BaseStyle> styles, string property, out T result) where T : BaseStyle
        {
            if (styles.TryGetValue(property, out var bs) && bs is T typed)
            {
                result = typed;
                return true;
            }
            result = null;
            return false;
        }
    }
}
