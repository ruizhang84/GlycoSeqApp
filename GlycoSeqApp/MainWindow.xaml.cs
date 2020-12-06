using Microsoft.Win32;
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

namespace GlycoSeqApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MSMSFileName_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileNameDialog = new OpenFileDialog();
            fileNameDialog.Filter = "Raw File|*.raw";
            fileNameDialog.Title = "Open a MS2 File";

            if (fileNameDialog.ShowDialog() == true)
            {
                Binding fileNameBinding = new Binding();
                fileNameBinding.Path = new PropertyPath("FileName");
                fileNameBinding.Source = fileNameDialog;
                fileNameBinding.Mode = BindingMode.OneWay;
                displayFileName.SetBinding(TextBox.TextProperty, fileNameBinding);
                SearchingParameters.Access.MSMSFile = displayFileName.Text;
            }
        }

        private void FastaFileName_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileNameDialog = new OpenFileDialog();
            fileNameDialog.Filter = "FASTA File|*.fasta";
            fileNameDialog.Title = "Open a Fasta File";

            if (fileNameDialog.ShowDialog() == true)
            {
                Binding fileNameBinding = new Binding();
                fileNameBinding.Path = new PropertyPath("FileName");
                fileNameBinding.Source = fileNameDialog;
                fileNameBinding.Mode = BindingMode.OneWay;
                displayFasta.SetBinding(TextBox.TextProperty, fileNameBinding);
                SearchingParameters.Access.FastaFile = displayFasta.Text;
            }
        }

        private void OutputFileName_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileNameDialog = new SaveFileDialog();
            fileNameDialog.Filter = "CSV Files (*.csv)|*.csv";
            fileNameDialog.Title = "Save Output File";

            if (fileNameDialog.ShowDialog() == true)
            {
                Binding fileNameBinding = new Binding();
                fileNameBinding.Path = new PropertyPath("FileName");
                fileNameBinding.Source = fileNameDialog;
                fileNameBinding.Mode = BindingMode.OneWay;
                displayOutput.SetBinding(TextBox.TextProperty, fileNameBinding);
                SearchingParameters.Access.OutputFile = displayOutput.Text;
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(displayFileName.Text))
            {
                MessageBox.Show("Please choose MS/MS files");
            }
            else if (string.IsNullOrEmpty(displayFasta.Text))
            {
                MessageBox.Show("Please choose Fasta files");

            }
            else if (string.IsNullOrEmpty(displayOutput.Text))
            {
                MessageBox.Show("Please choose Output files");
            }
            else
            {
                Window subWindow = new SearchWindow();
                subWindow.Show();
            }
        }

        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            Window subWindow = new ConfigureWindow();
            subWindow.Show();
        }
    }
}
