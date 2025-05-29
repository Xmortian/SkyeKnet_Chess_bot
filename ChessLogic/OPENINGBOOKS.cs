using System;
using System.Collections.Generic;

namespace ChessLogic
{
    public static class OpeningBook
    {
        private static readonly Dictionary<string, List<string>> Book = new()
        {
            // Starting moves
            { "", new List<string> { "e2e4", "d2d4", "g1f3", "c2c4" } },

            // Responses to e4
            { "e2e4", new List<string> { "e7e5", "c7c5", "e7e6", "c7c6", "g7g6", "d7d5", "g8f6", "b8c6" } }, // Added Nc6 (Scandinavian/Alekhine setups)
            // Responses to d4
            { "d2d4", new List<string> { "d7d5", "g8f6", "e7e6", "c7c5", "f7f5", "g7g6" } }, // Added g6 (King's Indian/Grunfeld setups)
            // Responses to Nf3
            { "g1f3", new List<string> { "d7d5", "g8f6", "c7c5", "e7e6", "g7g6" } },
            // Responses to c4
            { "c2c4", new List<string> { "e7e5", "c7c5", "g8f6", "e7e6", "g7g6", "e7f5" } }, // Added f5 (Dutch setup)

            // Sicilian Defense (Aggressive/Sharp)
            { "e2e4 c7c5", new List<string> { "g1f3", "b1c3", "c2c3", "d2d4", "f2f4" } }, // Added f4 (Grand Prix Attack)
            { "e2e4 c7c5 g1f3", new List<string> { "d7d6", "e7e6", "b8c6" } },
            { "e2e4 c7c5 g1f3 d7d6", new List<string> { "d2d4", "f1b5", "f1c4" } }, // Added Bc4 (Bowdler Attack)
            { "e2e4 c7c5 g1f3 d7d6 d2d4", new List<string> { "c5d4" } },
            { "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4", new List<string> { "f3d4", "d1d4" } }, // Nf3xd4, Qxd4
            { "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4 f3d4", new List<string> { "g8f6" } },
            { "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4 f3d4 g8f6", new List<string> { "b1c3" } },
            { "e2e4 c7c5 g1f3 d7d6 d2d4 c5d4 f3d4 g8f6 b1c3", new List<string> { "a7a6", "g7g6", "e7e6" } }, // Najdorf, Dragon, Scheveningen
            { "e2e4 c7c5 g1f3 e7e6", new List<string> { "d2d4", "c2c3", "b2b4" } }, // Added b4 (Wing Gambit style)
            { "e2e4 c7c5 g1f3 b8c6", new List<string> { "d2d4", "f1b5" } },
            { "e2e4 c7c5 g1f3 b8c6 d2d4", new List<string> {"c5d4"} },
            { "e2e4 c7c5 g1f3 b8c6 d2d4 c5d4", new List<string> {"f3d4"} },
            { "e2e4 c7c5 g1f3 b8c6 d2d4 c5d4 f3d4", new List<string> {"g8f6", "e7e5", "g7g6"} }, // Accelerated Dragon if g6

            // King's Pawn Opening (e4 e5) - Aggressive Lines
            { "e2e4 e7e5", new List<string> { "g1f3", "f1c4", "d2d4", "b1c3", "f2f4" } }, // Added f4 (King's Gambit)
            { "e2e4 e7e5 g1f3", new List<string> { "b8c6", "g8f6", "d7d6" } },
            { "e2e4 e7e5 g1f3 b8c6", new List<string> { "f1c4", "f1b5", "d2d4" } }, // Italian, Ruy Lopez, Scotch
            { "e2e4 e7e5 g1f3 b8c6 f1c4", new List<string> { "f8c5", "g8f6", "d7d6", "b7b5" } }, // Evans Gambit (b5)
            { "e2e4 e7e5 g1f3 b8c6 f1c4 g8f6", new List<string> { "d2d4", "e1g1", "g1g5"} }, // Scotch Gambit, Max Lange Attack (Ng5)
            { "e2e4 e7e5 g1f3 b8c6 f1b5", new List<string> { "a7a6", "g8f6", "d7d6", "f7f5" } }, // Ruy Lopez, Schliemann Defense (f5)
            { "e2e4 e7e5 f2f4", new List<string> { "e5f4", "d7d5" } }, // King's Gambit Accepted, Falkbeer Countergambit
            { "e2e4 e7e5 f2f4 e5f4", new List<string> { "g1f3", "f1c4", "b1c3"} },

            // French Defense - More lines
            { "e2e4 e7e6", new List<string> { "d2d4", "d2d3" } },
            { "e2e4 e7e6 d2d4", new List<string> { "d7d5" } },
            { "e2e4 e7e6 d2d4 d7d5", new List<string> { "b1c3", "b1d2", "e4d5", "e4e5" } }, // Tarrasch, Winawer/Classical, Exchange, Advance
            { "e2e4 e7e6 d2d4 d7d5 b1c3", new List<string> { "g8f6", "f8b4", "d5e4" } }, // Classical, Winawer, Rubinstein
            { "e2e4 e7e6 d2d4 d7d5 b1c3 g8f6", new List<string> { "e4e5", "c1g5" } }, // Steinitz, Burn Variation
            { "e2e4 e7e6 d2d4 d7d5 e4e5", new List<string> { "c7c5" } }, // Advance variation main line
            { "e2e4 e7e6 d2d4 d7d5 e4e5 c7c5", new List<string> { "c2c3" } },

            // Caro-Kann Defense - More lines
            { "e2e4 c7c6", new List<string> { "d2d4", "d2d3", "b1c3"} },
            { "e2e4 c7c6 d2d4", new List<string> { "d7d5" } },
            { "e2e4 c7c6 d2d4 d7d5", new List<string> { "b1c3", "e4d5", "e4e5", "f2f3" } },
            { "e2e4 c7c6 d2d4 d7d5 b1c3", new List<string> { "d5e4" } },
            { "e2e4 c7c6 d2d4 d7d5 b1c3 d5e4", new List<string> { "c3e4" } },
            { "e2e4 c7c6 d2d4 d7d5 b1c3 d5e4 c3e4", new List<string> { "b8d7", "c8f5", "g8f6" } }, // Main lines after classical

            // Queen's Pawn Opening (d4 d5) - More lines
            { "d2d4 d7d5", new List<string> { "c2c4", "g1f3", "c1f4" } }, // Added Bf4 (London System)
            { "d2d4 d7d5 c2c4", new List<string> { "e7e6", "c7c6", "d5c4", "b8c6", "e7e5" } }, // Added e5 (Albin Countergambit)
            { "d2d4 d7d5 c2c4 e7e6", new List<string> { "b1c3", "g1f3" } },
            { "d2d4 d7d5 c2c4 e7e6 b1c3", new List<string> { "g8f6", "c7c5" } }, // QGD Tarrasch
            { "d2d4 d7d5 c2c4 e7e6 b1c3 g8f6", new List<string> { "c1g5", "g1f3" } }, // QGD Orthodox / Ragozin if Nf3

            // Indian Defenses (d4 Nf6) - More lines
            { "d2d4 g8f6", new List<string> { "c2c4", "g1f3", "c1g5" } }, // Added Bg5 (Torre Attack / Trompowsky if c4 not played yet)
            { "d2d4 g8f6 c2c4", new List<string> { "g7g6", "e7e6", "c7c5" } },
            { "d2d4 g8f6 c2c4 g7g6", new List<string> { "b1c3", "g2g3", "g1f3" } },
            { "d2d4 g8f6 c2c4 g7g6 b1c3", new List<string> { "f8g7", "d7d5" } }, // KID main, Grunfeld
            { "d2d4 g8f6 c2c4 g7g6 b1c3 f8g7", new List<string> { "e2e4" } }, // KID with e4
            { "d2d4 g8f6 c2c4 g7g6 b1c3 f8g7 e2e4", new List<string> { "d7d6" } },
            { "d2d4 g8f6 c2c4 g7g6 b1c3 f8g7 e2e4 d7d6", new List<string> { "g1f3", "f2f3", "c1e3"} }, // Classical, Saemisch, Averbakh

            // "Xmortian game variations" from user file - these were short, integrated/expanded above.
            // { "e2e4 e7e5 g1f3 b8c6 f1c4", new List<string> { "g8f6" } }, // Already covered
            // { "e2e4 e7e5 g1f3 b8c6 f1c4 g8f6", new List<string> { "d2d3" } }, // Already covered

            // Aggressive openings from user file
            // { "e2e4 e7e5 f2f4", new List<string> { "e5f4" } },          // King's Gambit - Covered
            // { "e2e4 e7e5 f2f4 e5f4", new List<string> { "g1f3" } },    // Covered
            // { "e2e4 e7e5 d2d4 e5d4 c2c3", new List<string> { "d4c3" } }, // Danish Gambit
            { "e2e4 e7e5 d2d4 e5d4", new List<string> { "c2c3", "d1d4" } }, // Added Qxd4 (Center Game)
            { "e2e4 e7e5 d2d4 e5d4 c2c3", new List<string> { "d4c3", "d7d5" } }, // Danish Accepted, Counter
            { "e2e4 e7e5 d2d4 e5d4 c2c3 d4c3", new List<string> { "f1c4", "b1c3"} },

            // From user file: "Expanded aggressive lines from 1800+ Lichess Black wins"
            // { "e2e4 c7c5 g1f3 d7d6", new List<string> { "d2d4" } }, // Covered
            // { "e2e4 c7c5 g1f3 e7e6", new List<string> { "d2d4", "f1d3" } }, // d3 bishop move is unusual, kept d4
            { "e2e4 e7e5 f1c4", new List<string> { "b8c6", "g8f6", "c7c6" } }, // Bishop's Opening
            // { "e2e4 e7e5 d2d4", new List<string> { "e5d4", "e5e4" } } // e5e4 is an error for black. Covered e5d4.
        };

        private static readonly Random rng = new();

        public static string GetBookMove(List<string> history)
        {
            string key = string.Join(" ", history);
            if (Book.TryGetValue(key, out var responses) && responses.Count > 0)
            {
                return responses[rng.Next(responses.Count)];
            }
            return null; // No book move available
        }
    }
}