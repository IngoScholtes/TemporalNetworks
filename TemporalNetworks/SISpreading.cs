using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalNetworks
{
    /// <summary>
    /// This class implements an SI model for epidemic spreading
    /// </summary>
    public class SISpreading
    {
        /// <summary>
        /// A random generator used for the spreading
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// Runs a simple SI spreading on a given temporal network and returns the dynamics of the infections
        /// </summary>
        /// <param name="temp_net">The temporal network to use</param>
        /// <param name="p">The infection probability (default is 1)</param>
        /// <returns>An enumerable of infection numbers, each entry giving the number of infected individuals at a certain time of the simulation</returns>
        public static string RunSpreading(TemporalNetwork temp_net, out string times, double p=1d)
        {

            // Which nodes are already infected? 
            Dictionary<string, bool> discovered = new Dictionary<string, bool>();
            Dictionary<int,int> infections = new Dictionary<int,int>(temp_net.Count);

            // Initialize
            foreach (string s in temp_net.AggregateNetwork.Vertices)
                discovered[s] = false;

            // Build the initial matrix of fastestPathLengths
            Dictionary<string, int> indices = new Dictionary<string, int>(temp_net.AggregateNetwork.VertexCount);
            short[,] fastestPathLengths = new short[temp_net.AggregateNetwork.VertexCount, temp_net.AggregateNetwork.VertexCount];
            short[,] fastestPath_mid_Lengths = new short[temp_net.AggregateNetwork.VertexCount, temp_net.AggregateNetwork.VertexCount];
            uint[,] fastestPathDurations= new uint[temp_net.AggregateNetwork.VertexCount, temp_net.AggregateNetwork.VertexCount];                        
            
            // Build the indices
            int i = 0;
            foreach (string s in temp_net.AggregateNetwork.Vertices)
                indices[s] = i++;

            // Initialize with maximum values ... 
            foreach (string s in temp_net.AggregateNetwork.Vertices)
                foreach (string d in temp_net.AggregateNetwork.Vertices)
                {
                    fastestPathLengths[indices[s], indices[d]] = short.MaxValue;
                    fastestPath_mid_Lengths[indices[s], indices[d]] = short.MaxValue;
                    fastestPathDurations[indices[s], indices[d]] = uint.MaxValue;
                }

            // Run a spreading process starting at each node in the network ... 
            foreach (var start in temp_net.AggregateNetwork.Vertices)
            {
                // Reset the infected state for all 
                foreach (string s in temp_net.AggregateNetwork.Vertices)
                    discovered[s] = false;
                
                // Infect the start node 
                int discovered_n = 1;
                discovered[start] = true;                               
                fastestPathLengths[indices[start], indices[start]] = 0;
                fastestPathDurations[indices[start], indices[start]] = 0;                

                try
                {
                    // Get the first time stamp where the start node was source of two path
                    var start_time = temp_net.TwoPathsByStartTime.Keys.First(z =>
                    {
                        foreach (string twopath in temp_net.TwoPathsByStartTime[z])
                            if (twopath.StartsWith(start))
                                return true;
                        return false;
                    });
                    uint t_ = (uint) start_time;

                    // Create a list of ordered time stamps starting with the first activity of the start node
                    var time_stamps = from x in temp_net.TwoPathsByStartTime.Keys where x >= start_time orderby x ascending select x;
                    
                    // Extract all paths consisting of "two path" building blocks
                    foreach (int t in time_stamps)
                    {
                        // A path continues if the source node of a two-path has already been discovered
                        foreach (var two_path in temp_net.TwoPathsByStartTime[t])
                        {
                            // Get the three components of the two path
                            string[] comps = two_path.Split(',');

                            // Record the geodesic distance between the start and the middle node comps[1]
                            fastestPath_mid_Lengths[indices[start], indices[comps[1]]] = (short) Math.Min(fastestPath_mid_Lengths[indices[start], indices[comps[1]]], fastestPathLengths[indices[start], indices[comps[0]]]+1);
                            
                            // If the target node of a two path has not been discovered before, we just discovered a new fastest time-respecting path!
                            if (discovered[comps[0]] && !discovered[comps[2]])
                            {
                                // Add to nodes already discovered
                                discovered_n++;
                                discovered[comps[2]] = true;

                                // Record geodesic distance of fastest time-respecting path
                                fastestPathLengths[indices[start], indices[comps[2]]] = (short)(fastestPathLengths[indices[start], indices[comps[0]]] + 2);
                                
                                // Record temporal length of fastest time-respecting path
                                fastestPathDurations[indices[start], indices[comps[2]]] = (uint) (t-start_time);
                            }
                        }
                        // Stop as soon as all nodes have been discovered
                        if (discovered_n == temp_net.AggregateNetwork.VertexCount)
                            break;

                        // Advance the time by two
                        t_ += 2;
                    }
                }
                catch(Exception) { 
                    // In this case, a start node is never the source of two-path, so we just ignore it ... 
                }
            }

            // Aggregate the matrices 
            foreach(string s in temp_net.AggregateNetwork.Vertices)
                foreach (string d in temp_net.AggregateNetwork.Vertices)
                    fastestPathLengths[indices[s], indices[d]] = Math.Min(fastestPathLengths[indices[s], indices[d]], fastestPath_mid_Lengths[indices[s], indices[d]]);
           
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_t = new StringBuilder();

            var nodes = from n in temp_net.AggregateNetwork.Vertices.AsParallel() orderby n ascending select n;

            foreach (string s in nodes)
            {
                foreach (string d in nodes)
                {
                    sb.Append(fastestPathLengths[indices[s], indices[d]]+"\t");
                    sb_t.Append(fastestPathDurations[indices[s], indices[d]] + "\t");
                }
                sb.Append("\n");
                sb_t.Append("\n");
            }
            times = sb_t.ToString();            
            return sb.ToString();
        }
    }
}
