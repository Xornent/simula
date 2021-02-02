using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Odyssey.Controls
{
    public class BreadcrumbItemEventArgs : RoutedEventArgs
    {
        public BreadcrumbItemEventArgs(BreadcrumbItem item, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Item = item;
        }

        public BreadcrumbItem Item { get; private set; }
    }

    public delegate void BreadcrumbItemEventHandler(object sender, BreadcrumbItemEventArgs e);

}
