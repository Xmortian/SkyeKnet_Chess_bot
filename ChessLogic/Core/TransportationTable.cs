using System;
using System.Collections.Generic;
using ChessLogic;

namespace ChessLogic.Core.Ai
{
    public enum NodeType { Exact, LowerBound, UpperBound }

    public struct TTEntry
    {
        public int Depth;
        public int Value;
        public NodeType Type;
    }

    public static class TranspositionTable
    {
        private static readonly Dictionary<ulong, TTEntry> table = new();

        public static void Store(ulong zobristHash, int depth, int value, NodeType type)
        {
            if (table.TryGetValue(zobristHash, out TTEntry existing))
            {
                if (existing.Depth > depth) return; // keep deeper entry
            }
            table[zobristHash] = new TTEntry { Depth = depth, Value = value, Type = type };
        }

        public static bool TryGet(ulong zobristHash, int depth, int alpha, int beta, out int value)
        {
            if (table.TryGetValue(zobristHash, out TTEntry entry))
            {
                if (entry.Depth >= depth)
                {
                    switch (entry.Type)
                    {
                        case NodeType.Exact:
                            value = entry.Value;
                            return true;
                        case NodeType.LowerBound:
                            if (entry.Value >= beta)
                            {
                                value = entry.Value;
                                return true;
                            }
                            break;
                        case NodeType.UpperBound:
                            if (entry.Value <= alpha)
                            {
                                value = entry.Value;
                                return true;
                            }
                            break;
                    }
                }
            }
            value = 0;
            return false;
        }

        public static void Clear() => table.Clear();
    }
}
