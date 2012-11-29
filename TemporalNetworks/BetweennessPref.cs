using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemporalNetworks
{
    /// <summary>
    /// This class provides static methods to compute both betweenness preference matrices 
    /// as well as betweenness preference of nodes in temporal networks
    /// </summary>
    public class BetweennessPref
    {        
        /// <summary>
        /// Some states that will be used by the finite state machine that is used to extract two-paths
        /// </summary>
        private enum CollectState
        {
            Collect_From = 0, Collect_To = 1, Start = 2
        }

        /// <summary>
        /// Computes the baseline betweenness preference matrix of a node under the assumption 
        /// that the temporal network does not contain a betweenness preference correlation. This corresponds to 
        /// equation (5) in the paper.
        /// </summary>
        /// <param name="v">The node to compute the baseline betweenness preference for</param>
        /// <param name="aggregate_net">The weighted, aggregate ego network of node x based on which the matrix will be computed</param>
        /// <param name="index_pred">Indices of predecessor nodes in the betweenness preference matrix</param>
        /// <param name="index_succ">Indices of successor nodes in the betweenness preference matric</param>
        /// <param name="normalize">Whether or not to normalize the betweenness preference matrix (i.e. whether B or P shall be returned)</param>
        /// <returns>Depending on the normalization, a betweenness preference matrix B or the normalized version P will be returned</returns>
        public static double[,] GetUncorrelatedBetweennessPrefMatrix(WeightedNetwork aggregate_net, string v, out Dictionary<string, int> index_pred, out Dictionary<string, int> index_succ)
        {
            // Use a mapping of indices to node labels
            index_pred = new Dictionary<string, int>();
            index_succ = new Dictionary<string, int>();

            // Create an empty matrix
            double[,] P = new double[aggregate_net.GetIndeg(v), aggregate_net.GetOutdeg(v)];

            // Create the index-to-node mapping
            int i = 0;
            foreach (string u in aggregate_net.GetPredecessors(v))
                index_pred[u] = i++;

            i = 0;
            foreach (string w in aggregate_net.GetSuccessors(v))
                index_succ[w] = i++;

            // Sum over the weights of all source nodes
            double sum_source_weights = 0d;
            foreach (string s_prime in aggregate_net.GetPredecessors(v))
                sum_source_weights += aggregate_net.GetWeight(s_prime, v);

            // Normalization factor for d
            double sum_dest_weights = 0d;
            foreach (string d_prime in aggregate_net.GetSuccessors(v))
                sum_dest_weights += aggregate_net.GetWeight(v, d_prime);           

            double min_p = double.MaxValue;

            // Equation (5) in the paper
            foreach (string s in aggregate_net.GetPredecessors(v))
                foreach (string d in aggregate_net.GetSuccessors(v))
                {
                    P[index_pred[s], index_succ[d]] = (aggregate_net.GetWeight(s,v) / sum_source_weights)  * (aggregate_net.GetWeight(v,d) / sum_dest_weights);

                    min_p = Math.Min(P[index_pred[s], index_succ[d]], min_p);

                }
            return P;         
        }

        /// <summary>
        /// Computes the baseline betweenness preference matrix of a node under the assumption 
        /// that the temporal network does not contain a betweenness preference correlation. This corresponds to 
        /// equation (5) in the paper.
        /// </summary>
        /// <param name="temp_net">The temporal network for which to compute the matrix</param>
        /// <param name="x">The node to compute the baseline betweenness preference for</param>
        /// <param name="ego_net">The weighted, aggregate ego network of node x</param>
        /// <param name="index_pred">Indices of predecessor nodes in the betweenness preference matrix</param>
        /// <param name="index_succ">Indices of successor nodes in the betweenness preference matric</param>
        /// <param name="normalize">Whether or not to normalize the betweenness preference matrix (i.e. whether B or P shall be returned)</param>
        /// <returns>Depending on the normalization, a betweenness preference matrix B or the normalized version P will be returned</returns>
        public static double[,] GetUncorrelatedBetweennessPrefMatrix(TemporalNetwork temp_net, string x, out Dictionary<string, int> index_pred, out Dictionary<string, int> index_succ)
        {
            // Use a mapping of indices to node labels
            index_pred = new Dictionary<string, int>();
            index_succ = new Dictionary<string, int>();
            
            // Compute the matrix from the weighted ego network
            return GetUncorrelatedBetweennessPrefMatrix(temp_net.AggregateNetwork, x, out index_pred, out index_succ);
        }

        /// <summary>
        /// Computes the betweenness preference matrix of a node based on the set of two-paths of a node.
        /// By this we essentially implement equations (1) and (2). 
        /// If additionally the normalization parameter is set, equation (3) will be computed.
        /// </summary>        
        /// <param name="temp_net">The temporal network to compute betweeness preference for</param>
        /// <param name="x">The node for which to compute the betweenness preference matrix</param>
        /// <param name="ego_net">The weighted, aggregate ego network</param>
        /// <param name="index_pred">A mapping of nodes to columns in the betweenness preference matrix</param>
        /// <param name="index_succ">A mapping of nodes to rows in the betweenness preference matrix</param>
        /// <param name="normalize">Whether or not to normalize the matrix, i.e. whether B (eq. 2) or P (eq. 3) shall be returned</param>
        /// <returns>Depending on the normalization, a betweenness preference matrix B or the normalized version P will be returned</returns>
        public static double[,] GetBetweennessPrefMatrix(TemporalNetwork temp_net, 
            string x, 
            out Dictionary<string, int> index_pred, 
            out Dictionary<string, int> index_succ,
            bool normalized=true)
        {
            // Use a mapping of indices to node labels
            index_pred = new Dictionary<string, int>();
            index_succ = new Dictionary<string, int>();         

            // Create an empty matrix
            double[,] B = new double[temp_net.AggregateNetwork.GetIndeg(x), temp_net.AggregateNetwork.GetOutdeg(x)];

            // Create the index-to-node mapping
            int i = 0;
            foreach (string u in temp_net.AggregateNetwork.GetPredecessors(x))
                index_pred[u] = i++;

            i = 0;
            foreach (string w in temp_net.AggregateNetwork.GetSuccessors(x))
                index_succ[w] = i++;

            // Here we implement equation (1) and (2), i.e. we normalize PER TIME STEP
            foreach (var t in temp_net.TwoPathsByNode[x].Keys)
                foreach (var two_path in temp_net.TwoPathsByNode[x][t])
                    B[index_pred[two_path.Item1], index_succ[two_path.Item2]] += 1d / (double) temp_net.TwoPathsByNode[x][t].Count;            

            // This is equation 3, i.e. we normalize ACROSS THE MATRIX
            if(normalized)
                return NormalizeMatrix(x, temp_net.AggregateNetwork, B);
            else return B;
        }

        /// <summary>
        /// Computes a normalized version P of a given betweenness preference matrix B.
        /// </summary>
        /// <param name="x">The node for which the normalized matrix is computed</param>
        /// <param name="aggregate_net">The weighted aggregate network</param>
        /// <param name="B">The betweenness preference matrix that shall be normalized</param>
        /// <returns>A normalized version of the betweenness preference matrix B</returns>
        public static double[,] NormalizeMatrix(string x, WeightedNetwork aggregate_net, double[,] B)
        {
            // Normalize the matrix ( i.e. this is equation (3) )
            double[,] P = new double[aggregate_net.GetIndeg(x), aggregate_net.GetOutdeg(x)];

            double sum = 0d;
            for (int s = 0; s < aggregate_net.GetIndeg(x); s++)
                for (int d = 0; d < aggregate_net.GetOutdeg(x); d++)
                    sum += B[s, d];

            if(sum>0d)
                for (int s = 0; s < aggregate_net.GetIndeg(x); s++)
                    for (int d = 0; d < aggregate_net.GetOutdeg(x); d++)
                        P[s, d] = B[s, d] / sum;
            return P;
        }

        /// <summary>
        /// Parallely computes the betweenness preference distribution of all nodes in a temporal network
        /// </summary>
        /// <param name="temp_net">The temporal network to analyze</param>
        /// <returns>An IEnumerable containing betweenness preferences of all nodes</returns>
        public static IEnumerable<double> GetBetweennessPrefDist(TemporalNetwork temp_net)
        {
            List<double> dist = new List<double>(temp_net.AggregateNetwork.VertexCount);

            Parallel.ForEach<string>(temp_net.AggregateNetwork.Vertices, v =>
            {
                double betweennessPref = BetweennessPref.GetBetweennessPref(temp_net, v);

                // Synchronized access to the list
                if(temp_net.AggregateNetwork.GetIndeg(v)>0 && temp_net.AggregateNetwork.GetOutdeg(v)>0)
                    lock (dist)
                        dist.Add(betweennessPref);

            });
            return dist;
        }

        /// <summary>
        /// Computes the scalar betwenness preference of a node based on its normalized betweenness preference matrix
        /// </summary>
        /// <param name="aggregate_net">The temporal network for which to compute betweenness preference</param>
        /// <param name="x">The node for which to compute betweenness preference</param>
        /// <param name="P">The betweenness preference matrix based on which betw. pref. will be computed</param>
        /// <returns>The betweenness preference, defined as the mutual information of the source and target of two-paths</returns>
        public static double GetBetweennessPref(WeightedNetwork aggregate_net, string x, double[,] P)
        {
            // If the network is empty, just return zero
            if (aggregate_net.VertexCount == 0)
                return 0d;

            // Compute the mutual information (i.e. betweenness preference)
            double I = 0;

            int indeg = aggregate_net.GetIndeg(x);
            int outdeg = aggregate_net.GetOutdeg(x);

            double[] marginal_s = new double[indeg];
            double[] marginal_d = new double[outdeg];

            // Marginal probabilities P_d = \sum_s'{P_{s'd}}
            for (int d = 0; d < outdeg; d++)
            {
                double P_d = 0d;
                for (int s_prime = 0; s_prime < indeg; s_prime++)
                    P_d += P[s_prime, d];
                marginal_d[d] = P_d;
            }

            // Marginal probabilities P_s = \sum_d'{P_{sd'}}
            for (int s = 0; s < indeg; s++)
            {
                double P_s = 0d;
                for (int d_prime = 0; d_prime < outdeg; d_prime++)
                    P_s += P[s, d_prime];
                marginal_s[s] = P_s;
            }

            // Here we just compute equation (4) of the paper ... 
            for (int s = 0; s < indeg; s++)
                for (int d = 0; d < outdeg; d++)                    
                    if (P[s, d] != 0) // 0 * Log(0)  = 0
                        // Mutual information
                        I += P[s, d] * Math.Log(P[s, d] / (marginal_s[s] * marginal_d[d]), 2d);
            return I;
        }

        /// <summary>
        /// Computes the (scalar) betwenness preference of a node
        /// </summary>
        /// <param name="temp_net">The temporal network for which to compute betweenness preference</param>
        /// <param name="x">The node for which to compute betweenness preference</param>
        /// <returns>The betweenness preference, defined as the mutual information of the source and target of two-paths</returns>
        public static double GetBetweennessPref(TemporalNetwork temp_net, string x)
        {
            // This will be used to store the index mappings in the betweenness preference matrix
            Dictionary<string, int> index_pred;
            Dictionary<string, int> index_succ;

            // Compute the normalized betweenness preference matrix of x according to equation (2) and (3)
            double[,] P = GetBetweennessPrefMatrix(temp_net, x, out index_pred, out index_succ);

            return GetBetweennessPref(temp_net.AggregateNetwork, x, P);
        }
    }
}
