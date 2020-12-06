﻿using GlycoSeqClassLibrary.algorithm;
using GlycoSeqClassLibrary.model.glycan;
using SpectrumData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.search
{
    public class GlycanSearch
    {
        ISearch<int> searcher_;
        Dictionary<string, IGlycan> glycans_map_;
        readonly string kY1 = "1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 ";
        const int kMissing = 5;
        Dictionary<string, double> peptide_mass_;

        public GlycanSearch(ISearch<int> searcher,
            Dictionary<string, IGlycan> glycans_map)
        {
            searcher_ = searcher;
            glycans_map_ = glycans_map;
        }

        // peptide seq, glycan*
        public Dictionary<string, HashSet<int>> Search(List<IPeak> peaks, int max_charge,
            Dictionary<string, List<IGlycan>> candidates)
        {
            // init search engine
                InitSearch(peaks, max_charge);

            // init peak nodes
            Dictionary<double, PeakNode> peak_nodes_map = new Dictionary<double, PeakNode>();
            PriorityQueue<PeakNode> queue = new PriorityQueue<PeakNode>();
            InitPriorityQueue(candidates, peak_nodes_map, queue);

            // dp
            var dp_results = DynamicProgramming(peaks, peak_nodes_map, queue);

            // filter results
            Dictionary<string, HashSet<int>> results = new Dictionary<string, HashSet<int>>();
            foreach(var node in dp_results)
            {
                Dictionary<string, Dictionary<string, HashSet<int>>> peakMatch = node.Matches();
                foreach (var peptide in peakMatch.Keys)
                {
                    List<IGlycan> candidate_glycan = candidates[peptide];
                    foreach(var g in peakMatch[peptide].Keys)
                    {
                        foreach(var glycan in candidate_glycan)
                        {
                            if (Satisify(g, glycan))
                            {
                                string key = SearchHelper.MakeKeyGlycoSequence(glycan.ID(), peptide);
                                if(!results.ContainsKey(key))
                                {
                                    results[key] = new HashSet<int>();
                                }
                                results[key].UnionWith(peakMatch[peptide][g]);
                            }
                        }

                    }
                }
            }

            return results;
        }

    bool Satisify(string identified_glycan_id, IGlycan glycan)
    {
        List<int> identified_glycan_table = glycans_map_[identified_glycan_id].Table().ToList();
        List<int> candidate_glycan_table = glycan.Table().ToList();
        for(int i = 0; i< identified_glycan_table.Count; i++)
        {
            if (candidate_glycan_table[i] < identified_glycan_table[i])
                return false;
        }
        return true;
    }

    double ComputePeptideMass(string seq)
    {
        if (!peptide_mass_.ContainsKey(seq))
        {
            peptide_mass_[seq] = util.mass.Peptide.To.Compute(seq);
        }
        return peptide_mass_[seq];
    }


    List<PeakNode> DynamicProgramming(List<IPeak> peaks, Dictionary<double, PeakNode> peak_nodes_map,
        PriorityQueue<PeakNode> queue)
    {
        List<PeakNode> matched_nodes = new List<PeakNode>();
        while (queue.Count > 0)
        {
            // get node
            PeakNode node = queue.Dequeue();

            // // match peaks
            double target = node.Mass();
            List<int> matched = searcher_.Search(target);

            // max if matched a peak
            node.Max(peaks);

            // update matches
            if (matched.Count > 0)
            {
                node.Add(matched);
                node.set_miss(0);
                matched_nodes.Add(node);
            }

            if (node.Missing() > kMissing)
                continue;

                // extending queue
            Dictionary<string, Dictionary<string, HashSet<int>>> peakMatch = node.Matches();
            foreach (var peptide in peakMatch.Keys)
            {
                foreach (var glycan_id in peakMatch[peptide].Keys)
                {
                    List<int> peak_indexes = peakMatch[peptide][glycan_id].ToList();

                    IGlycan glycan = glycans_map_[glycan_id];
                    foreach (var g in glycan.Children())
                    {
                        double mass = g.Mass() + util.mass.Peptide.To.Compute(peptide);
                        if (!peak_nodes_map.ContainsKey(mass))
                        {
                            PeakNode next = new PeakNode();
                            // set mass
                            next.set_mass(mass);
                            // set matches
                            next.Add(peptide, g.ID(), peak_indexes);
                            // set missing
                            next.set_miss(node.Missing() + 1);
                            // add node 
                            peak_nodes_map[mass] = next;
                            // enqueue
                            queue.Enqueue(mass, peak_nodes_map[mass]);
                        }
                        else
                        {
                            // std::cout << "here" << std::endl;
                            PeakNode next = peak_nodes_map[mass];
                            // set missing
                            next.set_miss(Math.Min(next.Missing(), node.Missing() + 1));
                            // set matches
                            next.Add(peptide, g.ID(), peak_indexes);
                        }
                    }
                }
            }
        }
        return matched_nodes;
    }

    void InitPriorityQueue(Dictionary<string, List<IGlycan>> candidate,
        Dictionary<double, PeakNode> peak_nodes_map, PriorityQueue<PeakNode> queue)
    {
        foreach (var it in candidate.Keys)
        {
            // Y1 mass
            double mass = ComputePeptideMass(it) + util.mass.Glycan.kHexNAc;
            // node matches
            if (!peak_nodes_map.ContainsKey(mass))
            {
                PeakNode node = new PeakNode();
                // set mass
                node.set_mass(mass);
                // set matches
                node.Add(it, kY1, new List<int>());
                // add node 
                peak_nodes_map[mass] = node;
                // enqueue
                queue.Enqueue(mass, peak_nodes_map[mass]);
            }
            else
            {
                // update glycopeptide match
                peak_nodes_map[mass].Add(it, kY1, new List<int>());
            }
        }
    }


        void InitSearch(List<IPeak> peaks, int max_charge)
        {
            List<Point<int>> peak_points = new List<Point<int>>();
            for (int i = 0; i < peaks.Count; i++)
            {
                IPeak peak = peaks[i];
                for (int charge = 1; charge <= max_charge; charge++)
                {
                    double mass = util.mass.Spectrum.To.Compute(peak.GetMZ(), charge);
                    Point<int> point = new Point<int>(mass, i);
                    peak_points.Add(point);
                }
            }
            searcher_.Init(peak_points);
        }



    }
}