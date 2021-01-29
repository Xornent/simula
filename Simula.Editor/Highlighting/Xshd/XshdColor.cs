
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace Simula.Editor.Highlighting.Xshd
{
    /// <summary>
    /// A color in an Xshd file.
    /// </summary>
    [Serializable]
    public class XshdColor : XshdElement, ISerializable
    {
        /// <summary>
        /// Gets/sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the font family
        /// </summary>
        public FontFamily FontFamily { get; set; }

        /// <summary>
        /// Gets/sets the font size.
        /// </summary>
        public int? FontSize { get; set; }

        /// <summary>
        /// Gets/sets the foreground brush.
        /// </summary>
        public HighlightingBrush Foreground { get; set; }

        /// <summary>
        /// Gets/sets the background brush.
        /// </summary>
        public HighlightingBrush Background { get; set; }

        /// <summary>
        /// Gets/sets the font weight.
        /// </summary>
        public FontWeight? FontWeight { get; set; }

        /// <summary>
        /// Gets/sets the underline flag
        /// </summary>
        public bool? Underline { get; set; }

        /// <summary>
        /// Gets/sets the strikethrough flag
        /// </summary>
        public bool? Strikethrough { get; set; }

        /// <summary>
        /// Gets/sets the font style.
        /// </summary>
        public FontStyle? FontStyle { get; set; }

        /// <summary>
        /// Gets/Sets the example text that demonstrates where the color is used.
        /// </summary>
        public string ExampleText { get; set; }

        /// <summary>
        /// Creates a new XshdColor instance.
        /// </summary>
        public XshdColor()
        {
        }

        /// <summary>
        /// Deserializes an XshdColor.
        /// </summary>
        protected XshdColor(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            Name = info.GetString("Name");
            Foreground = (HighlightingBrush)info.GetValue("Foreground", typeof(HighlightingBrush));
            Background = (HighlightingBrush)info.GetValue("Background", typeof(HighlightingBrush));
            if (info.GetBoolean("HasWeight"))
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(info.GetInt32("Weight"));
            if (info.GetBoolean("HasStyle"))
                FontStyle = (FontStyle?)new FontStyleConverter().ConvertFromInvariantString(info.GetString("Style"));
            ExampleText = info.GetString("ExampleText");
            if (info.GetBoolean("HasUnderline"))
                Underline = info.GetBoolean("Underline");
            if (info.GetBoolean("HasStrikethrough"))
                Strikethrough = info.GetBoolean("Strikethrough");
            if (info.GetBoolean("HasFamily"))
                FontFamily = new FontFamily(info.GetString("Family"));
            if (info.GetBoolean("HasSize"))
                FontSize = info.GetInt32("Size");
        }

        /// <summary>
        /// Serializes this XshdColor instance.
        /// </summary>
        [System.Security.SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("Name", Name);
            info.AddValue("Foreground", Foreground);
            info.AddValue("Background", Background);
            info.AddValue("HasUnderline", Underline.HasValue);
            if (Underline.HasValue)
                info.AddValue("Underline", Underline.Value);
            info.AddValue("HasStrikethrough", Strikethrough.HasValue);
            if (Strikethrough.HasValue)
                info.AddValue("Strikethrough", Strikethrough.Value);
            info.AddValue("HasWeight", FontWeight.HasValue);
            info.AddValue("HasWeight", FontWeight.HasValue);
            if (FontWeight.HasValue)
                info.AddValue("Weight", FontWeight.Value.ToOpenTypeWeight());
            info.AddValue("HasStyle", FontStyle.HasValue);
            if (FontStyle.HasValue)
                info.AddValue("Style", FontStyle.Value.ToString());
            info.AddValue("ExampleText", ExampleText);
            info.AddValue("HasFamily", FontFamily != null);
            if (FontFamily != null)
                info.AddValue("Family", FontFamily.FamilyNames.FirstOrDefault());
            info.AddValue("HasSize", FontSize.HasValue);
            if (FontSize.HasValue)
                info.AddValue("Size", FontSize.Value.ToString());
        }

        /// <inheritdoc/>
        public override object AcceptVisitor(IXshdVisitor visitor)
        {
            return visitor.VisitColor(this);
        }
    }
}
