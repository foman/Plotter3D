﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelfProvider.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System;
    using System.Windows.Markup;

    /// <summary>
    /// A MarkupExtension that provides the instance itself.
    /// </summary>
    /// <remarks>
    /// This should not be used if multiple instances are created. In that case create a resource instead.
    /// </remarks>
    public abstract class SelfProvider : MarkupExtension
    {
        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">
        /// Object that can provide services for the markup extension.
        /// </param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

    }
}