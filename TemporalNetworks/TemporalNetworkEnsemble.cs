using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TemporalNetworks
{
    /// <summary>
    /// This class contains a number of static methods to create random temporal networks with given betweenness preference
    /// </summary>
    public class TemporalNetworkEnsemble
    {
        /// <summary>
        /// The random number generator to use
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// Reinitializes the internal random generator with a specific seed
        /// </summary>
        /// <param name="seed"></param>
        public static void InitializeRandomGenerator(int seed)
        {
            rand = new Random(seed);
        }       

        /// <summary>
        /// This method creates a random sequence of two paths, where the betweenness preferences as well as edge weights match those of a given temporal network. 
        /// The model implemented here can be viewd in two different ways: 
        /// 1.) It can be seen as a reshuffling of two paths realized in a given temporal network, while destroying other correlations like bursty activity patterns
        /// 2.) Alternatively, it can be seen as a random sampling based on the betweenness preference matrices of nodes as computed from an empirical network
        /// </summary>
        /// <param name="temp_net">The temporal network based on which a randomized sequence of two paths will be created</param>
        /// <param name="length">The length of the sequence in terms of the number of time-stamped interactions</param>
        /// <param name="precision">The numerical precision that will be used when sampling from the distribution. This
        /// at the same time affects the memory requirements of the procedure, which is at most precision*two_paths in the input network</param>
        /// <returns>A temporal network that preserves betweenness preferences as well as the aggregated network of the input</returns>
        public static TemporalNetwork ShuffleTwoPaths(TemporalNetwork temp_net, int length=0, int precision=1000)
        {
            // If no length is specified (i.e. length has the default value of 0), use the length of the original sequence
            length = length > 0 ? length : temp_net.Length;

            // This will take the betweenness pref. matrices of all nodes ... 
            Dictionary<string, double[,]> betweenness_matrices = new Dictionary<string, double[,]>();

            Dictionary<string, Dictionary<string, int>> pred_indices = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> succ_indices = new Dictionary<string, Dictionary<string, int>>();

            int lcm = 1;

            // Compute unnormalized betweenness preference matrices of all nodes in the aggregate network
            foreach (string x in temp_net.AggregateNetwork.Vertices)
            {
                Dictionary<string, int> preds = new Dictionary<string, int>();
                Dictionary<string, int> succs = new Dictionary<string, int>();
                betweenness_matrices[x] = BetweennessPref.GetBetweennessPrefMatrix(temp_net, x, out preds, out succs, normalized: false);
                pred_indices[x] = preds;
                succ_indices[x] = succs;

                // Compute the least common multiple of the denominators of all entries ... 
                // Eventually we will multiply ALL entries of the matrices of ALL nodes with the LCM in order to 
                // ensure that all two paths are represented with the correct relative frequencies
                foreach (string s in temp_net.AggregateNetwork.GetPredecessors(x))
                    foreach (string d in temp_net.AggregateNetwork.GetSuccessors(x))
                    {
                        int whole, numerator, denominator;
                        MathHelper.RoundToMixedFraction(betweenness_matrices[x][pred_indices[x][s], succ_indices[x][d]], precision, out whole, out numerator, out denominator);
                        lcm = MathHelper.LCM(lcm, denominator);
                    }
            }

            List<string> sampling_set = new List<string>();

            // Create a list of two_paths whose relative frequencies match the betweenness preference matrix... 
            foreach (string v in betweenness_matrices.Keys)
            {   
                // Add source and target of two paths to list according to their relative frequencies in the matrices
                foreach (string s in temp_net.AggregateNetwork.GetPredecessors(v))
                    foreach (string d in temp_net.AggregateNetwork.GetSuccessors(v))
                        for (int k = 0; k < Math.Round(betweenness_matrices[v][pred_indices[v][s], succ_indices[v][d]] * lcm); k++)
                            sampling_set.Add(s + "," + v + "," + d);
            }

            // Create an empty temporal network
            TemporalNetwork microstate = new TemporalNetwork();

            int time = 0;

            // Draw two-paths at random from the sampling set, this is equivalent to reshuffling existing two paths of the original sequence
            // However, it additionally correctly accounts for continued two paths (in which case an edge overlaps) and for multiple edges in
            // a single step (such two paths will be counted fractionally)
            for (int l = 0; l < length/2; l++)
            {               
                // Draw a random two path
                int r = rand.Next(sampling_set.Count);
                string tp = sampling_set[r];
                string[] nodes = tp.Split(',');

                // Add two temporal edges
                microstate.AddTemporalEdge(time++, nodes[0], nodes[1]);
                microstate.AddTemporalEdge(time++, nodes[1], nodes[2]);
            }
            return microstate;
        }
  

        /// <summary>
        /// Creates a random temporal network by shuffling the edges present in an original weighted network
        /// </summary>
        /// <param name="net">The weighted network to draw the microstate from</param>
        /// <param name="length">The length of the sequence in terms of the number of time-stamped interactions</param>
        /// <returns></returns>
        public static TemporalNetwork ShuffleEdges(TemporalNetwork net, int length=0, int precision = 1000)
        {
            // If no length is specified (i.e. length has the default value of 0), use the length of the original network
            length = length > 0 ? length : (int) net.AggregateNetwork.CumulativeWeight;

            int lcm = 1;            


            foreach (var twopath in net.TwoPathWeights.Keys)
            {
                    int whole, numerator, denominator;
                    MathHelper.RoundToMixedFraction(net.TwoPathWeights[twopath], precision, out whole, out numerator, out denominator);
                    lcm = MathHelper.LCM(lcm, denominator);
            }

            // Collect edges in two sampling sets for in and outgoing edges of two paths
            List<Tuple<string, string>> in_edges = new List<Tuple<string, string>>();
            List<Tuple<string, string>> out_edges = new List<Tuple<string, string>>();

            string[] split = null;

            foreach (var twopath in net.TwoPathWeights.Keys)
                for (int k = 0; k < Math.Round(net.TwoPathWeights[twopath] * lcm); k++)
                {
                    split = twopath.Split(',');
                    in_edges.Add(new Tuple<string,string>(split[0],split[1]));
                    out_edges.Add(new Tuple<string, string>(split[1], split[2]));
                }

            TemporalNetwork output = new TemporalNetwork();
            int l = 0;
            while (l < length)
            {
                // Draw edges uniformly at random and add them to temporal network
                var edge1 = in_edges.ElementAt(rand.Next(in_edges.Count));
                Tuple<string,string> edge2 = null;
                while (edge2 == null)
                {
                    edge2 = out_edges.ElementAt(rand.Next(out_edges.Count));
                    if(edge1.Item2 != edge2.Item1)
                        edge2 = null;
                }

                // Add to the output network
                output.AddTemporalEdge(l++, edge1.Item1, edge1.Item2);
                output.AddTemporalEdge(l++, edge2.Item1, edge2.Item2);
            }
            return output;
        }
    }
}
