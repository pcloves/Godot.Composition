﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Godot.Composition
{
    public class ComponentContainer : IEnumerable<IComponent>, ICollection<IComponent>
    {
        private Dictionary<Type, WeakReference<IComponent>> components;

        /// <summary>
        /// Number of components in the container.
        /// </summary>
        public int Count => components.Count;

        /// <summary>
        /// Indicates if the collection is read only or not.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ComponentContainer()
        {
            components = new Dictionary<Type, WeakReference<IComponent>>();
        }

        /// <summary>
        /// Constructor which builds the container based on the given node.
        /// It searches the given node for components to add.
        /// </summary>
        /// <param name="entity">The entity node that contains component nodes.</param>
        public ComponentContainer(Node entity) : this()
        {
            AddEntityComponents(entity);
        }

        public void AddEntityComponents(Node entity)
        {
            foreach (var c in entity.FindDescendantNodesIf(n => n is IComponent).Cast<IComponent>())
                Add(c);
        }

        #region Components
        /// <summary>
        /// Checks to see if the <see cref="IEntity"/> contains a component specified by type T.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Component{T}"/> to check for.</typeparam>
        /// <returns>True if the <see cref="IEntity"/> has the <see cref="Component{T}"/>, False otherwise.</returns>
        public bool HasComponent<T>() where T : Node
        {
            var searchType = typeof(T);
            return components.ContainsKey(searchType);
        }

        /// <summary>
        /// Gets the component specified by type T.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Component{T}"/> to check for.</typeparam>
        /// <returns>
        /// Null if the <see cref="Entity"/> does not have the <see cref="Component{T}"/>, 
        /// otherwise, the <see cref="Component{T}"/>.
        /// </returns>
        public T GetComponent<T>() where T : Node
        {
            var searchType = typeof(T);
            T component = default(T);

            if (components.ContainsKey(searchType))
            {
                if (components[searchType].TryGetTarget(out IComponent c))
                    component = (T)c;
                else
                    components.Remove(searchType);
            }

            return component;
        }
        #endregion

        #region IEnumerableT
        public IEnumerator<IComponent> GetEnumerator()
        {
            foreach (var weakComponent in components.Values)
            {
                if (weakComponent.TryGetTarget(out IComponent c))
                    yield return c;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var weakComponent in components.Values)
            {
                if (weakComponent.TryGetTarget(out IComponent c))
                    yield return c;
            }
        }
        #endregion

        #region ICollectionT
        public void Add(IComponent item)
        {
            if (item is not Node)
                throw new ArgumentException("Component must be a node.", nameof(item));

            if (!components.ContainsKey(item.GetType()))
            {
                var weakRef = new WeakReference<IComponent>(item);
                components.Add(item.GetType(), weakRef);
            }
        }

        public void Clear()
        {
            components.Clear();
        }

        public bool Contains(IComponent item)
        {
            return components.Values.Any(c =>
            {
                bool found = false;
                if (c.TryGetTarget(out IComponent target))
                    found = ReferenceEquals(target, item);
                return found;
            });
        }

        public void CopyTo(IComponent[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            var list = this.ToList();
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(IComponent item)
        {
            return components.Remove(item.GetType());
        }
        #endregion
    }
}
