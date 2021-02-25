using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simula.UI
{
    public class IconTreeNode : TreeViewItem
    {
        public ImageSource Icon { get; set; }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(IconTreeNode), new UIPropertyMetadata(null));

        public IconTreeNode() : base()
        {
            
        }

        public IconTreeNode(string text)
        {
            this.Header = text;
        }

        public IconTreeNode( string text, string imageExpr = "resources/icons/apple-files.png")
        {
            ImageSourceConverter isc = new ImageSourceConverter();
            SetCurrentValue(IconProperty, isc.ConvertFrom("pack://siteoforigin:,,,/" + imageExpr) as ImageSource);
            this.Header = text;
        }
    }
}
