using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Converters.Papyrus2Clr.Implementations
{
    public class PascalCaseNameResolver : INameConvetionResolver
    {
        internal string[] WordList;

        private readonly string[] orderedWordList;

        private readonly Dictionary<string, string> resolvedNames = new Dictionary<string, string>();
        /// <summary>
        /// </summary>
        /// <param name="uiRenderer"></param>
        /// <param name="wordDictionaryFile"></param>
        public PascalCaseNameResolver(IUiRenderer uiRenderer, string wordDictionaryFile = null)
        {
            var ui = uiRenderer;
            // "wordlist.txt"
            if (string.IsNullOrEmpty(wordDictionaryFile))
                return;
            if (File.Exists(wordDictionaryFile))
            {
                ui.DrawInterface("Loading wordlist... This may take a few seconds.");
                WordList = File.ReadAllLines(wordDictionaryFile);
                Array.Sort(WordList);
            }
            else
            {
                ui.DrawInterface("Wordlist was not found, skipping...");
                WordList = new string[0];
            }

            if (orderedWordList == null)
            {
                orderedWordList = WordList.Select(w => w.ToLower()).ToArray();
                Array.Sort(orderedWordList);
            }
        }

        /// <summary>
        ///     Resolves the inputName and returns a PascalCase class-friendly name
        /// </summary>
        /// <param name="inputName"></param>
        /// <returns></returns>
        public string Resolve(string inputName)
        {
            if (string.IsNullOrEmpty(inputName)) return null;

            var loweredName = inputName.ToLower();
            if (resolvedNames.ContainsKey(loweredName))
                return resolvedNames[loweredName];

            var outputName = inputName;
            if (WordList != null && WordList.Length > 0)
            {
                if (!char.IsUpper(inputName[0]) && !char.IsUpper(inputName[1]))
                {
                    var usedWords = new HashSet<string>();

                    var nameFound = false;
                    for (var j = 0; j < inputName.Length; j++)
                    {
                        for (var i = inputName.Length - 1; i > 0; i--)
                        {
                            var name = string.Join("", inputName.Skip(j).Take((i + 1) - j)).ToLower();
                            var index = Array.BinarySearch(orderedWordList, name);
                            if (index >= 0 && !usedWords.Any(s => s.Contains(name)))
                            {
                                usedWords.Add(name);

                                var insertionPoint = outputName.ToLower().IndexOf(name);
                                
                                outputName = outputName.Remove(insertionPoint, name.Length);
                                outputName = outputName.Insert(insertionPoint, WordList[index]);

                                if (name.Length == inputName.Length)
                                {
                                    nameFound = true;
                                    break;
                                }
                            }
                        }
                        if (nameFound) break;
                    }                 
                }
            }
            outputName = ToTitleCase(outputName);

            resolvedNames.Add(loweredName, outputName);

            return outputName;
        }

        private static string ToTitleCase(string text)
        {
            if (!char.IsUpper(text[0]))
            {
                text = char.ToUpper(text[0]) + text.Substring(1);
            }
            return text;
        }
    }
}