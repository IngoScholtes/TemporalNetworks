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

        public static IDictionary<int, int> RunRandomWalker(TemporalNetwork temp_net, bool weighted = false, bool betwPrefPres = false)
        {
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            Dictionary<int, int> uniqueVisitations = new Dictionary<int, int>(temp_net.Count);
            
            string currentNode = temp_net.AggregateNetwork.RandomNode;
            visited[currentNode] = true;
            int t = 0;
            uniqueVisitations[t] = 1;

            while (uniqueVisitations[t] < temp_net.AggregateNetwork.VertexCount)
            {
                t++;
                string next = temp_net.AggregateNetwork.GetRandomSuccessor(currentNode, weighted);
                if (!visited.ContainsKey(next))
                    visited[next] = true;
                uniqueVisitations[t] = visited.Keys.Count;
                currentNode = next;                
            }
            return uniqueVisitations;
        }
    }
}
