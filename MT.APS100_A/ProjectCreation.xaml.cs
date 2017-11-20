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
using System.Windows.Shapes;
using MT.APS100.Service;

namespace MT.APS100_A
{
    /// <summary>
    /// Interaction logic for ProjectCreation.xaml
    /// </summary>
    public partial class ProjectCreation : Window
    {
        public ProjectCreation()
        {
            InitializeComponent();
        }

        private void Btn_Create_Click(object sender, RoutedEventArgs e)
        {
            CreateProject.CreateFolderStructure(Txt_Project.Text);
            TxtCreatedProject.IsEnabled = true;
            BtnProjectConfig.IsEnabled = true;
            TxtCreatedProject.Text = Txt_Project.Text;
            Txt_Project.Text = string.Empty;
        }

        private void BtnProjectConfig_Click(object sender, RoutedEventArgs e)
        {
            MTConfig mtconfig = new MTConfig();
            mtconfig.LblSelectedProject.Content = TxtCreatedProject.Text;
            mtconfig.ShowDialog();
        }
    }
}
