﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HelixToolkitException.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// <summary>
//   Represents errors that occurs in the Helix 3D Toolkit.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System;

    /// <summary>
    /// Represents errors that occurs in the Helix 3D Toolkit.
    /// </summary>
    [Serializable]
    public class HelixToolkitException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelixToolkitException"/> class.
        /// </summary>
        /// <param name="formatString">
        /// The format string.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public HelixToolkitException(string formatString, params object[] args)
            : base(string.Format(formatString, args))
        {
        }

    }
}