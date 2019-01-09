using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WwfHelper
{
    class Helpers
    {
        public static Dictionary<char, int> LetterPoints = new Dictionary<char, int>
        {
            {'a', 1},
            {'b', 4},
            {'c', 4},
            {'d', 2},
            {'e', 1},
            {'f', 4},
            {'g', 3},
            {'h', 3},
            {'i', 1},
            {'j', 10},
            {'k', 5},
            {'l', 2},
            {'m', 4},
            {'n', 2},
            {'o', 1},
            {'p', 4},
            {'q', 10},
            {'r', 1},
            {'s', 1},
            {'t', 1},
            {'u', 2},
            {'v', 5},
            {'w', 4},
            {'x', 8},
            {'y', 3},
            {'z', 10}
        };

        private static void Swap(ref char a, ref char b)
        {
            if (a == b) return;

            a ^= b;
            b ^= a;
            a ^= b;
        }

        public static IEnumerable<string> GetPermutations(char[] list)
        {
            int x = list.Length - 1;
            var l = new List<string>();
            GetPer(list, 0, x, ref l);

            return l;
        }

        private static void GetPer(char[] list, int k, int m, ref List<string> perms)
        {
            if (k == m)
            {
                perms.Add(new string(list));
            }
            else
                for (int i = k; i <= m; i++)
                {
                    Swap(ref list[k], ref list[i]);
                    GetPer(list, k + 1, m, ref perms);
                    Swap(ref list[k], ref list[i]);
                }
        }
    }

    static class Extensions
    {
        public static void AddAll<T>(this ICollection<T> list, IEnumerable<T> other)
        {
            foreach (var o in other)
            {
                list.Add(o);
            }
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }

    class WordNode
    {
        public Dictionary<string, WordNode> Children { get; } = new Dictionary<string, WordNode>();

        public string Word { get; set; }
        public bool IsWord { get; }

        public WordNode(string word, bool isWord)
        {
            Word = word;
            IsWord = isWord;
        }

        public WordNode this[string key] => Children[key];

        public bool HasWord(string word)
        {
            if (this.Word == word && this.IsWord)
                return true;

            if (word.Length > this.Word.Length)
            {
                var subWord = word.Substring(0, this.Word.Length + 1);
                if (this.Children.Any() && this.Children.ContainsKey(subWord))
                {
                    return this.Children[subWord].HasWord(word);
                }
            }

            return false;
        }

        public WordNode GetOrAddChild(string key, bool isWord)
        {
            if (!Children.ContainsKey(key))
                Children.Add(key, new WordNode(key, isWord));

            return Children[key];
        }

        public override int GetHashCode()
        {
            return Word.GetHashCode();
        }
    }

    public class ConcurrentHashSet<T> : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly HashSet<T> _hashSet = new HashSet<T>();

        public List<T> ToList()
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.ToList();
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        } 

        #region Implementation of ICollection<T> ...ish
        public bool Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Add(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.Clear();
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Contains(item);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Remove(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _hashSet.Count;
                }
                finally
                {
                    if (_lock.IsReadLockHeld) _lock.ExitReadLock();
                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (_lock != null)
                    _lock.Dispose();
        }
        ~ConcurrentHashSet()
        {
            Dispose(false);
        }
        #endregion
    }
}
