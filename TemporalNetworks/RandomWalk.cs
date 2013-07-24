using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalNetworks
{
    public class RandomWalk
    {
        /// <summary>
        /// A random generator used for the spreading
        /// </summary>
        private static Random rand = new Random();


        public static IDictionary<int, double> RunRW_BWP(TemporalNetwork temp_net, int max_steps = 100000, bool null_model = false)
        {
            Random r = new Random();

            var cumulatives = new Dictionary<Tuple<string,string>, Dictionary<double, string>>();
            var sums = new Dictionary<Tuple<string, string>, double>();

            // Dictionary<string, Dictionary<double, string>> cumulatives = 
            // Dictionary<string, double> sums = new Dictionary<string, double>();

            Dictionary<string, Dictionary<string, int>> indices_pred = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> indices_succ = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, double[,]> matrices = new Dictionary<string, double[,]>();

            Dictionary<string, int> visitations = new Dictionary<string, int>();
            Dictionary<string, double> stationary = new Dictionary<string, double>();

            Dictionary<Tuple<string, string>, int> edge_visitations = new Dictionary<Tuple<string, string>, int>();
            Dictionary<Tuple<string, string>, double> edge_stationary = new Dictionary<Tuple<string, string>, double>();
            
            Dictionary<int, double> tvd = new Dictionary<int, double>();

            // Aggregate network
            WeightedNetwork network = temp_net.AggregateNetwork;

            // Read analytical stationary distribution (i.e. flow-corrected edge weights) from disk
            string[] lines = System.IO.File.ReadAllLines("stationary_dist_RM.dat");
            foreach (string x in lines)
            {
                string[] split = x.Split(' ');
                string[] nodes = split[0].Split('.');
                var edge = new Tuple<string,string>(nodes[0], nodes[1]);

                // Extract stationary dist, set visitations to zero and adjust edge weights ... 
                edge_stationary[edge] = double.Parse(split[1], System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
                edge_visitations[edge] = 0;
                network[edge] = edge_stationary[edge];
            }            

            // Compute stationary dist of vertices ... 
            double total = 0d;
            foreach(string x in network.Vertices)
            {
                stationary[x] = 0d;
                foreach(string s in network.GetPredecessors(x))
                    stationary[x] += edge_stationary[new Tuple<string,string>(s,x)];
                total += stationary[x];
            }
            foreach (string x in network.Vertices)
                stationary[x] = stationary[x] / total;    
                  

            // Compute betweenness preference matrices
            if(!null_model)
                Console.Write("Computing betweenness preference in temporal network ...");
            else
                Console.Write("Calculating null model betweenness preference ...");
            foreach (string x in network.Vertices)
            {
                var ind_p = new Dictionary<string, int>();
                var ind_s = new Dictionary<string, int>();
                if(!null_model)                                 
                    matrices[x] = BetweennessPref.GetBetweennessPrefMatrix(temp_net, x, out ind_p, out ind_s, false);
                else                
                    matrices[x] = BetweennessPref.GetUncorrelatedBetweennessPrefMatrix(temp_net, x, out ind_p, out ind_s);

                indices_pred[x] = ind_p;
                indices_succ[x] = ind_s;
            }
            Console.WriteLine("done.");

            // Initialize visitations, stationary distribution and cumulatives ... 
            foreach (string x in network.Vertices)
            {
                visitations[x] = 0;
                stationary[x] = 0d;

                foreach(string s in indices_pred[x].Keys)
                {
                    Tuple<string, string> key = new Tuple<string,string>(s, x);

                    stationary[x] += network.GetWeight(s,x);

                    // Compute the transition probability for a edge (x,t) given that we are in (s,x)
                    cumulatives[key] = new Dictionary<double, string>();
                    double sum = 0d;

                    foreach (string t in indices_succ[x].Keys)
                    {
                        double transition_prob = 0d; 

                        string two_path = s + "," + x + "," + t;
                        transition_prob = matrices[x][indices_pred[x][s], indices_succ[x][t]];

                        if (transition_prob > 0)
                        {
                            sum += transition_prob;
                            cumulatives[key][sum] = t;
                        }
                    }
                    sums[key] = sum;
                }
            }            
            
            // Draw two initial nodes ... 
            string pred = network.RandomNode;
            string current = network.GetRandomSuccessor(pred);
            
            visitations[pred] = 1;
            visitations[current] = 1;
            edge_visitations[new Tuple<string,string>(pred,current)] = 1;

            // Run the random walk (over edges) 
            for (int t = 0; t < max_steps; t++)
            {
                // The edge via which we arrived at the current node
                Tuple<string, string> current_edge = new Tuple<string, string>(pred, current);

                // If this happens, we are stuck in a sink, i.e. there is no out edge
                System.Diagnostics.Debug.Assert(sums[current_edge]>0, string.Format("Network not strongly connected! RW stuck after passing through edge {0}", current_edge));

                // Draw a sample uniformly from [0,1] and multiply it with the cumulative sum for the current edge ...
                double sample = rand.NextDouble() * sums[current_edge];

                // Determine the next transition ... 
                string next_node = null;               
                for (int i = 0; i < cumulatives[current_edge].Count; i++)
                {
                    if (cumulatives[current_edge].Keys.ElementAt(i) > sample)
                    {
                        next_node = cumulatives[current_edge].Values.ElementAt(i);
                        break;
                    }
                }
                pred = current;
                current = next_node;
                
                visitations[current] = visitations[current] + 1;
                edge_visitations[new Tuple<string, string>(pred, current)] = edge_visitations[new Tuple<string, string>(pred, current)] + 1;

                tvd[t] = TVD(visitations, stationary);
            }
            return tvd;
        }

        public static double TVD(Dictionary<string, int> visitations, Dictionary<string, double> stationary)
        {
            double total = 0d;

            foreach (string x in visitations.Keys)
                total += visitations[x];           

            double tvd = 0d;
            foreach (string x in visitations.Keys)
                tvd += Math.Abs( (visitations[x]/total) - stationary[x]);
            return 0.5d * tvd;
        }       
    }
}
