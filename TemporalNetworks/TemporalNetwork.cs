using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalNetworks
{
    /// <summary>
    /// A class that represents a temporal network which consists of an ordered sequence of edges
    /// </summary>
    public class TemporalNetwork : Dictionary<int,List<Tuple<string, string>>>
    {
        /// <summary>
        /// A cached version of the weighted network (will be recomputed only when needed)
        /// </summary>
        WeightedNetwork _cachedWeightedNetwork = null;

        bool _stripEdges = true;

        /// <summary>
        /// With this, we cache the preprocessed two path sequence, so we need to do the heavy computations only once
        /// </summary>
        Dictionary<string, Dictionary<int, List<Tuple<string, string>>>> _twoPathsByNode = null;
        Dictionary<string, double> _twoPathWeights = null;

        /// <summary>
        /// The two paths of all nodes in the temporal network
        /// </summary>
        public Dictionary<string, Dictionary<int, List<Tuple<string, string>>>> TwoPathsByNode
        {
            get 
            {
                if (_twoPathsByNode == null || _cachedWeightedNetwork == null)
                    ReduceToTwoPaths();
                return _twoPathsByNode;
            }
        }
        
        /// <summary>
        /// Returns a dictionary that contains the weights of two paths
        /// </summary>
        public Dictionary<string, double> TwoPathWeights
        {
            get {
                if (_twoPathWeights == null || _cachedWeightedNetwork == null)
                    ReduceToTwoPaths();
                return _twoPathWeights;
            }
        }


        /// <summary>
        /// Whether or not to remove all edges that do not belong to two paths
        /// </summary>
        public bool StripEdgesToTwoPaths
        {
            get
            {
                return _stripEdges;
            }
            set {
                _stripEdges = value;
            }
        }

        /// <summary>
        /// Returns the weighted, aggregate representation of the temporal network instance. 
        /// The aggregate network will be cached to speed up computations. 
        /// It is automatically invalidated (and will be recomputed) whenever the temporal network is changed.
        /// </summary>
        public WeightedNetwork AggregateNetwork
        {
            get
            {
                if (_twoPathsByNode == null || _cachedWeightedNetwork == null)
                    ReduceToTwoPaths();
                return _cachedWeightedNetwork;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="aggregationSize"></param>
        public void AggregateTime(int aggregationWindow = 1)
        {
            if (aggregationWindow == 1)
                return;

            Dictionary<int, List<Tuple<string, string>>> new_dict = new Dictionary<int, List<Tuple<string, string>>>();
            int min = Keys.Min();

            foreach (var t in Keys)
            {
                int new_t = (t - min) / aggregationWindow;
                if(!new_dict.ContainsKey(new_t))
                    new_dict[new_t] = new List<Tuple<string,string>>();
                new_dict[new_t].AddRange(this[t]);
            }

            this.Clear();
            foreach (var t in new_dict.Keys)
            {
                if (!this.ContainsKey(t))
                    this[t] = new List<Tuple<string, string>>();
                foreach (var edge in new_dict[t])
                    if (!this[t].Contains(edge))
                        this[t].Add(edge);
            }

            ReduceToTwoPaths();
        }

        public class CompareInts : IComparer<int> {
            public int Compare(int t1, int t2)
            {
                return t1 - t2;
            }
        }

        /// <summary>
        /// This methods extracts all two paths from the sequence of edges in the temporal network (two-paths according to eq. (1) of the paper).
        /// It will also extract the correct (statistical) weights for both the two paths and the edges in the aggregate network. 
        /// After this method returns, the weighted TwoPaths list as well as the weighted AggregateNetwork are available. 
        /// If an explicit call to preprocess is ommitted, the preprocessing will be triggered whenever the AggregateNetwork or the TwoPaths dictionary
        /// is used for the first time. Changing the temporal network (i.e. adding or removing and edge) will invalidate both, so this method has to be called 
        /// again.
        /// </summary>
        /// <seealso cref="TwoPathsByNode"/>
        /// <seealso cref="AggregateNetwork"/>
        public void ReduceToTwoPaths()
        {
            _twoPathsByNode = new Dictionary<string, Dictionary<int,List<Tuple<string, string>>>>();
            _twoPathWeights = new Dictionary<string, double>();
            var two_path_edges = new Dictionary<int,List<Tuple<string,string>>>();

            int prev_t = -1;

            var ordered_time = Keys.OrderBy(k => k, new CompareInts());

            // Walk through time ... 
            foreach(int t in ordered_time)
            {
                if (prev_t == -1)
                    prev_t = t; // We skip the first time step and just set the prev_t index ... 
                else
                {
                    // N.B.: Only two-paths consisting of edges in time steps immediately following each other are found 
                    // N.B.: We also account for multiple edges happening at the same time, i.e. multiple two-paths can pass through a node at a given time t!
                    // N.B.: For three consecutive edges (a,b), (b,c), (c,d) , two two-paths (a,b,c) and (b,c,d) will be found
                    foreach (var in_edge in this[prev_t])
                    {
                        foreach(var out_edge in this[t])
                        {
                            // In this case, we found the two_path (in_edge) -> (out_edge) = (s,v) -> (v,d)
                            if(in_edge.Item2 == out_edge.Item1)
                            {
                                // Use notation from the paper
                                string s = in_edge.Item1;
                                string v = in_edge.Item2;
                                string d = out_edge.Item2;

                                string two_path = s + "," + v + "," + d;

                                double indeg_v = 0d;
                                double outdeg_v = 0d;

                                indeg_v = (from x in this[prev_t].AsParallel() where x.Item2 == v select x).Count();

                                //foreach (var edge in this[prev_t])
                                 //   if (edge.Item2 == v)
                                 //       indeg_v++;

                                outdeg_v = (from x in this[t].AsParallel() where x.Item1 == v select x).Count();

                                //foreach (var edge in this[t])
                                 //   if (edge.Item1 == v)
                                  //      outdeg_v++;

                                if(!_twoPathWeights.ContainsKey(two_path))
                                    _twoPathWeights[two_path] = 0d;

                                _twoPathWeights[two_path] += 1d / (indeg_v * outdeg_v);

                                if (!two_path_edges.ContainsKey(prev_t))
                                    two_path_edges[prev_t] = new List<Tuple<string, string>>();
                                if (!two_path_edges.ContainsKey(t))
                                    two_path_edges[t] = new List<Tuple<string, string>>();

                                // Important: In the reduced temporal network, we only use edges belonging to two paths. Each edge is added only once, 
                                // even if it belongs to several two paths (this is the case for continued two paths as well as for two paths with multiple edges
                                // in one time step
                                if(!two_path_edges[prev_t].Contains(in_edge))
                                    two_path_edges[prev_t].Add(in_edge);
                                if(!two_path_edges[t].Contains(out_edge))
                                    two_path_edges[t].Add(out_edge);

                                // Add the identified two paths to the list of two paths passing through v at time t
                                if (!_twoPathsByNode.ContainsKey(v))
                                    _twoPathsByNode[v] = new Dictionary<int,List<Tuple<string, string>>>();
                                if (!_twoPathsByNode[v].ContainsKey(t))
                                    _twoPathsByNode[v][t] = new List<Tuple<string, string>>();

                                _twoPathsByNode[v][t].Add(new Tuple<string,string>(s,d));
                            }
                        }
                    }                   
                    prev_t = t;
                }                
            }
            
            // Replace the edges of the temporal network by those contributing to two paths
            if (_stripEdges)
            {
                this.Clear();
                foreach (int t in two_path_edges.Keys)
                    this[t] = two_path_edges[t];
            }

            // Build the aggregate network with the correct weights ... 
            _cachedWeightedNetwork = new WeightedNetwork();

            foreach (var two_path in _twoPathWeights.Keys)
            {
                string[] split = two_path.Split(',');
                _cachedWeightedNetwork.AddEdge(split[0], split[1], EdgeType.Directed, _twoPathWeights[two_path]);
                _cachedWeightedNetwork.AddEdge(split[1], split[2], EdgeType.Directed, _twoPathWeights[two_path]);                
            }

            foreach (string v in _cachedWeightedNetwork.Vertices)
                if (!_twoPathsByNode.ContainsKey(v))
                    _twoPathsByNode[v] = new Dictionary<int, List<Tuple<string, string>>>();
        }

        /// <summary>
        /// Creates a temporal network instance from an ordered sequence of edges
        /// </summary>
        /// <param name="sequence">An ordered sequence of edges</param>
        /// <returns>An instance of a temporal network</returns>
        public static TemporalNetwork FromEdgeSequence(List<Tuple<string, string>> sequence)
        {
            TemporalNetwork net = new TemporalNetwork();
            int time = 0;
            foreach (Tuple<string, string> t in sequence)
                net.AddTemporalEdge(time++, t.Item1, t.Item2);
            return net;
        }

        /// <summary>
        /// Reads an edge sequence from a file containing ordered edges and creates a temporal network instance. The file is 
        /// assumed to have one line per edge, each possibly consisting of several colums and separated by a special delimiter character. 
        /// The caller can specify which (zero-based) column number indicate the source and the target of an interaction. If no parameters 
        /// are specified apart from the path, each line in the file is assumed to contain two strings representing the source and target of an 
        /// edge in the first two columns separated by a space. No header line is assumed by default.
        /// </summary>
        /// <param name="path">The path of the file containing the edge sequence.</param>
        /// <param name="source_col">The zero-based column number on the source node of an edge. 0 by default</param>
        /// <param name="target_col">The zero-based column number on the target node of an edge. 1 by default</param>
        /// <param name="header">Whether or not there is a header line that shall be ignored</param>
        /// <param name="split_char">The character used to separate columns in each line</param>
        /// <returns>An instance of a temporal network corresponding to the input sequence</returns>
        public static TemporalNetwork ReadFromFile(string path, bool undirected = false)
        {
            TemporalNetwork temp_net = new TemporalNetwork();

            // Read all data from file 
            string[] lines = System.IO.File.ReadAllLines(path);

            // If empty ... 
            if(lines.Length==0)
                return temp_net;

            // Extract header
            char[] split_chars = new char[] {' ', '\t',';',','};
            char split_char = ' ';

            // Detect the correct separator character in CSV format
            string[] header = null;
            foreach(char c in split_chars)
            {                
                header = lines[0].Split(c);
                if (header.Length >= 2 && header.Contains("node1") && header.Contains("node2"))
                {
                    split_char = c;
                    break;
                }
            }

            if (header.Length < 2)
                return temp_net;

            // Extract indices of columns
            int time_ix = -1;
            int source_ix = -1;
            int target_ix = -1;
            for (int i = 0; i < header.Length; i++)
                if (header[i] == "time")
                    time_ix = i;
                else if (header[i] == "node1")
                    source_ix = i;
                else if (header[i] == "node2")
                    target_ix = i;

            // If there is no source and target column
            if (source_ix < 0 || target_ix < 0)
                return temp_net;

            for(int i=1; i< lines.Length; i++)
            {
                string[] components = lines[i].Split(split_char);
                if (components[source_ix] != "" && components[target_ix] != "")
                {
                    // If there is no explicit time, just consider each edge occuring at consecutive time steps
                    int t = time_ix >=0 ? int.Parse(components[time_ix]) : i;

                    temp_net.AddTemporalEdge(t, components[source_ix], components[target_ix]);
                    if (undirected)
                        temp_net.AddTemporalEdge(t, components[target_ix], components[source_ix]);
                }

            }
            return temp_net;
        }

        /// <summary>
        /// Saves a temporal network to a file
        /// </summary>
        /// <param name="path">The path to which the file shall be saved. Any existing file will be overwritten silently.</param>
        /// <param name="net">The temporal network to save.</param>
        public static void SaveToFile(string path, TemporalNetwork net)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("time node1 node2");

            foreach (int t in net.Keys)
                foreach(var edge in net[t])
                    sb.AppendLine(t + " " + edge.Item1 + " " + edge.Item2);
           
            System.IO.File.WriteAllText(path, sb.ToString());
        }

        /// <summary>
        /// Adds a single edge between two nodes to the temporal network (at the end of the sequence)
        /// </summary>
        /// <param name="v">the source node of an edge</param>
        /// <param name="w">the target node of an edge</param>
        public void AddTemporalEdge(int time, string v, string w)
        {
            if (!this.ContainsKey(time))
                this[time] = new List<Tuple<string, string>>();
            this[time].Add(new Tuple<string, string>(v, w));

            // Invalidate previously preprocessed data
            _cachedWeightedNetwork = null;
            _twoPathsByNode = null;
        }

        /// <summary>
        /// Returns the number of time steps in the temporal network
        /// </summary>
        public int Length {
            get {
                return this.Keys.Count;
            }
        }

        /// <summary>
        /// Returns the lowest number of edges present in a single time step of the network
        /// </summary>
        public int MaxGranularity
        {
            get
            {
                int min = int.MaxValue;
                foreach (int t in this.Keys)
                {
                    min = Math.Min(min, this[t].Count);
                }
                return min;
            }
        }

        /// <summary>
        /// Returns the highest number of edges present in a single time step of the network
        /// </summary>
        public int MinGranularity
        {
            get
            {
                int max = int.MinValue;
                foreach (int t in this.Keys)
                {
                    max = Math.Max(max, this[t].Count);
                }
                return max;
            }
        }

        /// <summary>
        /// Returns the number of recorded two-paths
        /// </summary>
        public int TwoPathCount
        {
            get {
                int twopaths = 0;
                foreach (string v in TwoPathsByNode.Keys)
                    foreach(int t in TwoPathsByNode[v].Keys)
                        twopaths += TwoPathsByNode[v][t].Count;
                return twopaths;
            }
        }

        /// <summary>
        /// Returns the number of edges in the temporal network
        /// </summary>
        public int EdgeCount
        {
            get
            { 
                int edges = 0;
                foreach (int t in this.Keys)
                    edges += this[t].Count;
                return edges;
            }
        }

        /// <summary>
        /// Returns the number of vertices involved in any of the interactions
        /// </summary>
        public int VertexCount {
            get 
            {
                List<string> vertices = new List<string>();
                foreach(int t in this.Keys)
                    foreach (var edge in this[t])
                    {
                        if (!vertices.Contains(edge.Item1))
                            vertices.Add(edge.Item1);
                        if (!vertices.Contains(edge.Item2))
                            vertices.Add(edge.Item2);
                    }
                return vertices.Count;
            }
        }

        /// <summary>
        /// Checks whether two temporal network instances are the same (including the labeling of time steps and the labeling of nodes)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
                return false;

            TemporalNetwork other = obj as TemporalNetwork;

            foreach (int t in this.Keys)
                if (!other.ContainsKey(t))
                    return false;
                else
                {
                    foreach (var edge in this[t])
                        if (!other[t].Contains(edge))
                            return false;
                }
            return true;
        }

        /// <summary>
        /// Just pass on the hash code from the base class
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
