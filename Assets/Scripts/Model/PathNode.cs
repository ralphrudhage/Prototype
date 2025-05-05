using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class PathNode : IComparable<PathNode>
    {
        public Vector2Int pos;
        public int g;
        private int f;
        public readonly List<Vector2Int> path;

        public PathNode(Vector2Int pos, int g, int f, List<Vector2Int> path)
        {
            this.pos = pos;
            this.g = g;
            this.f = f;
            this.path = path;
        }

        public int CompareTo(PathNode other)
        {
            return f.CompareTo(other.f);
        }
    }
}
