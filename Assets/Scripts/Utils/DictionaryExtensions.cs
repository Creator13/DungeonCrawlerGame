using System.Collections.Generic;

namespace EditorUtils
{
    public static class DictionaryExtensions
    {
        public static void Deconstruct<T, U>(this KeyValuePair<T, U> k, out T t, out U u)
        {
            t = k.Key;
            u = k.Value;
        }
    }
}
