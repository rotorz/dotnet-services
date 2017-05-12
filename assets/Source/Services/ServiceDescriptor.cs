// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotorz.Games.Services
{
    /// <summary>
    /// An object that describes a <see cref="IService"/>.
    /// </summary>
    public class ServiceDescriptor : IServiceDescriptor
    {
        private static Dictionary<Type, IServiceDescriptor> s_Cache = new Dictionary<Type, IServiceDescriptor>();


        /// <summary>
        /// Gets an array of all the services that have been discovered.
        /// </summary>
        public static IServiceDescriptor[] AllServices {
            get {
                return TypeMeta.DiscoverImplementations<Service>()
                    .Select(serviceType => For(serviceType))
                    .ToArray();
            }
        }


        /// <summary>
        /// Gets the <see cref="IServiceDescriptor"/> that identifies and describes a
        /// specified service.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>
        /// A singleton instance of the <see cref="IServiceDescriptor"/> that uniquely
        /// identifies the specified service.
        /// </returns>
        public static IServiceDescriptor For(Type serviceType)
        {
            IServiceDescriptor service;
            if (!s_Cache.TryGetValue(serviceType, out service)) {
                service = new ServiceDescriptor(serviceType);
                s_Cache[serviceType] = service;
            }
            return service;
        }

        /// <summary>
        /// Gets the <see cref="IServiceDescriptor"/> that identifies and describes a
        /// specified service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>
        /// A singleton instance of the <see cref="IServiceDescriptor"/> that uniquely
        /// identifies the specified service.
        /// </returns>
        public static IServiceDescriptor For<TService>()
            where TService : Service
        {
            return For(typeof(TService));
        }

        /// <summary>
        /// Gets a collection of <see cref="IServiceDescriptor"/> identifying and
        /// describing the service dependencies of a given service or installer type.
        /// </summary>
        /// <param name="type">A type of service of installer.</param>
        /// <returns>
        /// A collection of services that the given type depends upon.
        /// </returns>
        public static IEnumerable<IServiceDescriptor> ForDependenciesOf(Type type)
        {
            return TypeMeta.GetAnnotatedDependencies(type)
                .Select(dependencyType => For(dependencyType));
        }

        /// <summary>
        /// Gets a collection of <see cref="IServiceDescriptor"/> identifying and
        /// describing the service dependencies of a given service or installer type.
        /// </summary>
        /// <typeparam name="T">A type of service of installer.</typeparam>
        /// <returns>
        /// A collection of services that the given type depends upon.
        /// </returns>
        public static IEnumerable<IServiceDescriptor> ForDependenciesOf<T>()
            where T : Service
        {
            return ForDependenciesOf(typeof(T));
        }


        private readonly IServiceDescriptor[] dependencies;


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDescriptor"/> class.
        /// </summary>
        /// <param name="serviceType">The type representing the service.</param>
        public ServiceDescriptor(Type serviceType)
        {
            this.ServiceType = serviceType;
            this.dependencies = ForDependenciesOf(serviceType).ToArray();

            this.Title = TypeMeta.NicifyCompoundName(this.ServiceType.Name, unwantedSuffix: "_Service");
            this.TitleWithNamespace = TypeMeta.NicifyNamespaceQualifiedName(this.ServiceType.Namespace, this.Title);
        }


        /// <inheritdoc/>
        public Type ServiceType { get; private set; }

        /// <inheritdoc/>
        public string Title { get; private set; }

        /// <inheritdoc/>
        public string TitleWithNamespace { get; private set; }


        /// <inheritdoc/>
        public IEnumerable<IServiceDescriptor> GetDependencies()
        {
            return this.dependencies;
        }
    }
}
