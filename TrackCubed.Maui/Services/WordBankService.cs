using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Maui.Services
{
    /// <summary>
    /// A simple service to generate random confirmation phrases from an embedded word bank.
    /// </summary>
    public class WordBankService
    {
        // A large, curated list of simple, unambiguous, and easy-to-type words.
        private readonly List<string> _words = new List<string>
        {
            // --- Colors ---
            "red", "blue", "green", "yellow", "purple", "orange", "black", "white",
            "gray", "pink", "brown", "gold", "silver", "ivory", "teal", "navy",

            // --- Nature & Elements ---
            "ocean", "river", "mountain", "sky", "forest", "desert", "stone", "earth",
            "fire", "water", "air", "cloud", "rain", "snow", "sun", "moon", "star",
            "leaf", "tree", "flower", "grass", "wind", "storm", "ice", "wave",

            // --- Animals ---
            "lion", "tiger", "bear", "eagle", "shark", "whale", "fox", "wolf",
            "dog", "cat", "bird", "fish", "ant", "bee", "owl", "duck", "swan",
            "crab", "horse", "snake", "frog", "goat", "mouse", "deer", "raven",

            // --- Space ---
            "rocket", "comet", "planet", "galaxy", "nebula", "orbit", "nova", "luna",
            "solar", "void", "cosmos", "atlas", "titan", "pluto", "mars", "venus",

            // --- Shapes & Geometry ---
            "circle", "square", "triangle", "diamond", "arrow", "line", "point", "cube",
            "helix", "angle", "curve", "grid", "ring", "loop", "prism", "pyramid",

            // --- Simple Nouns (Objects) ---
            "house", "car", "boat", "train", "book", "key", "door", "window",
            "table", "chair", "phone", "clock", "anchor", "bell", "brick", "bridge",
            "brush", "button", "cable", "camera", "card", "chain", "chest", "coin",

            // --- Technology & Science ---
            "code", "data", "link", "network", "pixel", "atom", "cell", "gear",
            "robot", "signal", "wave", "bytes", "chip", "disk", "drone", "engine",
            "flask", "glass", "laser", "logic", "matrix", "modem", "motor", "node",

            // --- Concepts & Ideas ---
            "idea", "dream", "story", "hope", "joy", "truth", "time", "space",
            "life", "mind", "echo", "focus", "quest", "level", "nexus", "origin",
            "purity", "quest", "valor", "zenith", "epoch", "haven", "matrix", "proxy",

            // --- Directions & Seasons ---
            "north", "south", "east", "west", "alpha", "beta", "gamma", "delta",
            "omega", "sigma", "zeta", "spring", "summer", "autumn", "winter"
        };

        private readonly Random _random = new Random();

        /// <summary>
        /// Generates a random, hyphenated phrase of a specified word count.
        /// </summary>
        /// <param name="wordCount">The number of random words to include in the phrase.</param>
        /// <returns>A unique string like "blue-rocket-gamma".</returns>
        public string GetRandomPhrase(int wordCount = 5)
        {
            if (wordCount <= 0) wordCount = 5;

            var randomWords = new List<string>();
            for (int i = 0; i < wordCount; i++)
            {
                int index = _random.Next(_words.Count);
                randomWords.Add(_words[index]);
            }

            return string.Join("-", randomWords);
        }
    }
}
