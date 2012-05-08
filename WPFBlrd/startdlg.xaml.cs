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
using System.Windows.Shapes;

namespace WPFBlrd
{
    /// <summary>
    /// Interaction logic for startdlg.xaml
    /// </summary>
    public partial class startdlg : Window
    {
        public string m_name1, m_name2;
        public int m_radius;
        
        public startdlg()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            m_name1 = spieler1.Text;
            m_name2 = spieler2.Text;

            if ((bool)small.IsChecked)
            {
                m_radius = 5;
            }
            else if ((bool)medium.IsChecked)
            {
                m_radius = 10;
            }
            else if ((bool)large.IsChecked)
            {
                m_radius = 15;
            }
                       
            DialogResult = true;
        }

        private void spieler1_GotFocus(object sender, RoutedEventArgs e)
        {
            spieler1.Text = "";
        }

        private void spieler2_GotFocus(object sender, RoutedEventArgs e)
        {
            spieler2.Text = "";
        }
    }
}
