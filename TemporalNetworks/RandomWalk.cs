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

        /*
        /// <summary>
        /// Performs a random walk on a weighted static network. Depending on the parameter use_weights, the random walk transition probabilities 
        /// will be biased by the edge weights. 
        /// </summary>
        /// <param name="network">The static, weighted network to use</param>
        /// <param name="use_weights">Whether or not to consider the weights in the random walk transition probabilities</param>
        /// <returns>The number of visited nodes in each time step</returns>
        public static IDictionary<int, int> RunRandomWalk(WeightedNetwork network, bool use_weights = false)
        {            
            return Walk_Random(network, use_weights: use_weights, max_steps: 50000, cutoff: 0.95d);
        }

        public static IDictionary<int, int> RunRandomWalkSyntheticBWP(WeightedNetwork network, double min_prob)
        {           
            // Generate betweenness preference matrices
            Console.Write("Generating synthetic betweenness preference...");
            foreach (string v in network.Vertices)
            {
                if (network.GetOutdeg(v) != network.GetIndeg(v))
                    throw new Exception("\nAt the moment, synthetic BWP can only be generated for undirected networks!");

                indices_pred[v] = new Dictionary<string, int>();
                indices_succ[v] = new Dictionary<string, int>();
                int i = 0;
                foreach (string s in network.GetPredecessors(v))
                    indices_pred[v][s] = i++;
                i = 0;
                foreach (string d in network.GetSuccessors(v))
                    indices_succ[v][d] = i++;

                // All entries will be zero by default
                matrices[v] = new double[network.GetIndeg(v), network.GetOutdeg(v)];
                for (int l = 0; l < network.GetIndeg(v); l++)
                    for (int m = 0; m < network.GetOutdeg(v); m++)
                        matrices[v][l, m] = min_prob;

                // 
                for (int l = 0; l < network.GetIndeg(v); l++)
                    matrices[v][l, l] = 0d;

                // Walk through the diagonal elements 
                for (int k = 0; k<network.GetOutdeg(v); k++)
                {
                    // Is there a high prob. entry in the row? 
                    bool row_set = false;
                    for (int l = 0; l < network.GetOutdeg(v); l++)
                        if (matrices[v][k, l] > min_prob)
                            row_set = true;

                    bool col_set = false;
                    for (int l = 0; l < network.GetOutdeg(v); l++)
                        if (matrices[v][l, k] > min_prob)
                            col_set = true;

                    // Assign values ... 
                    if (!row_set)
                    {
                        int l = rand.Next(0, network.GetOutdeg(v));
                        if (l == k)
                            l = (l + rand.Next(1, network.GetOutdeg(v) - 1)) % network.GetOutdeg(v);
                        matrices[v][k, l] = 1d;
                    }

                    if (!col_set)
                    {
                        int l = rand.Next(0, network.GetOutdeg(v));
                        if (l == k)
                            l = (l + rand.Next(1, network.GetOutdeg(v) - 1)) % network.GetOutdeg(v);
                        matrices[v][l,k] = 1d;
                    }
                }                
            }
            Console.WriteLine("done.");
            return Walk_BWP(network, max_steps: 50000, cutoff: 0.95d);
        }
         * 
         * */


        /// <summary>
        /// Extracts a time-respecting path covering the whole network from a random realization 
        /// of a temporal network with betweenness preference according to a given temporal network
        /// </summary>
        /// <param name="temp_net"></param>
        /// <param name="max_steps"></param>
        /// <param name="cutoff"></param>
        /// <returns></returns>
        public static IDictionary<int, int> RunRW(TemporalNetwork temp_net, out string firstPassageMatrix, int max_steps = 100000, double cutoff = 1d, bool null_model = false)
        {
            Random r = new Random();
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            Dictionary<int, int> uniqueVisitations = new Dictionary<int, int>();
            Dictionary<string, Dictionary<double, string>> cumulatives = new Dictionary<string, Dictionary<double, string>>();
            Dictionary<string, double> sums = new Dictionary<string, double>();

            Dictionary<string, Dictionary<string, int>> indices_pred = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> indices_succ = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, double[,]> matrices = new Dictionary<string, double[,]>();            

            // Aggregate network
            WeightedNetwork network = temp_net.AggregateNetwork;            

            // Compute betweenness preference matrices
            Console.Write("Computing betweenness preference in temporal network...");
            foreach (string x in network.Vertices)
            {
                var ind_p = new Dictionary<string, int>();
                var ind_s = new Dictionary<string, int>();
                if (null_model)
                {
                    matrices[x] = BetweennessPref.GetUncorrelatedBetweennessPrefMatrix(temp_net, x, out ind_p, out ind_s);
                    Dictionary<string,int> i_p, i_s = null;
                    double[,] m = BetweennessPref.GetBetweennessPrefMatrix(temp_net, x, out i_p, out i_s, false);
                    double count = 0d;
                    foreach (string p in i_p.Keys)
                        foreach (string s in i_s.Keys)
                            count += m[i_p[p], i_s[s]];
                    foreach (string p in ind_p.Keys)
                        foreach (string s in ind_s.Keys)
                            matrices[x][ind_p[p], ind_s[s]] = matrices[x][ind_p[p], ind_s[s]] * count;
                }
                else
                    matrices[x] = BetweennessPref.GetBetweennessPrefMatrix(temp_net, x, out ind_p, out ind_s, false);
                indices_pred[x] = ind_p;
                indices_succ[x] = ind_s;
            }
            foreach (string x in network.Vertices)
            {
                double c;
                Dictionary<double, string> cumulative = CreateCumulative(network, x,  out c, matrices, indices_pred, indices_succ);
                sums[x] = c;
                cumulatives[x] = cumulative;
            }
            Console.WriteLine("done.");

            Dictionary<string, int> indices = new Dictionary<string, int>();
            int j = 0;

            foreach (string node in network.Vertices)
                indices[node] = j++;


            int[,] firstPassageTimes = new int[network.VertexCount, network.VertexCount];

            foreach (string node1 in network.Vertices)
                foreach (string node2 in network.Vertices)
                    firstPassageTimes[indices[node1], indices[node2]] = -1;

            foreach (var start in network.Vertices)
            {
                visited = new Dictionary<string, bool>();

                

                // Initialize random walker                
                string v = start;
                visited[v] = true;
                firstPassageTimes[indices[v], indices[v]] = 0;

                int t = 0;
                uniqueVisitations[t] = 1;

                // Create a time-respecting path starting at v
                while (uniqueVisitations[t] < cutoff * network.VertexCount && t < max_steps)
                {
                    double sample = rand.NextDouble() * sums[v];
                    string sampled_path = null;
                    for (int i = 0; i < cumulatives[v].Count; i++)
                    {
                        if (cumulatives[v].Keys.ElementAt(i) > sample)
                        {
                            sampled_path = cumulatives[v].Values.ElementAt(i);
                            break;
                        }
                    }
                    if (sampled_path == null)
                        break;

                    // Now we have the path ... 
                    string[] comps = sampled_path.Split(',');

                    // v is the first node of a two path, so we advance two steps ... 
                    if (comps.Length == 3)
                    {
                        t++;
                        if (!visited.ContainsKey(comps[1]))
                        {
                            visited[comps[1]] = true;
                            firstPassageTimes[indices[start], indices[comps[1]]] = t;
                        }
                        uniqueVisitations[t] = visited.Keys.Count;

                        t++;

                        if (!visited.ContainsKey(comps[2]))
                        {
                            visited[comps[2]] = true;
                            firstPassageTimes[indices[start], indices[comps[2]]] = t;
                        }
                        uniqueVisitations[t] = visited.Keys.Count;
                        v = comps[2];
                    }
                    // v is the second node of a two path, so we advance by one step ... 
                    else if (comps.Length == 2)
                    {
                        t++;

                        if (!visited.ContainsKey(comps[1]))
                        {
                            visited[comps[1]] = true;
                            firstPassageTimes[indices[start], indices[comps[1]]] = t;
                        }
                        uniqueVisitations[t] = visited.Keys.Count;
                        v = comps[1];
                    }
                    else
                        throw new Exception("This will never happen! If you see this, I was wrong :-)");

                }
            }

            StringBuilder sb = new StringBuilder();

            var nodes = from n in temp_net.AggregateNetwork.Vertices.AsParallel() orderby n ascending select n;

            foreach (string s in nodes)
            {
                Console.WriteLine(s);
                foreach (string d in nodes)
                {
                    sb.Append(firstPassageTimes[indices[s], indices[d]] + "\t");
                }
                sb.Append("\n");
            }
            firstPassageMatrix = sb.ToString();

            return uniqueVisitations;
        }

        private static Dictionary<double, string> CreateCumulative(WeightedNetwork network, string v, out double c, Dictionary<string, double[,]> matrices, Dictionary<string,Dictionary<string, int>> indices_pred, Dictionary<string,Dictionary<string, int>> indices_succ)
        {
            // Pick a random two path from those where v is either the first or the second node ...
            var cumulative = new Dictionary<double, string>();
            c = 0d;

            // Collect all two-paths (s, _v_ ,d)            
            /*
            foreach (string d in network.GetSuccessors(v))
            {
                double val = 0d;
                foreach (string s in network.GetPredecessors(v))
                   val += matrices[v][indices_pred[v][s], indices_succ[v][d]];
                if (val > 0d)
                {
                    c = c + val;
                    cumulative[c] = v + "," + d;
                }
            }
             * */

            // Two paths (_v_, x, y)
            foreach (string x in network.GetSuccessors(v))
            {
                foreach (string y in network.GetSuccessors(x))
                {
                    double val = matrices[x][indices_pred[x][v], indices_succ[x][y]];
                    if (val > 0d)
                    {
                        c = c + val;
                        cumulative[c] = v + "," + x + "," + y;
                    }
                }
            }
            return cumulative;
        }


        public static Dictionary<int, int> RunRW(WeightedNetwork network, int max_steps = 100000, double cutoff = 1d, bool use_weights = true)
        {
            Random r = new Random();
            Dictionary<int, int>  uniqueVisitations = new Dictionary<int, int>();

            // The nodes that have already been visited 
            Dictionary<string, bool> visited = new Dictionary<string, bool>();

            // Use a node for initialization
            string v = network.RandomNode;
            visited[v] = true;
            int t = 0;

            uniqueVisitations[t] = 1;

            // Perform random walk, either biased by the weights in the aggregate network or by the betweenness preference matrices
            while (uniqueVisitations[t] < cutoff * network.VertexCount && t < max_steps)
            {                                
                v = network.GetRandomSuccessor(v, use_weights);

                if (!visited.ContainsKey(v))
                    visited[v] = true;
                t++;
                uniqueVisitations[t] = visited.Keys.Count;
            }
            return uniqueVisitations;
        }
    }
}
