﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManipulationEventArgs.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System.Windows;

    /// <summary>
    /// Provides data for the manipulation events.
    /// </summary>
    public class ManipulationEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManipulationEventArgs"/> class.
        /// </summary>
        /// <param name="currentPosition">
        /// The current position.
        /// </param>
        public ManipulationEventArgs(Point currentPosition)
        {
            this.CurrentPosition = currentPosition;
        }

        /// <summary>
        /// Gets or sets the current position.
        /// </summary>
        /// <value>The current position.</value>
        public Point CurrentPosition { get; private set; }

    }
}