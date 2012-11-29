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
        public static IDictionary<int,int> RunSpreading(TemporalNetwork temp_net, double p=1d)
        {
            Dictionary<string, bool> infected = new Dictionary<string, bool>();

            Dictionary<int,int> infections = new Dictionary<int,int>(temp_net.Count);

            foreach (string s in temp_net.AggregateNetwork.Vertices)
                infected[s] = false;

            // Infect the first active node in the sequence of edges
            infected[temp_net.First().Value.ElementAt(0).Item1] = true;
            infections.Add(temp_net.First().Key, 1);
            int infections_n = 1;

            // Spreading in the temporal network
            foreach(int t in temp_net.Keys)            
            {
                foreach (var interaction in temp_net[t])
                {
                    bool infectious = (p == 1d || rand.NextDouble() <= p);

                    // Both nodes can get infected
                    if (infected[interaction.Item1] && !infected[interaction.Item2] && infectious)
                    {
                        infections_n++;
                        infected[interaction.Item2] = true;
                    }
                    if (infected[interaction.Item2] && !infected[interaction.Item1] && infectious)
                    {
                        infections_n++;
                        infected[interaction.Item1] = true;
                    }
                }
                infections[t] = infections_n;
            }
            return infections;
        }
    }
}
