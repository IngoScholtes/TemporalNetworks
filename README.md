TemporalNetworks
===

This project implements an Open Source C# library as well as a set of command line tools for the analysis of dynamic network structures, so-called temporal networks. The code particularly focuses on the computation of so-called betweenness preference, which has been introduced in [1].  

![Screenshot of command line tools](https://dl.dropbox.com/u/10482894/Screenshot_TempNet.png)

The command line tools and the library can be used to compare weighted aggregate networks from temporal sequences of edges, create tikz figures of temporal unfoldings of dynamic networks, compute betweenness preference distributions of empirical networks, generate null model realizations based on empirical data and create second-order aggregate networks as introduced in [2]. An example for a time-unfolded network with 6 nodes and 10 time steps is shown in the following figure.

![Example output of time-unfolded representation of temporal network with 6 nodes and 10 time steps](https://dl.dropbox.com/u/10482894/TikzOutput_TempNet.png)

Main features
---

The emphasis of the framework is on ... 

- ease-of-use
- efficiency and multi-core awareness with automatic parallelization
- clean, fully documented and easily understandable code
- easy integrateability in own projects
- inclusion of unit tests
- scriptability on the command line

The project consists of the following components (which you will find as separate class libraries and projects in the code).

TempNet
---

A set of command line tools that can be used to analyze betweenness preference in temporal networks

TemporalNetworks
---

The basis framework for the storage, analysis and visualization of temporal networks. This is the core code of the project, to which the TempNet command line tool provides an interface

TemporalNetworksDemo
---

A simple example application that shows how to use the framework in your own code.

TemporalNetworksTest
---

A set of example data and unit tests to make sure that future updates do not break the correctness of the computations as defined in: 


[1] *R. Pfitzner, I. Scholtes, A. Garas, C. J. Tessone, F. Schweitzer:* **"Betweenness Preference: Quantifying Correlations in The Topological Dynamics of Temporal Networks"**, Physical Review Letters, Vol. 110, May 10 2013, http://prl.aps.org/abstract/PRL/v110/i19/e198701

[2] *I. Scholtes, N. Wider, R. Pfitzner, A. Garas, C. J. Tessone, F. Schweitzer:* **"Slow-Down vs. Speed-Up of Information Diffusion in Non-Markovian Temporal Networks"**, http://arxiv.org/abs/1307.4030