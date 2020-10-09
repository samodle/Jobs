using System;
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
        public event EventHandler StatusEvent, TrashEvent, MoveUpEvent, MoveDownEvent;

        public UserControlActionItem()
        {
            InitializeComponent();
        }

        public void SetInfo(string numLabel, string mainText)
        {
            NumberLabel.Content = numLabel;
            PrimaryTextLabel.Content = mainText;

            if (numLabel.Equals("1"))
            {
                MoveUp.Visibility = Visibility.Hidden;
            }
            else
            {
                MoveUp.Visibility = Visibility.Visible;
            }
        }

        #region MouseDown
        private void MouseDown_Trash(object sender, MouseButtonEventArgs e)
        {
            if (this.TrashEvent != null) { this.TrashEvent(this, new EventArgs()); }
        }

        private void MouseDown_Up(object sender, MouseButtonEventArgs e)
        {
            if (this.MoveUpEvent != null) { this.MoveUpEvent(this, new EventArgs()); }
        }

        private void MouseDown_Down(object sender, MouseButtonEventArgs e)
        {
            if (this.MoveDownEvent != null) { this.MoveDownEvent(this, new EventArgs()); }
        }

        private void Checkbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Check.Visibility == Visibility.Visible)
            {
                Check.Visibility = Visibility.Hidden;
                PrimaryTextLabel.Foreground = Brushes.Black;

                if (this.StatusEvent != null) { this.StatusEvent(this, new EventArgs()); }
            }
            else
            {
                Check.Visibility = Visibility.Visible;
                PrimaryTextLabel.Foreground = Brushes.DarkGray;

                if (this.StatusEvent != null) { this.StatusEvent(this, new EventArgs()); }
            }
        }
        #endregion

        #region Mouse Move/Leave
        public void MouseMoveGeneric(object sender, EventArgs e)
        {
            if (sender.GetType().ToString().EndsWith("Image"))
            {
                Image tempsender = (Image)sender;
                tempsender.Opacity = 0.8;
            }
            else
            {
                Control tempsender = (Control)sender;
                tempsender.Opacity = 0.8;
            }

        }

        public void MouseLeaveGeneric(object sender, EventArgs e)
        {
            if (sender.GetType().ToString().EndsWith("Image"))
            {
                Image tempsender = (Image)sender;
                tempsender.Opacity = 1.0;
            }
            else
            {
                Control tempsender = (Control)sender;
                tempsender.Opacity = 1.0;
            }
        }
        #endregion


    }
}
