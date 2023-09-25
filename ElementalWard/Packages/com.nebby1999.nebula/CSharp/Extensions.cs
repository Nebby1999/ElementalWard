using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nebula
{
    public static class Extensions
    {
        /// <summary>
        /// Ensures that the string object is not Null, Empty or WhiteSpace.
        /// </summary>
        /// <param name="text">The string object to check</param>
        /// <returns>True if the string object is not Null, Empty or Whitespace, false otherwise.</returns>
        public static bool IsNullOrWhiteSpace(this string text)
        {
            return (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text));
        }

        public static string WithAllWhitespaceStripped(this string str)
        {
            var buffer = new StringBuilder();
            foreach (var ch in str)
                if (!char.IsWhiteSpace(ch))
                    buffer.Append(ch);
            return buffer.ToString();
        }

        /// <summary>
        /// Extension to allow tuple style deconstruction of keys and values when enumerating a dictionary.
        /// Example: foreach(var (key, value) in myDictionary)
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="kvp"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        public static ref T NextElementUniform<T>(this T[] array, Xoroshiro128Plus rng)
        {
            return ref rng.NextElementUniform(array);
        }

        public static T NextElementUniform<T>(this List<T> list, Xoroshiro128Plus rng)
        {
            return rng.NextElementUniform(list);
        }

        public static T NextElementUniform<T>(this IList<T> list, Xoroshiro128Plus rng)
        {
            return rng.NextElementUniform(list);
        }

        public static T RetrieveAndRemoveNextElementUniform<T>(this T[] array, Xoroshiro128Plus rng)
        {
            return rng.RetrieveAndRemoveNextElementUniform(array);
        }

        public static T RetrieveAndRemoveNextElementUniform<T>(this List<T> list, Xoroshiro128Plus rng)
        {
            return rng.RetrieveAndRemoveNextElementUniform(list);
        }

        public static T RetrieveAndRemoveNextElementUniform<T>(this IList<T> list, Xoroshiro128Plus rng)
        {
            return rng.RetrieveAndRemoveNextElementUniform(list);
        }
    }
}
