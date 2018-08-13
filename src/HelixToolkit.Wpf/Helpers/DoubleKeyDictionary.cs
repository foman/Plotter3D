﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleKeyDictionary.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A double key dictionary.
    /// </summary>
    /// <typeparam name="K">
    /// The first key type.
    /// </typeparam>
    /// <typeparam name="T">
    /// The second key type.
    /// </typeparam>
    /// <typeparam name="V">
    /// The value type.
    /// </typeparam>
    /// <remarks>
    /// See http://noocyte.wordpress.com/2008/02/18/double-key-dictionary/
    /// A Remove method was added.
    /// </remarks>
    public class DoubleKeyDictionary<K, T, V> : IEnumerable<DoubleKeyPairValue<K, T, V>>,
                                                IEquatable<DoubleKeyDictionary<K, T, V>>
    {
        /// <summary>
        /// The m_inner dictionary.
        /// </summary>
        private Dictionary<T, V> m_innerDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleKeyDictionary{K,T,V}"/> class.
        /// </summary>
        public DoubleKeyDictionary()
        {
            this.OuterDictionary = new Dictionary<K, Dictionary<T, V>>();
        }

        /// <summary>
        /// Gets or sets OuterDictionary.
        /// </summary>
        private Dictionary<K, Dictionary<T, V>> OuterDictionary { get; set; }

        /// <summary>
        /// Gets or sets the value with the specified indices.
        /// </summary>
        /// <value></value>
        public V this[K index1, T index2]
        {
            get
            {
                return this.OuterDictionary[index1][index2];
            }

            set
            {
                this.Add(index1, index2, value);
            }
        }

        /// <summary>
        /// Clears this dictionary.
        /// </summary>
        public void Clear()
        {
            OuterDictionary.Clear();
            if (m_innerDictionary!=null)
                m_innerDictionary.Clear();
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key1">
        /// The key1.
        /// </param>
        /// <param name="key2">
        /// The key2.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void Add(K key1, T key2, V value)
        {
            if (this.OuterDictionary.ContainsKey(key1))
            {
                if (this.m_innerDictionary.ContainsKey(key2))
                {
                    this.OuterDictionary[key1][key2] = value;
                }
                else
                {
                    this.m_innerDictionary = this.OuterDictionary[key1];
                    this.m_innerDictionary.Add(key2, value);
                    this.OuterDictionary[key1] = this.m_innerDictionary;
                }
            }
            else
            {
                this.m_innerDictionary = new Dictionary<T, V>();
                this.m_innerDictionary[key2] = value;
                this.OuterDictionary.Add(key1, this.m_innerDictionary);
            }
        }

        /// <summary>
        /// Determines whether the specified dictionary contains the key.
        /// </summary>
        /// <param name="index1">
        /// The index1.
        /// </param>
        /// <param name="index2">
        /// The index2.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified index1 contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(K index1, T index2)
        {
            if (!this.OuterDictionary.ContainsKey(index1))
            {
                return false;
            }

            if (!this.OuterDictionary[index1].ContainsKey(index2))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The equals.
        /// </returns>
        public bool Equals(DoubleKeyDictionary<K, T, V> other)
        {
            if (this.OuterDictionary.Keys.Count != other.OuterDictionary.Keys.Count)
            {
                return false;
            }

            bool isEqual = true;

            foreach (var innerItems in this.OuterDictionary)
            {
                if (!other.OuterDictionary.ContainsKey(innerItems.Key))
                {
                    isEqual = false;
                }

                if (!isEqual)
                {
                    break;
                }

                // here we can be sure that the key is in both lists,
                // but we need to check the contents of the inner dictionary
                Dictionary<T, V> otherInnerDictionary = other.OuterDictionary[innerItems.Key];
                foreach (var innerValue in innerItems.Value)
                {
                    if (!otherInnerDictionary.ContainsValue(innerValue.Value))
                    {
                        isEqual = false;
                    }

                    if (!otherInnerDictionary.ContainsKey(innerValue.Key))
                    {
                        isEqual = false;
                    }
                }

                if (!isEqual)
                {
                    break;
                }
            }

            return isEqual;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// </returns>
        public IEnumerator<DoubleKeyPairValue<K, T, V>> GetEnumerator()
        {
            foreach (var outer in this.OuterDictionary)
            {
                foreach (var inner in outer.Value)
                {
                    yield return new DoubleKeyPairValue<K, T, V>(outer.Key, inner.Key, inner.Value);
                }
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key1">
        /// The key1.
        /// </param>
        /// <param name="key2">
        /// The key2.
        /// </param>
        public void Remove(K key1, T key2)
        {
            this.OuterDictionary[key1].Remove(key2);
            if (this.OuterDictionary[key1].Count == 0)
            {
                this.OuterDictionary.Remove(key1);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }

    /// <summary>
    /// Represents two keys and a value.
    /// </summary>
    /// <typeparam name="K">
    /// First key type.
    /// </typeparam>
    /// <typeparam name="T">
    /// Second key type.
    /// </typeparam>
    /// <typeparam name="V">
    /// Value type.
    /// </typeparam>
    public class DoubleKeyPairValue<K, T, V>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleKeyPairValue{K,T,V}"/> class.
        /// </summary>
        /// <param name="key1">
        /// The key1.
        /// </param>
        /// <param name="key2">
        /// The key2.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public DoubleKeyPairValue(K key1, T key2, V value)
        {
            this.Key1 = key1;
            this.Key2 = key2;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the key1.
        /// </summary>
        /// <value>The key1.</value>
        public K Key1 { get; set; }

        /// <summary>
        /// Gets or sets the key2.
        /// </summary>
        /// <value>The key2.</value>
        public T Key2 { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public V Value { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Key1 + " - " + this.Key2 + " - " + this.Value;
        }

    }
}