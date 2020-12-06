using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlycoSeqClassLibrary.model.protein;

namespace GlycoSeqClassLibrary.util.io
{
    public class FastaReader : IProteinReader
    {
        protected string path;
        public string Path()
        {
            return path;
        }

        public List<IProtein> Read(string path)
        {
            List<IProtein> proteins = new List<IProtein>();
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    string line;
                    StringBuilder sequence = new StringBuilder();
                    IProtein entry = null;
                    // Read lines from the file until end of file (EOD) is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        // ignore comment lines
                        if (line.StartsWith(";"))
                        {
                            continue;
                        }
                        //e.g. >gi|186681228|ref|YP_001864424.1| phycoerythrobilin:ferredoxin oxidoreductase
                        else if (line.StartsWith(">"))
                        {
                            if (entry != null)
                            {
                                entry.SetSequence(sequence.ToString());
                                proteins.Add(entry);
                            }
                            sequence.Clear();
                            entry = new BaseProtein();
                            entry.SetID(line.TrimStart('>'));
                        }
                        else
                        {
                            sequence.Append(line.Trim());
                        }
                    }

                    if (sequence.Length > 0)
                    {
                        entry.SetSequence(sequence.ToString());
                        proteins.Add(entry);
                    }
                }
            }

            return proteins;
        }

        public void SetPath(string path)
        {
            this.path = path;
        }
    }
}
