using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Import the temporal network namespace
using TemporalNetworks;

namespace TemporalNetworksDemo
{
    /// <summary>
    /// A simple demo of how to use the temporal networks framework
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {            
            /// Create a simple temporal network consisting of 11 repeated two-paths (this network corresponds two Fig. 2 (right part) in the paper)
            TemporalNetwork temporal_net = new TemporalNetwork();

            int k = 0;

            for (int i = 0; i < 100; i++)
            {
                // Add edges according to two paths
                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                temporal_net.AddTemporalEdge(k++, "a", "e");
                temporal_net.AddTemporalEdge(k++, "e", "g");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                temporal_net.AddTemporalEdge(k++, "a", "e");
                temporal_net.AddTemporalEdge(k++, "e", "g");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                // Note that the next added edge additionally continues a two-path e -> f -> e
                temporal_net.AddTemporalEdge(k++, "f", "e");
                temporal_net.AddTemporalEdge(k++, "e", "b");

                // An additional edge that should be filtered during preprocessing ...
                temporal_net.AddTemporalEdge(k++, "e", "b");

                // And one case where we have multiple edges in a single time step
                temporal_net.AddTemporalEdge(k, "g", "e");                      
                temporal_net.AddTemporalEdge(k++, "c", "e");                                
                temporal_net.AddTemporalEdge(k++, "e", "f");

                temporal_net.AddTemporalEdge(k++, "b", "e");
                temporal_net.AddTemporalEdge(k++, "e", "g");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");

                temporal_net.AddTemporalEdge(k++, "c", "e");
                temporal_net.AddTemporalEdge(k++, "e", "f");
            }

            // Preprocess two-paths and the aggregate network of the temporal network, if this is skipped it will be done automatically when the aggregate network is computed for the first time
            Console.Write("Preparing temporal network...");
            temporal_net.ReduceToTwoPaths();
            Console.WriteLine(" done.");

            // Aggregate the temporal network
            WeightedNetwork aggregate_net = temporal_net.AggregateNetwork;
            
            // Compute and output betweenness preference of v
            Console.WriteLine("Betw. pref. of e in empirical network = \t\t{0:0.00000}", 
                    BetweennessPref.GetBetweennessPref(temporal_net, "e"));

            // Create a random temporal network which only preserves the aggregate network (and destroys bet. pref.)
            TemporalNetwork microstate_random = TemporalNetworkEnsemble.ShuffleEdges(temporal_net, temporal_net.Length);

            microstate_random.ReduceToTwoPaths();

            // Create a random temporal network that preserves both the aggregate network and betw. pref.
            TemporalNetwork microstate_betweennessPref = TemporalNetworkEnsemble.ShuffleTwoPaths(temporal_net, temporal_net.Length);

            microstate_betweennessPref.ReduceToTwoPaths();

            // Compute and output betweenness preference of v in random temporal network
            Console.WriteLine("Betw. pref. of e with shuffled edges = \t\t\t{0:0.00000}",
                    BetweennessPref.GetBetweennessPref(microstate_random, "e"));

            // Compute and output betweenness preference of v in temporal network preserving betw. pref.
            Console.WriteLine("Betw. pref. of e with shuffled two paths = \t\t{0:0.00000}", 
                    BetweennessPref.GetBetweennessPref(microstate_betweennessPref, "e"));

            // Compute the betweenness preference matrices of the networks
            Dictionary<string, int> ind_pred;
            Dictionary<string, int> ind_succ;
            double[,] m1 = BetweennessPref.GetBetweennessPrefMatrix(temporal_net, "e", out ind_pred, out ind_succ, normalized: false);

            double[,] m2 = BetweennessPref.GetBetweennessPrefMatrix(microstate_betweennessPref, "e", out ind_pred, out ind_succ, normalized: false);

            // Get the betweenness preference distribution
            IEnumerable<double> dist = BetweennessPref.GetBetweennessPrefDist(temporal_net);                       

        }
    }
}
