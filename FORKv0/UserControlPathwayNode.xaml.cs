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
    /// Interaction logic for UserControlPathwayNode.xaml
    /// </summary>
    public partial class UserControlPathwayNode : UserControl
    {
        public event EventHandler SelectEvent;

        public UserControlPathwayNode()
        {
            InitializeComponent();
        }

        #region MouseDown
        private void Ball_Generic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.SelectEvent != null) { this.SelectEvent(this, new EventArgs()); }
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
