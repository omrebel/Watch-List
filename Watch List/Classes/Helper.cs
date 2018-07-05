using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.Editors;
using System.Windows;
using System.Windows.Controls;

namespace Watch_List.Classes
{
    public class Helper
    {
        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.RegisterAttached("MaxLines", typeof(int), typeof(Helper), new FrameworkPropertyMetadata(MaxLinesPropertyChanged));
        public static void SetMaxLines(UIElement element, int value)
        {
            element.SetValue(MaxLinesProperty, value);
        }
        public static int GetMaxLines(UIElement element)
        {
            return (int)element.GetValue(MaxLinesProperty);
        }

        private static void MaxLinesPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((TextEdit)source).Loaded += Helper_Loaded;
        }

        static void Helper_Loaded(object sender, RoutedEventArgs e)
        {
            TextEdit editor = sender as TextEdit;
            ((TextBox)editor.EditCore).MaxLines = (int)editor.GetValue(Helper.MaxLinesProperty);
        }

    }
}
