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
            Dictionary<string, bool> infected = new Dictionary<string, bool>();
            Dictionary<int,int> infections = new Dictionary<int,int>(temp_net.Count);

            foreach (string s in temp_net.AggregateNetwork.Vertices)
                infected[s] = false;



            // Build the matrix of fastestPathLengths
            Dictionary<string, int> indices = new Dictionary<string, int>(temp_net.AggregateNetwork.VertexCount);
            short[,] fastestPathLengths = new short[temp_net.AggregateNetwork.VertexCount, temp_net.AggregateNetwork.VertexCount];
            uint[,] fastestPathDurations= new uint[temp_net.AggregateNetwork.VertexCount, temp_net.AggregateNetwork.VertexCount];                        
            int i = 0;
            foreach (string s in temp_net.AggregateNetwork.Vertices)
                indices[s] = i++;

            foreach (string s in temp_net.AggregateNetwork.Vertices)
                foreach (string d in temp_net.AggregateNetwork.Vertices)
                {
                    fastestPathLengths[indices[s], indices[d]] = short.MaxValue;
                    fastestPathDurations[indices[s], indices[d]] = uint.MaxValue;
                }

            // Infect the first active node in the sequence of edges
            foreach (var start in temp_net.AggregateNetwork.Vertices)
            {
                foreach (string s in temp_net.AggregateNetwork.Vertices)
                    infected[s] = false;
                infected[start] = true;
                //infections.Add(temp_net.First().Key, 1);
                int infections_n = 1;
                fastestPathLengths[indices[start], indices[start]] = 0;
                fastestPathDurations[indices[start], indices[start]] = 0;

                var start_time = temp_net.Keys.First(z => {
                    foreach (var edge in temp_net[z])
                        if (edge.Item1 == start || edge.Item2 == start)
                            return true;
                        return false;
                });

                var time_stamps = from x in temp_net.Keys where x >= start_time orderby x ascending select x;
                uint t_ = (uint) start_time;

                // Spreading in the temporal network
                foreach (int t in time_stamps)
                {
                    foreach (var interaction in temp_net[t])
                    {
                        bool infectious = (p == 1d || rand.NextDouble() <= p);

                        // Both nodes can get infected
                        if (infected[interaction.Item1] && !infected[interaction.Item2] && infectious)
                        {
                            infections_n++;
                            infected[interaction.Item2] = true;
                            fastestPathLengths[indices[start], indices[interaction.Item2]] = (short)(fastestPathLengths[indices[start], indices[interaction.Item1]] + 1);
                            fastestPathDurations[indices[start], indices[interaction.Item2]] = t_;
                        }
                        if (infected[interaction.Item2] && !infected[interaction.Item1] && infectious)
                        {
                            infections_n++;
                            infected[interaction.Item1] = true;
                            fastestPathLengths[indices[start], indices[interaction.Item1]] = (short)(fastestPathLengths[indices[start], indices[interaction.Item2]] + 1);
                            fastestPathDurations[indices[start], indices[interaction.Item1]] = t_;
                        }
                    }
                    if (infections_n == temp_net.AggregateNetwork.VertexCount)
                        break;               
                    t_++;
                }
            }
            Console.WriteLine("Finished spreading. Writing path length matrix");
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_t = new StringBuilder();

            var nodes = from n in temp_net.AggregateNetwork.Vertices.AsParallel() orderby n ascending select n;

            foreach (string s in nodes)
            {
                Console.WriteLine(s);
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
