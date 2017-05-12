// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotorz.Games.Services
{
    /// <summary>
    /// Graph of service dependency nodes.
    /// </summary>
    public sealed class ServiceDependencyGraph
    {
        /// <summary>
        /// Create service dependency graph from the specified array of service definitions.
        /// </summary>
        /// <param name="services">Array of services.</param>
        /// <returns>
        /// A <see cref="ServiceDependencyGraph"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="services"/> is <c>null</c>.
        /// </exception>
        public static ServiceDependencyGraph Create(IEnumerable<IServiceInstaller> services)
        {
            if (services == null) {
                throw new ArgumentNullException("services");
            }

            // Generate nodes along with mapping between definition and nodes.
            var map = services.ToDictionary(x => x.TargetService, x => new Node(x));

            // Form edges between nodes.
            foreach (var node in map.Values) {
                foreach (var dependencyDescriptor in node.Installer.GetDependencies()) {
                    Node dependencyNode;
                    if (map.TryGetValue(dependencyDescriptor, out dependencyNode)) {
                        node.Targets.Add(dependencyNode);
                        dependencyNode.Inputs.Add(node);
                    }
                }
            }

            var graph = new ServiceDependencyGraph();
            graph.Nodes = new List<Node>(map.Values);
            return graph;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDependencyGraph"/> class.
        /// </summary>
        public ServiceDependencyGraph()
        {
            Nodes = new List<Node>();
        }


        /// <summary>
        /// Gets an editable collection of nodes which form graph.
        /// </summary>
        public IList<Node> Nodes { get; private set; }


        /// <summary>
        /// Node representing a service in a <see cref="ServiceDependencyGraph"/>.
        /// </summary>
        public sealed class Node
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class.
            /// </summary>
            /// <param name="installer">Associated service installer.</param>
            /// <exception cref="System.ArgumentNullException">
            /// If <paramref name="installer"/> is <c>null</c>.
            /// </exception>
            public Node(IServiceInstaller installer)
            {
                if (installer == null) {
                    throw new ArgumentNullException("installer");
                }

                this.Installer = installer;
                this.Inputs = new HashSet<Node>();
                this.Targets = new HashSet<Node>();
            }


            /// <summary>
            /// Gets the service that is associated with this node.
            /// </summary>
            public IServiceInstaller Installer { get; private set; }
            /// <summary>
            /// Gets an editable collection of nodes leading into this node.
            /// </summary>
            public ICollection<Node> Inputs { get; private set; }
            /// <summary>
            /// Gets an editable collection of nodes targeted by this node.
            /// </summary>
            public ICollection<Node> Targets { get; private set; }
        }
    }
}
