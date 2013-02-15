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

            // Compute betweenness preference matrices 
            var matrices = new Dictionary<string,double[,]>();
            var indices_pred = new Dictionary<string, Dictionary<string,int>>();
            var indices_succ = new Dictionary<string, Dictionary<string,int>>();            

            foreach(string x in temp_net.AggregateNetwork.Vertices)
            {
                var ind_p = new Dictionary<string,int>();
                var ind_s = new Dictionary<string, int>();
                matrices[x] = BetweennessPref.GetBetweennessPrefMatrix(temp_net, x, out ind_p, out ind_s, true);
                indices_pred[x] = ind_p;
                indices_succ[x] = ind_s;
            }

            Random r = new Random();

            // The nodes that have already been visited 
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            // The number of unique visited nodes per time step
            Dictionary<int, int> uniqueVisitations = new Dictionary<int, int>(temp_net.Count);
            
            // Use a random node and a random predecessor for initialization
            string v = temp_net.AggregateNetwork.RandomNode;
            string s = temp_net.AggregateNetwork.GetRandomPredecessor(v);
            visited[v] = true;
            int t = 0;
            uniqueVisitations[t] = 1;
            int restarts = 0;

            // Perform random walk, either biased by the weights in the aggregate network or by the betweenness preference matrices
            while (uniqueVisitations[t] < temp_net.AggregateNetwork.VertexCount-5)
            {
                t++;

                // The target of this step ... 
                string d = null;

                // If we are not using betweenness preference biasing, then just use the weights 
                if (!betwPrefPres)
                    d = temp_net.AggregateNetwork.GetRandomSuccessor(v, weighted);
                else
                {
                    // sum the probabilities for all targets from v coming from s
                    double sum = 0d;
                    foreach (string target in temp_net.AggregateNetwork.GetSuccessors(v))
                    {
                        sum = sum + matrices[v][indices_pred[v][s], indices_succ[v][target]];
                    }                    

                    // If there is nowhere to go from v coming from s (i.e. all entries in the matrix column are zero) generate a new initial node
                    if (sum == 0)
                    {
                        d = temp_net.AggregateNetwork.RandomNode;
                        v = temp_net.AggregateNetwork.GetRandomPredecessor(d);
                        restarts++;
                    }
                    // Sample a target with correct probabilities 
                    else
                    {
                        // Create cumulative probabilities 
                        double c = 0d;
                        Dictionary<double, string> cumulative = new Dictionary<double, string>();

                        foreach (string target in temp_net.AggregateNetwork.GetSuccessors(v))
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
                        for (int i = 0; i < temp_net.AggregateNetwork.GetOutdeg(v); i++)
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
                s = v;
                v = d;
                System.Diagnostics.Debug.Assert(v != null && s != null && d != null);
            }
            Console.WriteLine("Restart = {0}", restarts);
            return uniqueVisitations;
        }
    }
}
