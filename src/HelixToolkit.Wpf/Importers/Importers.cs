﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Importers.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    /// <summary>
    /// Contains a list of all supported importers.
    /// </summary>
    public static class Importers
    {
        /// <summary>
        /// Default file import extension.
        /// </summary>
        public static readonly string DefaultExtension = ".obj";

        /// <summary>
        /// File filter for all the supported importers.
        /// </summary>
        public static readonly string Filter =
            "All supported files|*.3ds;*.lwo;*.obj;*.objx;*.stl;*.off|3D Studio (*.3ds)|*.3ds|Lightwave (*.lwo)|*.lwo|Wavefront (*.obj)|*.obj;*.objx|StereoLithography (*.stl)|*.stl|OFF (*.off)|*.off";
    }
}