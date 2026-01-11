using UnityEngine;
using System.Collections.Generic;

namespace PxP.PCG
{
    public class UnionFind<T>
    {
        private readonly Dictionary<T, T> parent;
        private readonly Dictionary<T, int> rank;

        public UnionFind(IEnumerable<T> nodes)
        {
            parent = new Dictionary<T, T>();
            rank = new Dictionary<T, int>();

            foreach (var n in nodes)
            {
                parent[n] = n;
                rank[n] = 0;
            }
        }

        public T Find(T x)
        {
            if (!parent[x].Equals(x))
                parent[x] = Find(parent[x]);

            return parent[x];
        }

        public bool Union(T a, T b)
        {
            T rootA = Find(a);
            T rootB = Find(b);

            if (rootA.Equals(rootB))
                return false;

            if (rank[rootA] < rank[rootB])
                parent[rootA] = rootB;
            else if (rank[rootA] > rank[rootB])
                parent[rootB] = rootA;
            else
            {
                parent[rootB] = rootA;
                rank[rootA]++;
            }

            return true;
        }
    }
}
