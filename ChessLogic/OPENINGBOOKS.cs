using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; // Required for .Union().ToList()

namespace ChessLogic
{
    public static class OpeningBook
    {
        private static readonly Dictionary<string, List<string>> Book = new()
        {
            // Starting moves
            { "", new List<string> { "e2e4", "d2d4", "g1f3", "c2c4" } },

            // Responses to e4
            { "e2e4", new List<string> { "e7e5", "c7c5", "e7e6", "c7c6", "g7g6", "d7d5", "g8f6", "b8c6" } },
            // Responses to d4
            { "d2d4", new List<string> { "d7d5", "g8f6", "e7e6", "c7c5", "f7f5", "g7g6" } },
            // Responses to Nf3
            { "g1f3", new List<string> { "d7d5", "g8f6", "c7c5", "e7e6", "g7g6" } },
            // Responses to c4
            { "c2c4", new List<string> { "e7e5", "c7c5", "g8f6", "e7e6", "g7g6", "f7f5" } },

            // Sicilian Defense (Aggressive/Sharp)
            { "e2e4 c7c5", new List<string> { "g1f3", "b1c3", "c2c3", "d2d4", "f2f4" } },
            { "e2e4 c7c5 g1f3", new List<string> { "d7d6", "e7e6", "b8c6" } },
            { "e2e4 c7c5 g1f3 d7d6", new List<string> { "d2d4", "f1b5", "f1c4" } },
            { "e2e4 c7c5 g1f3 d7d6 d2d4", new List<string> { "c5d4" } },
            { "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4", new List<string> { "f3d4", "d1d4" } },
            { "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4 f3d4", new List<string> { "g8f6" } },
            { "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4 f3d4 g8f6", new List<string> { "b1c3" } },
            { "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4 f3d4 g8f6 b1c3", new List<string> { "a7a6", "g7g6", "e7e6" } },
            { "e2e4 c7c5 g1f3 e7e6", new List<string> { "d2d4", "c2c3", "b2b4" } },
            { "e2e4 c7c5 g1f3 b8c6", new List<string> { "d2d4", "f1b5" } },
            { "e2e4 c7c5 g1f3 b8c6 d2d4", new List<string> {"c5d4"} },
            { "e2e4 c7c5 g1f3 b8c6 d2d4 c5d4", new List<string> {"f3d4"} },
            { "e2e4 c7c5 g1f3 b8c6 d2d4 c5d4 f3d4", new List<string> {"g8f6", "e7e5", "g7g6"} },

            // King's Pawn Opening (e4 e5) - Aggressive Lines
            { "e2e4 e7e5", new List<string> { "g1f3", "f1c4", "d2d4", "b1c3", "f2f4" } },
            { "e2e4 e7e5 g1f3", new List<string> { "b8c6", "g8f6", "d7d6" } },
            { "e2e4 e7e5 g1f3 b8c6", new List<string> { "f1c4", "f1b5", "d2d4" } },
            { "e2e4 e7e5 g1f3 b8c6 f1c4", new List<string> { "f8c5", "g8f6", "d7d6", "b7b5" } },
            { "e2e4 e7e5 g1f3 b8c6 f1c4 g8f6", new List<string> { "d2d3", "d2d4", "e1g1", "g1g5"} },
            { "e2e4 e7e5 g1f3 b8c6 f1b5", new List<string> { "a7a6", "g8f6", "d7d6", "f7f5" } },
            { "e2e4 e7e5 f2f4", new List<string> { "e5f4", "d7d5" } },
            { "e2e4 e7e5 f2f4 e5f4", new List<string> { "g1f3", "f1c4", "b1c3"} },

            // French Defense
            { "e2e4 e7e6", new List<string> { "d2d4", "d2d3" } },
            { "e2e4 e7e6 d2d4", new List<string> { "d7d5" } },
            { "e2e4 e7e6 d2d4 d7d5", new List<string> { "b1c3", "b1d2", "e4d5", "e4e5" } },
            { "e2e4 e7e6 d2d4 d7d5 b1c3", new List<string> { "g8f6", "f8b4", "d5e4" } },
            { "e2e4 e7e6 d2d4 d7d5 b1c3 g8f6", new List<string> { "e4e5", "c1g5" } },
            { "e2e4 e7e6 d2d4 d7d5 e4e5", new List<string> { "c7c5" } },
            { "e2e4 e7e6 d2d4 d7d5 e4e5 c7c5", new List<string> { "c2c3" } },

            // Caro-Kann Defense
            { "e2e4 c7c6", new List<string> { "d2d4", "d2d3", "b1c3"} },
            { "e2e4 c7c6 d2d4", new List<string> { "d7d5" } },
            { "e2e4 c7c6 d2d4 d7d5", new List<string> { "b1c3", "e4d5", "e4e5", "f2f3" } },
            { "e2e4 c7c6 d2d4 d7d5 b1c3", new List<string> { "d5e4" } },
            { "e2e4 c7c6 d2d4 d7d5 b1c3 d5e4", new List<string> { "c3e4" } },
            { "e2e4 c7c6 d2d4 d7d5 b1c3 d5e4 c3e4", new List<string> { "b8d7", "c8f5", "g8f6" } },

            // --- QUEEN'S GAMBIT SECTION ---
            { "d2d4 d7d5 c2c4", new List<string> { "e7e6", "c7c6", "d5c4", "b8c6", "e7e5" } }, 

            // Queen's Gambit Declined (QGD)
            { "d2d4 d7d5 c2c4 e7e6", new List<string> { "b1c3", "g1f3" } },
            { "d2d4 d7d5 c2c4 e7e6 b1c3", new List<string> { "g8f6", "c7c5", "f8e7" } }, 
            { "d2d4 d7d5 c2c4 e7e6 b1c3 g8f6", new List<string> { "c1g5", "g1f3", "c4d5" } }, 
            { "d2d4 d7d5 c2c4 e7e6 b1c3 g8f6 c1g5", new List<string> { "f8e7", "b8d7", "c7c6" } },
            { "d2d4 d7d5 c2c4 e7e6 b1c3 g8f6 c1g5 f8e7", new List<string> { "e2e3", "g1f3", "c4d5", "a1c1" } },
            { "d2d4 d7d5 c2c4 e7e6 b1c3 g8f6 c1g5 f8e7 e2e3", new List<string> { "e8g8", "b8d7", "c7c6" } },
            { "d2d4 d7d5 c2c4 e7e6 b1c3 g8f6 c1g5 f8e7 e2e3 e8g8", new List<string> { "g1f3", "a1c1" } },
            { "d2d4 d7d5 c2c4 e7e6 b1c3 g8f6 c1g5 f8e7 e2e3 e8g8 g1f3", new List<string> { "b8d7", "c7c5", "c7c6", "h7h6" } },

            // Queen's Gambit Accepted (QGA)
            { "d2d4 d7d5 c2c4 d5c4", new List<string> { "e2e4", "e2e3", "g1f3" } },
            { "d2d4 d7d5 c2c4 d5c4 e2e4", new List<string> { "g8f6", "e7e5", "c7c5", "b7b5", "b8c6" } },
            { "d2d4 d7d5 c2c4 d5c4 e2e3", new List<string> { "e7e5", "g8f6", "b7b5", "c7c5" } },
            { "d2d4 d7d5 c2c4 d5c4 g1f3", new List<string> { "g8f6", "a7a6", "e7e6" } },
            { "d2d4 d7d5 c2c4 d5c4 g1f3 g8f6", new List<string> { "e2e3", "b1c3" } },
            { "d2d4 d7d5 c2c4 d5c4 g1f3 g8f6 e2e3", new List<string> { "e7e6", "c7c5", "b7b5", "f8g7" } },
            
            // Slav Defense
            // Key: 1. d4 d5 2. c4 c6
            { "d2d4 d7d5 c2c4 c7c6", new List<string> { "g1f3", "b1c3", "e2e3", "c4d5" } },
            // Key: 1. d4 d5 2. c4 c6 3. Nf3
            { "d2d4 d7d5 c2c4 c7c6 g1f3", new List<string> { "g8f6" } },
            // Key: 1. d4 d5 2. c4 c6 3. Nf3 Nf6
            { "d2d4 d7d5 c2c4 c7c6 g1f3 g8f6", new List<string> { "b1c3", "e2e3" } },
            // Key: 1. d4 d5 2. c4 c6 3. Nf3 Nf6 4. Nc3
            { "d2d4 d7d5 c2c4 c7c6 g1f3 g8f6 b1c3", new List<string> { "d5c4", "e7e6", "a7a6", "g7g6" } }, 
            // Key: 1. d4 d5 2. c4 c6 3. Nf3 Nf6 4. Nc3 dxc4 (Main Line Slav Accepted)
            { "d2d4 d7d5 c2c4 c7c6 g1f3 g8f6 b1c3 d5c4", new List<string> { "a2a4", "e2e3", "e2e4" } },
            // Key: 1. d4 d5 2. c4 c6 3. Nf3 Nf6 4. Nc3 e6 (Semi-Slav)
            { "d2d4 d7d5 c2c4 c7c6 g1f3 g8f6 b1c3 e7e6", new List<string> { "e2e3", "c1g5" } }, 
            // --- END QUEEN'S GAMBIT SECTION ---

            // Indian Defenses (d4 Nf6) - More lines
            { "d2d4 g8f6", new List<string> { "c2c4", "g1f3", "c1g5" } },
            { "d2d4 g8f6 c2c4", new List<string> { "g7g6", "e7e6", "c7c5" } },
            { "d2d4 g8f6 c2c4 g7g6", new List<string> { "b1c3", "g2g3", "g1f3" } },
            { "d2d4 g8f6 c2c4 g7g6 b1c3", new List<string> { "f8g7", "d7d5" } },
            { "d2d4 g8f6 c2c4 g7g6 b1c3 f8g7", new List<string> { "e2e4" } },
            { "d2d4 g8f6 c2c4 g7g6 b1c3 f8g7 e2e4", new List<string> { "d7d6" } },
            { "d2d4 g8f6 c2c4 g7g6 b1c3 f8g7 e2e4 d7d6", new List<string> { "g1f3", "f2f3", "c1e3"} },

            // Aggressive openings from user file
            { "e2e4 e7e5 d2d4 e5d4", new List<string> { "c2c3", "d1d4" } },
            { "e2e4 e7e5 d2d4 e5d4 c2c3", new List<string> { "d4c3", "d7d5" } },
            { "e2e4 e7e5 d2d4 e5d4 c2c3 d4c3", new List<string> { "f1c4", "b1c3"} },
            { "e2e4 e7e5 f1c4", new List<string> { "b8c6", "g8f6", "c7c6" } },
        };

        private static readonly Random rng = new();

        private static void AddOrUpdateEntry(string key, List<string> newResponses)
        {
            if (Book.TryGetValue(key, out var existingResponses))
            {
                Book[key] = existingResponses.Union(newResponses).ToList();
            }
            else
            {
                Book[key] = newResponses;
            }
        }

        public static string GetBookMove(List<string> history)
        {
            string key = string.Join(" ", history);
            //Debug.WriteLine($"[Book Lookup] Trying key: '{key}'");

            if (Book.TryGetValue(key, out var responses) && responses.Count > 0)
            {
                string move = responses[rng.Next(responses.Count)];
                //Debug.WriteLine($"[Book Hit] Found: {move} for key '{key}'");
                return move;
            }

            //Debug.WriteLine("[Book Miss] No entry found.");
            return null;
        }
    }
}
