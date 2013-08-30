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
            _current_visitations = new Dictionary<string, int>();

            // Set visitations to 0
            foreach (string v in network.AggregateNetwork.Vertices)
                _current_visitations[v] = 0;

            // Reduce first and second-order aggregate network to strongly connected component
            _network.AggregateNetwork.ReduceToLargestStronglyConnectedComponent();
            _network.SecondOrderAggregateNetwork.ReduceToLargestStronglyConnectedComponent();

            // Initialize random walk 
            CurrentEdge = StringToTuple(_network.SecondOrderAggregateNetwork.RandomNode);
            CurrentNode = CurrentEdge.Item2;
            _current_visitations[CurrentNode] = 1;
            Time = 1;

            // Compute stationary distribution
            _stationary_dist = ComputeStationaryDist();

            InitializeCumulatives();
        }

        /// <summary>
        /// Initializes all vectors that will be used for sampling transition probabilities in first and second-order network
        /// </summary>
        private void InitializeCumulatives()
        {
            cumulatives_first = new Dictionary<string, Dictionary<double, string>>();
            sums_first = new Dictionary<string, double>();

            cumulatives_second = new Dictionary<Tuple<string, string>, Dictionary<double, Tuple<string, string>>>();
            sums_second = new Dictionary<Tuple<string, string>, double>();
            
            // Initialize sampling vectors for walk in static first-order network
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
                    sum += _network.SecondOrderAggregateNetwork.GetWeight(e1, e2);
                    cumulatives_second[e1_t][sum] = e2_t;
                }
                sums_second[e1_t] = sum;
            }                    
        }

        /// <summary>
        /// Convert an edge in string format to tuple format
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private static Tuple<string, string> StringToTuple(string edge)
        {
            string[] splitted = edge.Split(';');
            return new Tuple<string, string>(splitted[0].TrimStart('('), splitted[1].TrimEnd(')'));
        }

        /// <summary>
        /// Convert an edge in tuple format to string format
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private static string TupleToString(Tuple<string, string> edge)
        {
            return string.Format("({0};{1})", edge.Item1,edge.Item2);
        }


        /// <summary>
        /// Performs one transition of the random walk process. 
        /// </summary>
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
                    // Draw a sample uniformly from [0,1] and multiply it with the cumulative sum for the current edge ...
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

                    // System.Diagnostics.Debug.Assert(next_edge != null, "Error! Random walk cannot find a transition from edge {0}", TupleToString(CurrentEdge));

                    CurrentEdge = next_edge;
                    CurrentNode = next_edge.Item2;
                    _current_visitations[CurrentNode]++;
                    Time++;
                    break;
                }

            }
        }

        public double GetMaxVisitationProb()
        { 
            return (double) _current_visitations.Values.Max() / (double) Time;
        }

        public double GetMinVisitationProb()
        {
            return (double)_current_visitations.Values.Min() / (double)Time;
        }

        public double GetVisitationProb(string node)
        {
            return (double) _current_visitations[node] / (double) Time;
        }

        /// <summary>
        /// Computes the expected stationary distribution of a random walk in the weighted aggregate network
        /// NOTE: At present, this implementation only works correctly if the network is undirected!
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        private Dictionary<string, double> ComputeStationaryDist()
        {
            Dictionary<string, double> dist = new Dictionary<string, double>();

            foreach (var node in _network.AggregateNetwork.Vertices)
                dist[node] = 0d;

            if(RandomWalkMode == TemporalNetworks.RandomWalkMode.StaticFirstOrder)
                foreach (var node in _network.AggregateNetwork.Vertices)
                    dist[node] =  _network.AggregateNetwork.GetCumulativeInWeight(node) /  _network.AggregateNetwork.CumulativeWeight;
            else if (RandomWalkMode == TemporalNetworks.RandomWalkMode.StaticSecondOrder)
            { 
                double sum = 0d;
                foreach(var edge in _network.SecondOrderAggregateNetwork.Vertices)
                {
                    var edge_t = StringToTuple(edge);
                    dist[edge_t.Item2] += _network.SecondOrderAggregateNetwork.GetCumulativeInWeight(edge);
                    sum += _network.SecondOrderAggregateNetwork.GetCumulativeInWeight(edge);
                }
                foreach (var node in _network.AggregateNetwork.Vertices)
                    dist[node] = dist[node] / sum;
            }

            return dist; 
        }           
    }
}
