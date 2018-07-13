// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rotorz.Games.Services
{
    /// <summary>
    /// Sorts services so that root-most dependencies occur first in reslting array.
    /// </summary>
    public sealed class ServiceDependencySorter
    {
        /// <summary>
        /// Sort the specified services by their dependencies so that root-most services
        /// occur first in resulting array.
        /// </summary>
        /// <param name="installers">Collection of service installers.</param>
        /// <returns>
        /// Results of sort; check the value of <see cref="Results.HasCircularDependency"/>
        /// to determine whether sort was successful or unsuccessful.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="installers"/> is <c>null</c>.
        /// </exception>
        public Results SortDependencies(IEnumerable<IServiceInstaller> installers)
        {
            if (installers == null) {
                throw new ArgumentNullException("installers");
            }

            var graph = ServiceDependencyGraph.Create(installers);
            var graphNodes = new HashSet<ServiceDependencyGraph.Node>(graph.Nodes);
            return SortDependencies(graphNodes);
        }

        private Results SortDependencies(HashSet<ServiceDependencyGraph.Node> graphNodes)
        {
            // http://en.wikipedia.org/wiki/Topological_sorting
            var L = new List<ServiceDependencyGraph.Node>();
            var S = new HashSet<ServiceDependencyGraph.Node>(graphNodes.Where(n => n.Inputs.Count == 0));

            while (S.Count != 0) {
                var n = S.First();
                S.Remove(n);
                L.Add(n);

                foreach (var m in graphNodes) {
                    if (m.Inputs.Contains(n)) {
                        // Remove edge from graph.
                        n.Targets.Remove(m);
                        m.Inputs.Remove(n);

                        if (m.Inputs.Count == 0) {
                            S.Add(m);
                        }
                    }
                }

                // This node is disconnected from graph, it should no longer be considered!
                if (n.Targets.Count == 0) {
                    graphNodes.Remove(n);
                }
            }

            // Prepare results!
            var sortedInstallers = (from n in L select n.Installer).Reverse().ToArray();
            RemoveNonCyclicNodesFromGraph(graphNodes);
            return new Results(sortedInstallers, graphNodes);
        }

        private void RemoveNonCyclicNodesFromGraph(HashSet<ServiceDependencyGraph.Node> graphNodes)
        {
            var nonCyclicNodes = from n in graphNodes where n.Inputs.Count == 0 select n;
            foreach (var n in nonCyclicNodes) {
                // Remove input connections of resulting nodes so that graph only
                // contains nodes that have circular dependencies.
                foreach (var target in n.Targets) {
                    target.Inputs.Remove(n);
                }

                graphNodes.Remove(n);
            }
        }


        /// <summary>
        /// Results produced by <see cref="ServiceDependencySorter.SortDependencies(IEnumerable{IServiceInstaller})"/>.
        /// </summary>
        public sealed class Results
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Results"/> class.
            /// </summary>
            /// <param name="sortedInstallers">Sorted array of service installers.</param>
            /// <param name="circularDependencyNodes">Collection of zero or more nodes
            /// that have circular dependencies.</param>
            public Results(IEnumerable<IServiceInstaller> sortedInstallers, IEnumerable<ServiceDependencyGraph.Node> circularDependencyNodes)
            {
                SortedInstallers = sortedInstallers;
                CircularDependencyNodes = circularDependencyNodes.ToArray();
            }


            /// <summary>
            /// Gets the sorted array of services where early dependencies occur first.
            /// </summary>
            public IEnumerable<IServiceInstaller> SortedInstallers { get; private set; }
            /// <summary>
            /// Gets a collection of <see cref="ServiceDependencyGraph.Node"/> instances
            /// that have circular dependencies.
            /// </summary>
            public IEnumerable<ServiceDependencyGraph.Node> CircularDependencyNodes { get; private set; }

            /// <summary>
            /// Gets a value indicating whether one or more circular dependencies were
            /// encountered.
            /// </summary>
            public bool HasCircularDependency {
                get { return CircularDependencyNodes.Any(); }
            }


            /// <summary>
            /// Generate error message which highlights services with circular dependencies.
            /// </summary>
            /// <remarks>
            /// <para>You can check the value of <see cref="HasCircularDependency"/> to
            /// determine whether circular dependencies hvae been detected before using
            /// this method.</para>
            /// </remarks>
            /// <param name="contextInstaller">Circular dependencies are reported if related
            /// to the specified context service installer.</param>
            /// <returns>
            /// A string containing an error message when circular service dependencies
            /// have been detected; otherwise, a value of <c>null</c>.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            /// If <paramref name="contextInstaller"/> is a value of <c>null</c>.
            /// </exception>
            /// <seealso cref="HasCircularDependency"/>
            public string GenerateCircularDependencyErrorMessage(IServiceInstaller contextInstaller)
            {
                if (contextInstaller == null) {
                    throw new ArgumentNullException("contextInstaller");
                }

                var circularNode = CircularDependencyNodes.FirstOrDefault(n => n.Installer == contextInstaller);
                if (circularNode != null) {
                    // Produce list of other services that form circular dependency.
                    var circular = new HashSet<ServiceDependencyGraph.Node>();
                    AddCircularDependencies(circularNode, circular);
                    circular.Remove(circularNode);

                    var sb = new StringBuilder();
                    sb.AppendLine(circular.Count > 1 ? "Has Circular Dependencies:" : "Has Circular Dependency:");
                    sb.AppendLine();

                    // Present immediate dependencies first.
                    foreach (var n in circularNode.Targets) {
                        sb.AppendLine("  ➜ " + n.Installer.TargetService.ServiceType.FullName);
                        circular.Remove(n);
                    }

                    // Then present subsequent dependencies.
                    foreach (var n in circular) {
                        sb.AppendLine("  → " + n.Installer.TargetService.ServiceType.FullName);
                    }

                    return sb.ToString();
                }
                else {
                    return null;
                }
            }

            /// <summary>
            /// Generate error message which highlights services with circular dependencies.
            /// </summary>
            /// <remarks>
            /// <para>You can check the value of <see cref="HasCircularDependency"/> to
            /// determine whether circular dependencies hvae been detected before using
            /// this method.</para>
            /// </remarks>
            /// <returns>
            /// A string containing an error message when circular service dependencies
            /// have been detected; otherwise, a value of <c>null</c>.
            /// </returns>
            /// <seealso cref="HasCircularDependency"/>
            public string GenerateCircularDependencyErrorMessage()
            {
                var sb = new StringBuilder();
                sb.AppendLine(CircularDependencyNodes.Count() > 2 ? "Has circular dependencies" : "Has circular dependency");
                sb.AppendLine("Select this log entry to see circular dependencies:");

                var remainingCircularNodes = new HashSet<ServiceDependencyGraph.Node>(CircularDependencyNodes);

                while (remainingCircularNodes.Count != 0) {
                    var circularNode = remainingCircularNodes.First();
                    remainingCircularNodes.Remove(circularNode);

                    sb.AppendLine();
                    sb.AppendLine(circularNode.Installer.TargetService.ServiceType.FullName);

                    // Produce list of other services that form circular dependency.
                    var circular = new HashSet<ServiceDependencyGraph.Node>();
                    AddCircularDependencies(circularNode, circular);
                    circular.Remove(circularNode);

                    // Present immediate dependencies first.
                    foreach (var n in circularNode.Targets) {
                        sb.AppendLine("  ➜ " + n.Installer.TargetService.ServiceType.FullName);
                        circular.Remove(n);
                        remainingCircularNodes.Remove(n);
                    }

                    // Then present subsequent dependencies.
                    foreach (var n in circular) {
                        sb.AppendLine("  → " + n.Installer.TargetService.ServiceType.FullName);
                        remainingCircularNodes.Remove(n);
                    }
                }

                return sb.ToString();
            }

            private static void AddCircularDependencies(ServiceDependencyGraph.Node node, HashSet<ServiceDependencyGraph.Node> circular)
            {
                circular.Add(node);

                foreach (var target in node.Targets) {
                    if (!circular.Contains(target)) {
                        AddCircularDependencies(target, circular);
                    }
                }
            }
        }
    }
}
