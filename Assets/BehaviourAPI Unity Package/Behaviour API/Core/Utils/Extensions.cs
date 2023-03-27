using BehaviourAPI.Core.Perceptions;
using System.Collections.Generic;

namespace BehaviourAPI.Core
{
    /// <summary>
    /// Extension method class.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Invert the <see cref="Status"/> value (<see cref="Status.Success"/> --- <see cref="Status.Failure"/>).
        /// </summary>
        /// <param name="status">The status value</param>
        /// <returns>The value inverted.</returns>
        public static Status Inverted(this Status status)
        {
            if (status == Status.Success) return Status.Failure;
            if (status == Status.Failure) return Status.Success;
            else return status;
        }

        /// <summary>
        /// Cast a boolean value into a <see cref="Status"/> value. 
        /// if true, return <see cref="Status.Success"/>.
        /// If false, return the value specified in <paramref name="valueIfFalse"/>.
        /// </summary>
        /// <param name="check">The boolean value.</param>
        /// <param name="valueIfFalse">The returned value if boolean is false.</param>
        /// <returns>The converted status value.</returns>
        public static Status ToStatus(this bool check, Status valueIfFalse = Status.Failure)
        {
            return check ? Status.Success : valueIfFalse;
        }

        /// <summary>
        /// Move an element of the list at the first position.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="element">The element moved.</param>
        public static void MoveAtFirst<T>(this List<T> list, T element)
        {
            if(list.Remove(element)) list.Insert(0, element);
        }
    }
}
