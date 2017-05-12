// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;

namespace Rotorz.Games.Services
{
    /// <summary>
    /// An object that identifies and describes a <see cref="IService"/>.
    /// </summary>
    public interface IServiceDescriptor
    {
        /// <summary>
        /// Gets the <see cref="Type"/> of the service.
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// Gets the more user friendly title of the service.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the more user friendly title of the service including the namespace name.
        /// </summary>
        string TitleWithNamespace { get; }


        /// <summary>
        /// Gets the collection of services that the service is dependent upon which must
        /// be installed before the service can be used.
        /// </summary>
        /// <returns>
        /// A collection of zero-or-more service descriptors.
        /// </returns>
        IEnumerable<IServiceDescriptor> GetDependencies();
    }
}
