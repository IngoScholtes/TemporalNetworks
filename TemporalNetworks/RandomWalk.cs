using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalNetworks
{

    public enum RandomWalkMode { Temporal = 0, StaticFirstOrder = 1, StaticSecondOrder = 2 };

    /// <summary>
    /// A class that can be used to study random walk dynamics in temporal networks
    /// </summary>
    public class RandomWalk
    {

        /// <summary>
        /// A random generator used for the random walker
        /// </summary>
        private Random rand;

        /// <summary>
        /// Returns the current node the random walker is is
        /// </summary>
        public string CurrentNode
        {
            get;
            private set; 
        }

        /// <summary>
        /// Returns the current edge the random walk is in
        /// </summary>
        public Tuple<string, string> CurrentEdge
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the total variation distance between the stationary distribution and the current visitation probabilities
        /// </summary>
        public double TVD
        {
            get {
                double tvd = 0d;
                foreach (string v in _network.AggregateNetwork.Vertices)
                    tvd += Math.Abs(_stationary_dist[v] - (double)_current_visitations[v] / (double)Time);
                return tvd / 2d;
            }
        }

        /// <summary>
        /// Returns the current step number of the random walk process 
        /// </summary>
        public int Time
        {
            get;
            private set; 
        }

        /// <summary>
        /// Returns the mode of this random walk instance
        /// </summary>
        public RandomWalkMode RandomWalkMode
        {
            get;
            private set;
        }

        private Dictionary<string, int> _current_visitations = null;
        private Dictionary<string, double> _stationary_dist = null;
        private TemporalNetwork _network = null;

        Dictionary<string, Dictionary<double, string>> cumulatives_first;
        Dictionary<string, double> sums_first;

        Dictionary<Tuple<string, string>, Dictionary<double, Tuple<string, string>>> cumulatives_second;
        Dictionary<Tuple<string, string>, double> sums_second;               

        /// <summary>
        /// Creates an instance of a random walk process
        /// </summary>
        /// <param name="network"></param>
        /// <param name="walkmode"></param>
        public RandomWalk(TemporalNetwork network, RandomWalkMode walkmode)
        {
            RandomWalkMode = walkmode;
            _network = network;
            rand = new Random();

            // Set visitations to 0
            foreach (string v in network.AggregateNetwork.Vertices)
                _current_visitations[v] = 0;

            // Initialize random walk 
            CurrentEdge = network.AggregateNetwork.RandomEdge;
            CurrentNode = CurrentEdge.Item2;
            _current_visitations[CurrentNode] = 1;
            Time = 1;

            // Compute stationary distribution
            _stationary_dist = ComputeStationaryDist(network);
        }

        private void InitializeCumulatives()
        {
            cumulatives_first = new Dictionary<string, Dictionary<double, string>>();
            sums_first = new Dictionary<string, double>();

            cumulatives_second = new Dictionary<Tuple<string, string>, Dictionary<double, Tuple<string, string>>>();
            sums_second = new Dictionary<Tuple<string, string>, double>();
            
            // Initializesampling vectors for walk in static first-order network
            foreach (var v in _network.AggregateNetwork.Vertices)
            {
                cumulatives_first[v] = new Dictionary<double,string>();
                double sum = 0d;
                foreach (string target in _network.AggregateNetwork.GetSuccessors(v))
                {
                    sum += _network.AggregateNetwork.GetWeight(v,target);
                    cumulatives_first[v][sum] = target;
                }
                sums_first[v] = sum;
            }

            // Initialize sampling vectors for walk in static second-order network
            foreach (var e1 in _network.SecondOrderAggregateNetwork.Vertices)
            {
                var e1_t = StringToTuple(e1);
                cumulatives_second[e1_t] = new Dictionary<double, Tuple<string, string>>();
                double sum = 0d;
                foreach (string e2 in _network.SecondOrderAggregateNetwork.GetSuccessors(e1))
                {
                    var e2_t = StringToTuple(e2);
                    sum += _network.AggregateNetwork.GetWeight(e1, e2);
                    cumulatives_second[e1_t][sum] = e2_t;
                }
                sums_second[e1_t] = sum;
            }                    
        }

        private static Tuple<string, string> StringToTuple(string edge)
        {
            string[] splitted = edge.Split(';');
            return new Tuple<string, string>(splitted[0].TrimStart('('), splitted[0].TrimEnd(')'));
        }

        private static string TupleToString(Tuple<string, string> edge)
        {
            return string.Format("({0};{1})", edge.Item1,edge.Item2);
        }

        public void Step()
        {            
            switch(RandomWalkMode)
            {
                case RandomWalkMode.Temporal:
                {

                    break; 
                }
                case TemporalNetworks.RandomWalkMode.StaticFirstOrder:
                {
                    // Draw a sample uniformly from [0,1] and multiply it with the cumulative sum for the current node ...
                    double sample = rand.NextDouble() * sums_first[CurrentNode];

                    // Determine the next transition ... 
                    string next_node = null;
                    for (int i = 0; i < cumulatives_first[CurrentNode].Count; i++)
                    {
                        if (cumulatives_first[CurrentNode].Keys.ElementAt(i) > sample)
                        {
                            next_node = cumulatives_first[CurrentNode].Values.ElementAt(i);
                            break;
                        }
                    }

                    System.Diagnostics.Debug.Assert(next_node != null, "Error! Random walk cannot find a transition from node {0}", CurrentNode);

                    CurrentEdge = new Tuple<string, string>(CurrentNode, next_node);
                    CurrentNode = next_node;
                    _current_visitations[CurrentNode]++;
                    Time++;

                    break; 
                }
                case TemporalNetworks.RandomWalkMode.StaticSecondOrder:
                {
                    // Draw a sample uniformly from [0,1] and multiply it with the cumulative sum for the current node ...
                    double sample = rand.NextDouble() * sums_second[CurrentEdge];

                    // Determine the next transition ... 
                    Tuple<string,string> next_edge = null;
                    for (int i = 0; i < cumulatives_second[CurrentEdge].Count; i++)
                    {
                        if (cumulatives_second[CurrentEdge].Keys.ElementAt(i) > sample)
                        {
                            next_edge = cumulatives_second[CurrentEdge].Values.ElementAt(i);
                            break;
                        }
                    }

                    System.Diagnostics.Debug.Assert(next_edge != null, "Error! Random walk cannot find a transition from edge {0}", TupleToString(CurrentEdge));

                    CurrentEdge = next_edge;
                    CurrentNode = next_edge.Item2;
                    _current_visitations[CurrentNode]++;
                    Time++;
                    break;
                }

            }
        }

        private static Dictionary<string, double> ComputeStationaryDist(TemporalNetwork network)
        {
            Dictionary<string, double> dist = new Dictionary<string, double>();

            foreach (var node in network.AggregateNetwork.Vertices)
                dist[node] = 0d;

            foreach (var edge in network.AggregateNetwork.Edges)
            {
                dist[edge.Item2] += network.AggregateNetwork.GetWeight(edge);
                dist[edge.Item1] += network.AggregateNetwork.GetWeight(edge);
            }

            foreach (var node in network.AggregateNetwork.Vertices)
                dist[node] = dist[node] / 2d * network.AggregateNetwork.CumulativeWeight;

            return dist; 
        }           
    }
}
