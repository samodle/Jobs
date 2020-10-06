﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Windows_Desktop
{
    /// <summary>
    /// Interaction logic for UserControlActionItem.xaml
    /// </summary>
    public partial class UserControlActionItem : UserControl
    {
        public UserControlActionItem()
        {
            InitializeComponent();
        }

        private void Checkbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(Check.Visibility == Visibility.Visible)
            {
                Check.Visibility = Visibility.Hidden;
                PrimaryTextLabel.Foreground = Brushes.Black;
            }
            else
            {
                Check.Visibility = Visibility.Visible;
                PrimaryTextLabel.Foreground = Brushes.DarkGray;
            }
        }
    }
}
