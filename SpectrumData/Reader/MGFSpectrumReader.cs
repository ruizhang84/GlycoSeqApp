using SpectrumData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SpectrumData.Spectrum;

namespace SpectrumData.Reader
{
    public class MGFSpectrumReader : ISpectrumReader
    {

        //Regex start = new Regex("BEGIN\\s+IONS", RegexOptions.Compiled);
        //Regex end = new Regex("END\\s+IONS", RegexOptions.Compiled);
        //Regex title = new Regex("TITLE=(.*)", RegexOptions.Compiled);
        Regex pepmass = new Regex("PEPMASS=(\\d+\\.?\\d*)", RegexOptions.Compiled);
        Regex charge = new Regex("CHARGE=(\\d+)", RegexOptions.Compiled);
        Regex rt_second = new Regex("RTINSECONDS=(\\d+\\.?\\d*)", RegexOptions.Compiled);
        Regex scan = new Regex("SCANS=(\\d+)", RegexOptions.Compiled);
        Regex mz_intensity = new Regex("^(\\d+\\.?\\d*)\\s+(\\d+\\.?\\d*)", RegexOptions.Compiled);

        Dictionary<int, MS2Spectrum> spectrumTable =
            new Dictionary<int, MS2Spectrum>();

        public Dictionary<int, MS2Spectrum> GetSpectrum()
        {
            return spectrumTable;
        }

        public void Init(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    string line;
                    List<IPeak> peaks = new List<IPeak>();
                    int scan_num = 0;
                    double rention_time = 0;
                    double precursor_mz = 0;
                    int precursor_charge = 0;

                    // Read lines from the file until end of file (EOD) is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("BEGIN"))
                        {
                            peaks = new List<IPeak>();
                        }
                        else if (line.StartsWith("END"))
                        {
                            MS2Spectrum spectrum = new MS2Spectrum(scan_num, rention_time, 
                                precursor_mz, precursor_charge);
                            spectrum.SetPeaks(peaks);
                            spectrumTable[scan_num] = spectrum;
                        }
                        else if (scan.IsMatch(line))
                        {
                            MatchCollection matches = scan.Matches(line);
                            foreach (Match match in matches)
                            {
                                GroupCollection groups = match.Groups;
                                scan_num =  int.Parse(groups[1].Value);
                                break;
                            }
                        }
                        else if (rt_second.IsMatch(line))
                        {
                            MatchCollection matches = rt_second.Matches(line);
                            foreach (Match match in matches)
                            {
                                GroupCollection groups = match.Groups;
                                rention_time = double.Parse(groups[1].Value);
                                break;
                            }
                        }
                        else if (pepmass.IsMatch(line))
                        {
                            MatchCollection matches = pepmass.Matches(line);
                            foreach (Match match in matches)
                            {
                                GroupCollection groups = match.Groups;
                                precursor_mz = double.Parse(groups[1].Value);
                                break;
                            }
                        }
                        else if (charge.IsMatch(line))
                        {
                            MatchCollection matches = charge.Matches(line);
                            foreach (Match match in matches)
                            {
                                GroupCollection groups = match.Groups;
                                precursor_charge = int.Parse(groups[1].Value);
                                break;
                            }
                        }
                        else if (mz_intensity.IsMatch(line))
                        {
                            MatchCollection matches = mz_intensity.Matches(line);
                            foreach (Match match in matches)
                            {
                                GroupCollection groups = match.Groups;
                                peaks.Add(new GeneralPeak(double.Parse(groups[1].Value), 
                                    double.Parse(groups[2].Value)));
                                break;
                            }
                        }
                    }
                }
            }
        }


        public int GetFirstScan()
        {
            return spectrumTable.Keys.Min();
        }

        public int GetLastScan()
        {
            return spectrumTable.Keys.Max();
        }

        public int GetMSnOrder(int scanNum)
        {
            return 2;
        }

        public double GetPrecursorMass(int scanNum, int msOrder)
        {
            if (!spectrumTable.ContainsKey(scanNum) || msOrder != 2)
            {
                return -1;
            }
            return spectrumTable[scanNum].PrecursorMZ();
        }

        public double GetRetentionTime(int scanNum)
        {
            if (!spectrumTable.ContainsKey(scanNum))
            {
                return -1;
            }
            return spectrumTable[scanNum].GetRetention();
        }

        public ISpectrum GetSpectrum(int scanNum)
        {
            if (!spectrumTable.ContainsKey(scanNum))
            {
                return new GeneralSpectrum(scanNum, 0);
            }
            return spectrumTable[scanNum];
        }
    }
}
