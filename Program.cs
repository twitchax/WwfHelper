using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static System.Console;

namespace WwfHelper
{
    class Program
    {
        const string EnableFile = "enable.txt";

        static void Main(string[] args)
        {
            var trie = LoadWords();

            Write("Enter your letters: ");
            var letters = ReadLine().ToLower();

            while (true)
            {
                Write("Enter board letters: ");
                var boardLetters = ReadLine().ToLower();

                FindMatches(trie, letters, boardLetters).ToList().ForEach(WriteLine);
                WriteLine();
            }
        }

        // Loads the dictionary into a trie (radix tree).
        static WordNode LoadWords()
        {
            var trie = new WordNode("", false);

            foreach (var word in File.ReadAllLines(EnableFile))
            {
                var currentNode = trie;
                for (int k = 1; k <= word.Length; k++)
                {
                    var node = word.Substring(0, k);
                    currentNode = currentNode.GetOrAddChild(node, k == word.Length);
                }
            }

            return trie;
        }

        static IEnumerable<string> FindMatches(WordNode trie, string letters, string boardLetters)
        {
            var foundWords = new ConcurrentHashSet<string>();

            // First, expand dash.
            var possibleBoardConfigurations = new List<string>();
            if (boardLetters.Contains('-'))
            {
                var amountOfDotsToAdd = boardLetters.Count(c => c == '.');
                if (boardLetters.Count(c => c == '-') == 1)
                    for (int k = 0; k <= letters.Length - amountOfDotsToAdd; k++)
                        possibleBoardConfigurations.Add(boardLetters.Replace("-", Enumerable.Range(0, k).Aggregate("", (acc, n) => acc + ".")));
                else if (boardLetters.Count(c => c == '-') == 2)
                {
                    for (int k = 0; k <= letters.Length - amountOfDotsToAdd; k++)
                        for(int j = 0; j <= k; j++)
                            possibleBoardConfigurations.Add(boardLetters.ReplaceFirst("-", Enumerable.Range(0, j).Aggregate("", (acc, n) => acc + ".")).ReplaceFirst("-", Enumerable.Range(0, k - j).Aggregate("", (acc, n) => acc + ".")));
                }
                else
                    throw new Exception("Only two dashes are allowed.");
            }
            else
                possibleBoardConfigurations.Add(boardLetters);

            var letterPermutations = Helpers.GetPermutations(letters.ToCharArray());

            var allMatchPossibilities = new HashSet<string>();
            foreach (var perm in letterPermutations)
            {
                // First, replace all "dot"s in board configuration.
                var innerPossibilities = new HashSet<string>();
                foreach (var possibleBoardConfiguration in possibleBoardConfigurations)
                {
                    var permutation = perm;
                    var possibility = "";

                    foreach (var boardLetter in possibleBoardConfiguration)
                    {
                        if (boardLetter == '.')
                        {
                            possibility += permutation[0];
                            permutation = permutation.Substring(1);
                        }
                        else
                        {
                            possibility += boardLetter;
                        }
                    }

                    innerPossibilities.Add(possibility);
                }

                // Find all remaining "dots" (wildcard player letters) and replace with all letters.
                var innerPossibilities2 = new HashSet<string>();
                foreach (var possibility in innerPossibilities.ToList())
                {
                    if (possibility.Any(c => c == '.'))
                    {
                        if (possibility.Count(c => c == '.') > 1)
                            throw new Exception("Too many dots!");

                        for (var k = 'a'; k <= 'z'; k++)
                        {
                            innerPossibilities2.Add(possibility.Replace('.', k));
                        }
                    }
                    else
                        innerPossibilities2.Add(possibility);
                }

                allMatchPossibilities.AddAll(innerPossibilities2);
            }

            var matchPossibilities = allMatchPossibilities.ToList();

            Parallel.ForEach(matchPossibilities, matchPossibility =>
            {
                if (trie.HasWord(matchPossibility))
                    foundWords.Add(matchPossibility);
            });

            return foundWords.ToList()
                .Select(w => new
                {
                    Word = w, 
                    w.Length, 
                    Score = w.Aggregate(0, (acc, c) => acc + Helpers.LetterPoints[c])
                }).OrderBy(o => o.Length).ThenBy(o => o.Score).Select(o => $"[{o.Score:D3}, {o.Length:D2}] {o.Word}").ToList();
        }
    }
}
