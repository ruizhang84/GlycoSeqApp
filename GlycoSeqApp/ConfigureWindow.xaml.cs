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

namespace GlycoSeqApp
{
    /// <summary>
    /// Interaction logic for ConfigureWindow.xaml
    /// </summary>
    public partial class ConfigureWindow : Window
    {
        public ConfigureWindow()
        {
            InitializeComponent();
            InitWindow();
        }

        public void InitWindow()
        {
            MS1Tol.Text = SearchingParameters.Access.MS1Tolerance.ToString();
            MSMS2Tol.Text = SearchingParameters.Access.MSMSTolerance.ToString();
            if (SearchingParameters.Access.MS1ToleranceBy 
                == GlycoSeqClassLibrary.algorithm.ToleranceBy.Dalton)
            {
                MS1TolByDalton.IsChecked = true;
            }
            if (SearchingParameters.Access.MS2ToleranceBy
                == GlycoSeqClassLibrary.algorithm.ToleranceBy.Dalton)
            {
                MS2TolByDalton.IsChecked = true;
            }

            foreach (string enzyme in SearchingParameters.Access.DigestionEnzyme)
            {
                switch (enzyme)
                {
                    case "Trypsin":
                        Trypsin.IsChecked = true;
                        break;
                    case "GluC":
                        GluC.IsChecked = true;
                        break;
                    case "Chymotrypsin":
                        Chymotrypsin.IsChecked = true;
                        break;
                    case "Pepsin":
                        Pepsin.IsChecked = true;
                        break;
                }
            }

            if (SearchingParameters.Access.Oxidiatoin)
                Oxidation.IsChecked = true;
            if (SearchingParameters.Access.Deamidation)
                Deamidation.IsChecked = true;

            ThreadNums.Text = SearchingParameters.Access.ThreadNums.ToString();

            DigestionEnzymes.Text = string.Join("+", SearchingParameters.Access.DigestionEnzyme);
            MissCleave.Text = SearchingParameters.Access.MissCleavage.ToString();
            MiniPeptideLength.Text = SearchingParameters.Access.MiniPeptideLength.ToString();

            if (SearchingParameters.Access.ComplexInclude)
                ComplexNGlycan.IsChecked = true;
            if (SearchingParameters.Access.HybridInclude)
                HybridNGlycan.IsChecked = true;
            if (SearchingParameters.Access.MannoseInclude)
                HighMannose.IsChecked = true;
 
            HexNAc.Text = SearchingParameters.Access.HexNAc.ToString();
            Hex.Text = SearchingParameters.Access.Hex.ToString();
            Fuc.Text = SearchingParameters.Access.Fuc.ToString();
            NeuAc.Text = SearchingParameters.Access.NeuAc.ToString();
            NeuGc.Text = SearchingParameters.Access.NeuGc.ToString();

            FDRs.Text = (SearchingParameters.Access.FDRValue * 100.0).ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
            {
                SearchingParameters.Access.Update();
                Close();
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool SaveChanges()
        {
            return SaveTolerance() &&
                    SaveDigestion() &&
                    SaveGlycan() &&
                    SaveOutput() &&
                    SaveScore();
        }

        private bool SaveScore()
        {
            int nums = 6;
            if (int.TryParse(ThreadNums.Text, out nums))
            {
                ConfigureParameters.Access.ThreadNums = nums;
            }
            else
            {
                MessageBox.Show("Thread value is invalid!");
                return false;
            }

            return true;
        }


        private bool SaveTolerance()
        {
            double tol = 20;
            if (double.TryParse(MS1Tol.Text, out tol))
            {
                ConfigureParameters.Access.MS1Tolerance = tol;
            }
            else
            {
                MessageBox.Show("MS tolerance value is invalid!");
                return false;
            }

            if (double.TryParse(MSMS2Tol.Text, out tol))
            {
                ConfigureParameters.Access.MSMSTolerance = tol;
            }
            else
            {
                MessageBox.Show("MSMS tolerance value is invalid!");
                return false;
            }
            return true;
        }

        public bool SaveDigestion()
        {
            int length = 7;
            if (int.TryParse(MissCleave.Text, out length))
            {
                ConfigureParameters.Access.MissCleavage = length;
            }
            else
            {
                MessageBox.Show("HexNAc value is invalid!");
                return false;
            }
            if (int.TryParse(MiniPeptideLength.Text, out length))
            {
                ConfigureParameters.Access.MiniPeptideLength = length;
            }
            else
            {
                MessageBox.Show("HexNAc value is invalid!");
                return false;
            }

            return true;
        }


        private bool SaveGlycan()
        {
            int bound = 12;
            if (int.TryParse(HexNAc.Text, out bound) && bound >= 0)
            {
                ConfigureParameters.Access.HexNAc = bound;
            }
            else
            {
                MessageBox.Show("HexNAc value is invalid!");
                return false;
            }
            if (int.TryParse(Hex.Text, out bound) && bound >= 0)
            {
                ConfigureParameters.Access.Hex = bound;
            }
            else
            {
                MessageBox.Show("HexNAc value is invalid!");
                return false;
            }
            if (int.TryParse(Fuc.Text, out bound) && bound >= 0)
            {
                ConfigureParameters.Access.Fuc = bound;
            }
            else
            {
                MessageBox.Show("Fuc value is invalid!");
                return false;
            }
            if (int.TryParse(NeuAc.Text, out bound) && bound >= 0)
            {
                ConfigureParameters.Access.NeuAc = bound;
            }
            else
            {
                MessageBox.Show("NeuAc value is invalid!");
                return false;
            }
            if (int.TryParse(NeuGc.Text, out bound) && bound >= 0)
            {
                ConfigureParameters.Access.NeuGc = bound;
            }
            else
            {
                MessageBox.Show("NeuGc value is invalid!");
                return false;
            }
            if (ComplexNGlycan.IsChecked == false &&
                HybridNGlycan.IsChecked == false && HighMannose.IsChecked == false)
            {
                MessageBox.Show("Choose at least one NGlycan type!");
                return false;
            }
            else
            {
                ConfigureParameters.Access.ComplexInclude = ComplexNGlycan.IsChecked == true;
                ConfigureParameters.Access.HybridInclude = HybridNGlycan.IsChecked == true;
                ConfigureParameters.Access.MannoseInclude = HighMannose.IsChecked == true; 
            }

            return true;
        }

        private bool SaveOutput()
        {
            double fdr = 1.0;
            if (double.TryParse(FDRs.Text, out fdr) && fdr >= 0 && fdr <= 100)
            {
                ConfigureParameters.Access.FDRValue = fdr * 0.01;
                return true;
            }
            else
            {
                MessageBox.Show("FDR level is invalid!");
                return false;
            }
        }

        private void Trypsin_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfigureParameters.Access.DigestionEnzyme.RemoveAll(x => x == "Trypsin");
            DigestionEnzymes.Text = string.Join("+", ConfigureParameters.Access.DigestionEnzyme);
        }

        private void Trypsin_Checked(object sender, RoutedEventArgs e)
        {
            if (!ConfigureParameters.Access.DigestionEnzyme.Contains("Trypsin"))
                ConfigureParameters.Access.DigestionEnzyme.Add("Trypsin");
            DigestionEnzymes.Text = string.Join("+", ConfigureParameters.Access.DigestionEnzyme);
        }

        private void GluC_UnChecked(object sender, RoutedEventArgs e)
        {
            ConfigureParameters.Access.DigestionEnzyme.RemoveAll(x => x == "GluC");
            DigestionEnzymes.Text = string.Join("+", ConfigureParameters.Access.DigestionEnzyme);
        }

        private void GluC_Checked(object sender, RoutedEventArgs e)
        {
            if (!ConfigureParameters.Access.DigestionEnzyme.Contains("GluC"))
                ConfigureParameters.Access.DigestionEnzyme.Add("GluC");
            DigestionEnzymes.Text = string.Join("+", ConfigureParameters.Access.DigestionEnzyme);
        }


        private void Chymotrypsin_UnChecked(object sender, RoutedEventArgs e)
        {
            ConfigureParameters.Access.DigestionEnzyme.RemoveAll(x => x == "Chymotrypsin");
            DigestionEnzymes.Text = string.Join("+", ConfigureParameters.Access.DigestionEnzyme);
        }

        private void Chymotrypsin_Checked(object sender, RoutedEventArgs e)
        {
            if (!ConfigureParameters.Access.DigestionEnzyme.Contains("Chymotrypsin"))
                ConfigureParameters.Access.DigestionEnzyme.Add("Chymotrypsin");
            DigestionEnzymes.Text = string.Join("+", ConfigureParameters.Access.DigestionEnzyme);
        }


        private void Pepsin_UnChecked(object sender, RoutedEventArgs e)
        {
            ConfigureParameters.Access.DigestionEnzyme.RemoveAll(x => x == "Pepsin");
            DigestionEnzymes.Text = string.Join("+", ConfigureParameters.Access.DigestionEnzyme);
        }

        private void Pepsin_Checked(object sender, RoutedEventArgs e)
        {
            if (!ConfigureParameters.Access.DigestionEnzyme.Contains("Pepsin"))
                ConfigureParameters.Access.DigestionEnzyme.Add("Pepsin");
            DigestionEnzymes.Text = string.Join("+", ConfigureParameters.Access.DigestionEnzyme);
        }

        private void Oxidation_UnChecked(object sender, RoutedEventArgs e)
        {
            ConfigureParameters.Access.Oxidiatoin = false;
        }

        private void Oxidation_Checked(object sender, RoutedEventArgs e)
        {
            ConfigureParameters.Access.Oxidiatoin = true;
        }
        private void Deamidation_UnChecked(object sender, RoutedEventArgs e)
        {
            ConfigureParameters.Access.Deamidation = false;
        }

        private void Deamidation_Checked(object sender, RoutedEventArgs e)
        {
            ConfigureParameters.Access.Deamidation = true;
        }

    }
}
