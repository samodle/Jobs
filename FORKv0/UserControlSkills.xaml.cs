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
    /// Interaction logic for UserControlSkills.xaml
    /// </summary>
    public partial class UserControlSkills : UserControl
    {
        public static SolidColorBrush lightPurpleBrush = new SolidColorBrush(Color.FromRgb(128, 110, 177));
        public static SolidColorBrush whiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));


        public UserControlSkills()
        {
            InitializeComponent();
        }

        public void SetLevel(int n)
        {
            if(n < 0) { n = 0; }
            else if(n > 5) { n = 5; }

            switch (n)
            {
                case 0:
                    Skill1.Fill = whiteBrush;
                    Skill2.Fill = whiteBrush;
                    Skill3.Fill = whiteBrush;
                    Skill4.Fill = whiteBrush;
                    Skill5.Fill = whiteBrush;
                    break;
                case 1:
                    Skill1.Fill = lightPurpleBrush;
                    Skill2.Fill = whiteBrush;
                    Skill3.Fill = whiteBrush;
                    Skill4.Fill = whiteBrush;
                    Skill5.Fill = whiteBrush;
                    break;
                case 2:
                    Skill1.Fill = lightPurpleBrush;
                    Skill2.Fill = lightPurpleBrush;
                    Skill3.Fill = whiteBrush;
                    Skill4.Fill = whiteBrush;
                    Skill5.Fill = whiteBrush;
                    break;
                case 3:
                    Skill1.Fill = lightPurpleBrush;
                    Skill2.Fill = lightPurpleBrush;
                    Skill3.Fill = lightPurpleBrush;
                    Skill4.Fill = whiteBrush;
                    Skill5.Fill = whiteBrush;
                    break;
                case 4:
                    Skill1.Fill = lightPurpleBrush;
                    Skill2.Fill = lightPurpleBrush;
                    Skill3.Fill = lightPurpleBrush;
                    Skill4.Fill = lightPurpleBrush;
                    Skill5.Fill = whiteBrush;
                    break;
                case 5:
                    Skill1.Fill = lightPurpleBrush;
                    Skill2.Fill = lightPurpleBrush;
                    Skill3.Fill = lightPurpleBrush;
                    Skill4.Fill = lightPurpleBrush;
                    Skill5.Fill = lightPurpleBrush;
                    break;

            }
        }
    }
}
