
using Simula.Editor.Utils;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Simula.Editor.Highlighting
{
    /// <summary>
    /// A highlighting color is a set of font properties and foreground and background color.
    /// </summary>
    [Serializable]
    public class HighlightingColor : ISerializable, IFreezable, ICloneable, IEquatable<HighlightingColor>
    {
        internal static readonly HighlightingColor Empty = FreezableHelper.FreezeAndReturn(new HighlightingColor());
        private string name;
        private FontFamily fontFamily = null;
        private int? fontSize;
        private FontWeight? fontWeight;
        private FontStyle? fontStyle;
        private bool? underline;
        private bool? strikethrough;
        private HighlightingBrush foreground;
        private HighlightingBrush background;
        private bool frozen;

        /// <summary>
        /// Gets/Sets the name of the color.
        /// </summary>
        public string Name {
            get {
                return name;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                name = value;
            }
        }

        /// <summary>
        /// Gets/sets the font family. Null if the highlighting color does not change the font style.
        /// </summary>
        public FontFamily FontFamily {
            get {
                return fontFamily;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                fontFamily = value;
            }
        }

        /// <summary>
        /// Gets/sets the font size. Null if the highlighting color does not change the font style.
        /// </summary>
        public int? FontSize {
            get {
                return fontSize;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                fontSize = value;
            }
        }

        /// <summary>
        /// Gets/sets the font weight. Null if the highlighting color does not change the font weight.
        /// </summary>
        public FontWeight? FontWeight {
            get {
                return fontWeight;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                fontWeight = value;
            }
        }

        /// <summary>
        /// Gets/sets the font style. Null if the highlighting color does not change the font style.
        /// </summary>
        public FontStyle? FontStyle {
            get {
                return fontStyle;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                fontStyle = value;
            }
        }

        /// <summary>
        ///  Gets/sets the underline flag. Null if the underline status does not change the font style.
        /// </summary>
        public bool? Underline {
            get {
                return underline;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                underline = value;
            }
        }

        /// <summary>
        ///  Gets/sets the strikethrough flag. Null if the strikethrough status does not change the font style.
        /// </summary>
        public bool? Strikethrough {
            get {
                return strikethrough;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                strikethrough = value;
            }
        }

        /// <summary>
        /// Gets/sets the foreground color applied by the highlighting.
        /// </summary>
        public HighlightingBrush Foreground {
            get {
                return foreground;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                foreground = value;
            }
        }

        /// <summary>
        /// Gets/sets the background color applied by the highlighting.
        /// </summary>
        public HighlightingBrush Background {
            get {
                return background;
            }
            set {
                if (frozen)
                    throw new InvalidOperationException();
                background = value;
            }
        }

        /// <summary>
        /// Creates a new HighlightingColor instance.
        /// </summary>
        public HighlightingColor()
        {
        }

        /// <summary>
        /// Deserializes a HighlightingColor.
        /// </summary>
        protected HighlightingColor(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            Name = info.GetString("Name");
            if (info.GetBoolean("HasWeight"))
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(info.GetInt32("Weight"));
            if (info.GetBoolean("HasStyle"))
                FontStyle = (FontStyle?)new FontStyleConverter().ConvertFromInvariantString(info.GetString("Style"));
            if (info.GetBoolean("HasUnderline"))
                Underline = info.GetBoolean("Underline");
            if (info.GetBoolean("HasStrikethrough"))
                Strikethrough = info.GetBoolean("Strikethrough");
            Foreground = (HighlightingBrush)info.GetValue("Foreground", typeof(SimpleHighlightingBrush));
            Background = (HighlightingBrush)info.GetValue("Background", typeof(SimpleHighlightingBrush));
            if (info.GetBoolean("HasFamily"))
                FontFamily = new FontFamily(info.GetString("Family"));
            if (info.GetBoolean("HasSize"))
                FontSize = info.GetInt32("Size");
        }

        /// <summary>
        /// Serializes this HighlightingColor instance.
        /// </summary>
        [System.Security.SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("Name", Name);
            info.AddValue("HasWeight", FontWeight.HasValue);
            if (FontWeight.HasValue)
                info.AddValue("Weight", FontWeight.Value.ToOpenTypeWeight());
            info.AddValue("HasStyle", FontStyle.HasValue);
            if (FontStyle.HasValue)
                info.AddValue("Style", FontStyle.Value.ToString());
            info.AddValue("HasUnderline", Underline.HasValue);
            if (Underline.HasValue)
                info.AddValue("Underline", Underline.Value);
            info.AddValue("HasStrikethrough", Strikethrough.HasValue);
            if (Strikethrough.HasValue)
                info.AddValue("Strikethrough", Strikethrough.Value);
            info.AddValue("Foreground", Foreground);
            info.AddValue("Background", Background);
            info.AddValue("HasFamily", FontFamily != null);
            if (FontFamily != null)
                info.AddValue("Family", FontFamily.FamilyNames.FirstOrDefault());
            info.AddValue("HasSize", FontSize.HasValue);
            if (FontSize.HasValue)
                info.AddValue("Size", FontSize.Value.ToString());
        }

        /// <summary>
        /// Gets CSS code for the color.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "CSS usually uses lowercase, and all possible values are English-only")]
        public virtual string ToCss()
        {
            StringBuilder b = new StringBuilder();
            if (Foreground != null) {
                Color? c = Foreground.GetColor(null);
                if (c != null) {
                    b.AppendFormat(CultureInfo.InvariantCulture, "color: #{0:x2}{1:x2}{2:x2}; ", c.Value.R, c.Value.G, c.Value.B);
                }
            }
            if (FontWeight != null) {
                b.Append("font-weight: ");
                b.Append(FontWeight.Value.ToString().ToLowerInvariant());
                b.Append("; ");
            }
            if (FontStyle != null) {
                b.Append("font-style: ");
                b.Append(FontStyle.Value.ToString().ToLowerInvariant());
                b.Append("; ");
            }
            if (Underline != null) {
                b.Append("text-decoration: ");
                b.Append(Underline.Value ? "underline" : "none");
                b.Append("; ");
            }
            if (Strikethrough != null) {
                if (Underline == null)
                    b.Append("text-decoration:  ");

                b.Remove(b.Length - 1, 1);
                b.Append(Strikethrough.Value ? " line-through" : " none");
                b.Append("; ");
            }
            return b.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[" + GetType().Name + " " + (string.IsNullOrEmpty(Name) ? ToCss() : Name) + "]";
        }

        /// <summary>
        /// Prevent further changes to this highlighting color.
        /// </summary>
        public virtual void Freeze()
        {
            frozen = true;
        }

        /// <summary>
        /// Gets whether this HighlightingColor instance is frozen.
        /// </summary>
        public bool IsFrozen {
            get { return frozen; }
        }

        /// <summary>
        /// Clones this highlighting color.
        /// If this color is frozen, the clone will be unfrozen.
        /// </summary>
        public virtual HighlightingColor Clone()
        {
            HighlightingColor c = (HighlightingColor)MemberwiseClone();
            c.frozen = false;
            return c;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <inheritdoc/>
        public override sealed bool Equals(object obj)
        {
            return Equals(obj as HighlightingColor);
        }

        /// <inheritdoc/>
        public virtual bool Equals(HighlightingColor other)
        {
            if (other == null)
                return false;
            return name == other.name && fontWeight == other.fontWeight
                && fontStyle == other.fontStyle && underline == other.underline && strikethrough == other.strikethrough
                && object.Equals(foreground, other.foreground) && object.Equals(background, other.background)
                && object.Equals(fontFamily, other.fontFamily) && object.Equals(FontSize, other.FontSize);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked {
                if (name != null)
                    hashCode += 1000000007 * name.GetHashCode();
                hashCode += 1000000009 * fontWeight.GetHashCode();
                hashCode += 1000000021 * fontStyle.GetHashCode();
                if (foreground != null)
                    hashCode += 1000000033 * foreground.GetHashCode();
                if (background != null)
                    hashCode += 1000000087 * background.GetHashCode();
                if (fontFamily != null)
                    hashCode += 1000000123 * fontFamily.GetHashCode();
                if (fontSize != null)
                    hashCode += 1000000167 * fontSize.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Overwrites the properties in this HighlightingColor with those from the given color;
        /// but maintains the current values where the properties of the given color return <c>null</c>.
        /// </summary>
        public void MergeWith(HighlightingColor color)
        {
            FreezableHelper.ThrowIfFrozen(this);
            if (color.fontWeight != null)
                fontWeight = color.fontWeight;
            if (color.fontStyle != null)
                fontStyle = color.fontStyle;
            if (color.foreground != null)
                foreground = color.foreground;
            if (color.background != null)
                background = color.background;
            if (color.underline != null)
                underline = color.underline;
            if (color.strikethrough != null)
                strikethrough = color.strikethrough;
            if (color.fontFamily != null)
                fontFamily = color.fontFamily;
            if (color.fontSize != null)
                fontSize = color.fontSize;
        }

        internal bool IsEmptyForMerge {
            get {
                return fontWeight == null && fontStyle == null && underline == null
                       && strikethrough == null && foreground == null && background == null
                       && fontFamily == null && fontSize == null;
            }
        }
    }
}
