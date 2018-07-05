using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Watch_List.Views
{
    /// <summary>
    /// Interaction logic for LookupView.xaml
    /// </summary>
    public partial class LookupView : UserControl
    {
        public LookupView()
        {
            InitializeComponent();

            //Don't judge me for adding this line. Technically, I'm not breaking MVVM....
            lookupValue.Focus();
        }
    }
}
