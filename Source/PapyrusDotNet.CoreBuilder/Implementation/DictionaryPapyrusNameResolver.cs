/*
    This file is part of PapyrusDotNet.

    PapyrusDotNet is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PapyrusDotNet is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
	
	Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using PapyrusDotNet.Common.Interfaces;
using PapyrusDotNet.CoreBuilder.Interfaces;

namespace PapyrusDotNet.CoreBuilder.Implementation
{
    public class DictionaryPapyrusNameResolver : IPapyrusNameResolver
    {
        internal string[] WordList;

        private IStatusCallbackService statusCallbackService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusCallback"></param>
        /// <param name="wordDictionaryFile"></param>
        public DictionaryPapyrusNameResolver(IStatusCallbackService statusCallback, string wordDictionaryFile = null)
        {
            statusCallbackService = statusCallback;
            // "wordlist.txt"
            if (string.IsNullOrEmpty(wordDictionaryFile))
                return;
            if (File.Exists(wordDictionaryFile))
            {
                statusCallbackService.WriteLine("Loading wordlist... This may take a few seconds.");
                WordList = File.ReadAllLines(wordDictionaryFile);
            }
            else
            {
                statusCallbackService.WriteLine("Wordlist was not found, skipping...");
                WordList = new string[0];
            }
        }

        /// <summary>
        /// Resolves the inputName and returns a PascalCase class-friendly name
        /// </summary>
        /// <param name="inputName"></param>
        /// <returns></returns>
        public string Resolve(string inputName)
        {
            if (string.IsNullOrEmpty(inputName)) return null;

            var outputName = inputName;
            if (WordList != null && WordList.Length > 0)
            {
                if (!char.IsUpper(inputName[0]))
                {
                    var usedWords = new List<string>();
                    var ordered = WordList.OrderByDescending(o => o.Length);
                    foreach (var word in ordered)
                    {
                        if (string.IsNullOrEmpty(word) || word.Length < 4) continue;
                        if (inputName.ToLower().Contains(word) && !usedWords.Any(s => s.Contains(word)))
                        {
                            var i = inputName.ToLower().IndexOf(word);

                            bool skip = false;
                            if (i > 0)
                            {
                                skip = char.IsUpper(inputName[i - 1]);
                            }

                            if (skip) continue;

                            var w = char.ToUpper(word[0]) + word.Substring(1);
                            outputName = outputName.Replace(word, w);
                            usedWords.Add(word);
                        }
                    }
                }
            }
            if (!char.IsUpper(outputName[0]))
            {
                outputName = char.ToUpper(outputName[0]) + outputName.Substring(1);
            }
            return outputName;
        }
    }
}