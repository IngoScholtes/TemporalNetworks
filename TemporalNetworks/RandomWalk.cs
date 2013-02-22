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

        private static Dictionary<string, Dictionary<string, int>> indices_pred = new Dictionary<string, Dictionary<string, int>>();
        private static Dictionary<string, Dictionary<string, int>> indices_succ = new Dictionary<string, Dictionary<string, int>>();
        private static Dictionary<string, double[,]> matrices = new Dictionary<string, double[,]>();

        /// <summary>
        /// Performs a random walk on a weighted static network. Depending on the parameter use_weights, the random walk transition probabilities 
        /// will be biased by the edge weights. 
        /// </summary>
        /// <param name="network">The static, weighted network to use</param>
        /// <param name="use_weights">Whether or not to consider the weights in the random walk transition probabilities</param>
        /// <returns>The number of visited nodes in each time step</returns>
        public static IDictionary<int, int> RunRandomWalk(WeightedNetwork network, bool use_weights = false)
        {
            return Walk(network, use_weights: use_weights, use_bwp: false);
        }

        public static IDictionary<int, int> RunRandomWalkSyntheticBWP(WeightedNetwork network, double min_prob)
        {
            // Make network undirected
            Console.Write("Making network undirected...");
            foreach (string v in network.Vertices)
            {
                var successors = network.GetSuccessors(v);
                var predecessors = network.GetPredecessors(v);
                foreach (string s in successors)
                    if (!predecessors.Contains(s))
                        network.AddEdge(s, v);
                foreach (string p in predecessors)
                    if (!successors.Contains(p))
                        network.AddEdge(v, p);
            }
            Console.WriteLine("done.");

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
            return Walk(network, use_weights: false, use_bwp: true);
        }

        /// <summary>
        /// Performs a random walk on the weighted aggregate network of a temporal network. 
        /// The random walk will preserve the betweenness preference of the given temporal network.
        /// </summary>
        /// <returns>The number of visited nodes in each time step</returns>
        public static IDictionary<int, int> RunRandomWalkBWP(TemporalNetwork temp_net)
        {
            // Compute betweenness preference matrices
            Console.Write("Computing betweenness preference in temporal network...");
            foreach (string x in temp_net.AggregateNetwork.Vertices)
            {
                var ind_p = new Dictionary<string, int>();
                var ind_s = new Dictionary<string, int>();
                matrices[x] = BetweennessPref.GetBetweennessPrefMatrix(temp_net, x, out ind_p, out ind_s, true);
                indices_pred[x] = ind_p;
                indices_succ[x] = ind_s;
            }
            Console.WriteLine("done.");
            return Walk(temp_net.AggregateNetwork, use_weights: false, use_bwp: true);
        }

        /// <summary>
        /// The actual implementation of the random walk on a weighted aggregate network
        /// </summary>
        /// <param name="network"></param>
        /// <param name="use_weights"></param>
        /// <param name="use_bwp"></param>
        /// <returns></returns>
        private static IDictionary<int, int> Walk(WeightedNetwork network, bool use_weights, bool use_bwp)
        {
            Random r = new Random();

            // The nodes that have already been visited 
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            // The number of unique visited nodes per time step
            Dictionary<int, int> uniqueVisitations = new Dictionary<int, int>();
            //var passages = new Dictionary<Tuple<string, string>, int>();

            //foreach (var e in temp_net.AggregateNetwork.Edges)
            //    passages[e] = 0;

            // Use a random node and a random predecessor for initialization
            string v = network.RandomNode;
            string s = network.GetRandomPredecessor(v);
            visited[v] = true;
            int t = 0;
            uniqueVisitations[t] = 1;
            int restarts = 0;

            // Perform random walk, either biased by the weights in the aggregate network or by the betweenness preference matrices
            while (uniqueVisitations[t] < network.VertexCount - 10 && t < 50000)
            {
                t++;

                // The target of this step
                string d = null;

                // If we are not using betweenness preference biasing, then just use the weights
                if (!use_bwp)
                    d = network.GetRandomSuccessor(v, use_weights);
                else
                {
                    // sum the probabilities for all targets from v coming from s
                    double sum = 0d;
                    foreach (string target in network.GetSuccessors(v))
                    {
                        sum = sum + matrices[v][indices_pred[v][s], indices_succ[v][target]];
                    }

                    // If there is nowhere to go from v coming from s (i.e. all entries in the matrix column are zero) generate a new initial node
                    if (sum == 0)
                    {
                        d = network.RandomNode;
                        v = network.GetRandomPredecessor(d);
                        restarts++;
                    }
                    // Sample a target with correct probabilities
                    else
                    {
                        // Create cumulative probabilities
                        double c = 0d;
                        Dictionary<double, string> cumulative = new Dictionary<double, string>();

                        foreach (string target in network.GetSuccessors(v))
                        {
                            double val = matrices[v][indices_pred[v][s], indices_succ[v][target]] / sum;
                            if (val > 0d)
                            {
                                c = c + val;
                                cumulative[c] = target;
                            }
                        }
                        // Sample a node
                        double dice = r.NextDouble();
                        for (int i = 0; i < network.GetOutdeg(v); i++)
                        {
                            if (cumulative.Keys.ElementAt(i) > dice)
                            {
                                d = cumulative.Values.ElementAt(i);
                                break;
                            }
                        }
                    }
                }
                if (!visited.ContainsKey(d))
                    visited[d] = true;
                uniqueVisitations[t] = visited.Keys.Count;
                var edge = new Tuple<string, string>(v, d);
                // passages[edge] = passages[edge] + 1;
                s = v;
                v = d;
                System.Diagnostics.Debug.Assert(v != null && s != null && d != null);
            }
            Console.WriteLine("Restarts = {0}", restarts);
            return uniqueVisitations;
        }        
    }
}
