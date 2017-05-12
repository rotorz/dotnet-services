// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;

namespace Rotorz.Games.Services
{
    /// <summary>
    /// Interface for the installer of a service.
    /// </summary>
    public interface IServiceInstaller
    {
        /// <summary>
        /// Gets an object identifying the service that the <see cref="IServiceInstaller"/>
        /// is designed to
        /// install.
        /// </summary>
        IServiceDescriptor TargetService { get; }


        /// <summary>
        /// Gets the dependencies of the service installer.
        /// </summary>
        /// <returns>
        /// An enumerable collection of services.
        /// </returns>
        IEnumerable<IServiceDescriptor> GetDependencies();

        /// <summary>
        /// Installs the service.
        /// </summary>
        void InstallBindings();
    }
}
