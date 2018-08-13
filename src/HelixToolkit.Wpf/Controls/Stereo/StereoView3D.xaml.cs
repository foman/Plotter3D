﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StereoView3D.xaml.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    /// <summary>
    /// A stereoscopic Viewport3D control.
    /// </summary>
    public partial class StereoView3D : StereoControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StereoView3D"/> class.
        /// </summary>
        public StereoView3D()
        {
            this.InitializeComponent();
            this.BindViewports(this.LeftView, this.RightView);
        }

    }
}