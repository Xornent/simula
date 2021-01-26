using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Simula
{
    public class FolderItem
    {
        public string Folder { get; set; }
        public ImageSource Image { get; set; }

        public FolderItem()
            : base()
        {
            ImageSourceConverter isc = new ImageSourceConverter();
            Image = isc.ConvertFrom("pack://siteoforigin:,,,/resources/icons/folder.png") as ImageSource;
        }
    }

}
