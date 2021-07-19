using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GolemUI
{
    public class EditButton : Button
    {

        static EditButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditButton), new FrameworkPropertyMetadata(typeof(EditButton)));
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource),
            typeof(EditButton), new FrameworkPropertyMetadata(null));

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceHoverProperty = DependencyProperty.Register("ImageSourceHover", typeof(ImageSource),
            typeof(EditButton), new FrameworkPropertyMetadata(null));

        public ImageSource ImageSourceHover
        {
            get { return (ImageSource)GetValue(ImageSourceHoverProperty); }
            set { SetValue(ImageSourceHoverProperty, value); }
        }

        public static readonly DependencyProperty ImageSourcePressedProperty = DependencyProperty.Register("ImageSourcePressed", typeof(ImageSource),
            typeof(EditButton), new FrameworkPropertyMetadata(null));

        public ImageSource ImageSourcePressed
        {
            get { return (ImageSource)GetValue(ImageSourcePressedProperty); }
            set { SetValue(ImageSourcePressedProperty, value); }
        }

    }
}
