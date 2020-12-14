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

        private void MSMSFileNames_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileNamesDialog = new OpenFileDialog();
            fileNamesDialog.Filter = "Raw File|*.raw|MGF File|*.mgf";
            fileNamesDialog.Title = "Open a MS2 File";
            fileNamesDialog.Multiselect = true;
            fileNamesDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (fileNamesDialog.ShowDialog() == true)
            {
                foreach (string filename in fileNamesDialog.FileNames)
                {
                    if (!SearchingParameters.Access.MSMSFiles.Contains(filename))
                    {
                        lbFiles.Items.Add(filename);
                        SearchingParameters.Access.MSMSFiles.Add(filename);
                    }
                }

            }
        }

        private void DeselectFiles_Click(object sender, RoutedEventArgs e)
        {
            if (lbFiles.SelectedItem != null)
            {
                string filename = lbFiles.SelectedItem.ToString();
                lbFiles.Items.Remove(lbFiles.SelectedItem);
                if (SearchingParameters.Access.MSMSFiles.Contains(filename))
                    SearchingParameters.Access.MSMSFiles.Remove(filename);
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

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (SearchingParameters.Access.MSMSFiles.Count == 0)
            {
                MessageBox.Show("Please choose MS/MS files");
            }
            else if (string.IsNullOrEmpty(displayFasta.Text))
            {
                MessageBox.Show("Please choose Fasta files");

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
