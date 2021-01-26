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
    }
}
