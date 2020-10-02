using System.Collections.Generic;
using UnityEngine;

namespace instance.id.AAI.Extensions
{
    // -- https://github.com/Magicolo/PseudoFramework  --
    // --------------------------------------------------
    public static class StringExtensions
    {
        public static char First(this string s)
        {
            return string.IsNullOrEmpty(s) ? default(char) : s[0];
        }

        public static char Last(this string s)
        {
            return string.IsNullOrEmpty(s) ? default(char) : s[s.Length - 1];
        }

        public static char Pop(this string s, int index, out string remaining)
        {
            var c = s[0];
            remaining = s.Remove(index, 1);

            return c;
        }

        public static char Pop(this string s, out string remaining)
        {
            return s.Pop(0, out remaining);
        }

        public static char PopRandom(this string s, out string remaining)
        {
            return s.Pop(Random.Range(0, s.Length), out remaining);
        }

        public static string PopRange(this string s, int startIndex, char stopCharacter, out string remaining)
        {
            var popped = "";
            var maximumIterations = s.Length;

            for (var i = 0; i < maximumIterations - startIndex; i++)
            {
                var c = s.Pop(startIndex, out s);

                if (c == stopCharacter)
                    break;

                popped += c;
            }

            remaining = s;

            return popped;
        }

        public static string PopRange(this string s, char stopCharacter, out string remaining)
        {
            return s.PopRange(0, stopCharacter, out remaining);
        }

        public static string PopRange(this string s, int startIndex, int length, out string remaining)
        {
            var popped = "";

            for (var i = 0; i < length; i++)
                popped += s.Pop(startIndex, out s);

            remaining = s;

            return popped;
        }

        public static string PopRange(this string s, int length, out string remaining)
        {
            return s.PopRange(0, length, out remaining);
        }

        public static string GetRange(this string s, int startIndex, char stopCharacter)
        {
            var substring = "";

            for (var i = startIndex; i < s.Length; i++)
            {
                var c = s[i];

                if (c == stopCharacter)
                    break;

                substring += c;
            }

            return substring;
        }

        public static string GetRange(this string s, char stopCharacter)
        {
            return s.GetRange(0, stopCharacter);
        }

        public static string GetRange(this string s, int startIndex, int length)
        {
            var substring = "";

            for (var i = startIndex; i < startIndex + length; i++)
                substring += s[i];

            return substring;
        }

        public static string GetRange(this string s, int startIndex)
        {
            return s.GetRange(startIndex, s.Length - startIndex);
        }

        public static string Reverse(this string s)
        {
            var reversed = "";

            for (var i = s.Length; i-- > 0;)
                reversed += s[i];

            return reversed;
        }

        public static string Capitalized(this string s)
        {
            var capitalized = "";

            if (!string.IsNullOrEmpty(s))
            {
                if (s.Length == 1)
                    capitalized = char.ToUpper(s[0]).ToString();
                else
                    capitalized = char.ToUpper(s[0]) + s.Substring(1);
            }

            return capitalized;
        }

        public static string[] SplitWords(this string s, int minWordLength)
        {
            var words = new List<string>();
            var lastCapitalIndex = 0;
            var counter = 0;

            for (var i = 0; i < s.Length; i++)
            {
                if (counter >= minWordLength && i <= s.Length - minWordLength && (char.IsUpper(s[i]) || char.IsNumber(s[i])))
                {
                    words.Add(s.Substring(lastCapitalIndex, counter));
                    lastCapitalIndex = i;
                    counter = 0;
                }

                counter += 1;
            }

            words.Add(s.Substring(lastCapitalIndex));

            return words.ToArray();
        }

        public static string[] SplitWords(this string s)
        {
            return SplitWords(s, 1);
        }

        public static T Capitalized<T>(this T stringArray) where T : IList<string>
        {
            for (var i = 0; i < stringArray.Count; i++)
                stringArray[i] = stringArray[i].Capitalized();

            return stringArray;
        }

        public static string Concat(this IList<string> stringArray, string separator, int startIndex, int count)
        {
            var concat = "";

            for (var i = startIndex; i < Mathf.Min(startIndex + count, stringArray.Count); i++)
            {
                concat += stringArray[i];

                if (i < stringArray.Count - 1)
                    concat += separator;
            }

            return concat;
        }

        public static string Concat(this IList<string> stringArray, string separator, int startIndex)
        {
            return stringArray.Concat(separator, startIndex, stringArray.Count - startIndex);
        }

        public static string Concat(this IList<string> stringArray, string separator)
        {
            return stringArray.Concat(separator, 0, stringArray.Count);
        }

        public static string Concat(this IList<string> stringArray, char separator)
        {
            return stringArray.Concat(separator.ToString(), 0, stringArray.Count);
        }

        public static string Concat(this IList<string> stringArray)
        {
            return stringArray.Concat(string.Empty);
        }

        public static float GetWidth(this string s, Font font)
        {
            float widthSum = 0;

            for (var i = 0; i < s.Length; i++)
            {
                var letter = s[i];
                CharacterInfo charInfo;
                font.GetCharacterInfo(letter, out charInfo);
                widthSum += charInfo.advance;
            }

            return widthSum;
        }

        public static Rect GetRect(this string s, Font font, int size = 10, FontStyle fontStyle = FontStyle.Normal)
        {
            float width = 0;
            float height = 0;
            float lineWidth = 0;
            float lineHeight = 0;

            foreach (var letter in s)
            {
                CharacterInfo charInfo;
                font.GetCharacterInfo(letter, out charInfo, size, fontStyle);

                if (letter == '\n')
                {
                    if (lineHeight == 0) lineHeight = size;
                    width = Mathf.Max(width, lineWidth);
                    height += lineHeight;
                    lineWidth = 0;
                    lineHeight = 0;
                }
                else
                {
                    lineWidth += charInfo.advance;
                    lineHeight = Mathf.Max(lineHeight, charInfo.size);
                }
            }

            width = Mathf.Max(width, lineWidth);
            height += lineHeight;

            return new Rect(0, 0, width, height);
        }

        public static GUIContent ToGUIContent(this string s, char labelTooltipSeparator)
        {
            var split = s.Split(labelTooltipSeparator);

            return new GUIContent(split[0], split[1]);
        }

        public static GUIContent ToGUIContent(this string s, string tooltip)
        {
            return new GUIContent(s, tooltip);
        }

        public static GUIContent ToGUIContent(this string s)
        {
            return new GUIContent(s);
        }

        public static GUIContent[] ToGUIContents(this IList<string> labels, char labelTooltipSeparator = '\0')
        {
            var guiContents = new GUIContent[labels.Count];

            for (var i = 0; i < labels.Count; i++)
            {
                if (labelTooltipSeparator != '\0')
                {
                    var split = labels[i].Split(labelTooltipSeparator);
                    if (split.Length == 1) guiContents[i] = new GUIContent(split[0]);
                    else if (split.Length == 2) guiContents[i] = new GUIContent(split[0], split[1]);
                    else guiContents[i] = new GUIContent(labels[i]);
                }
                else
                    guiContents[i] = new GUIContent(labels[i]);
            }

            return guiContents;
        }

        public static GUIContent[] ToGUIContents(this IList<string> labels, IList<string> tooltips)
        {
            var guiContents = new GUIContent[labels.Count];

            for (var i = 0; i < labels.Count; i++)
                guiContents[i] = new GUIContent(labels[i], tooltips[i]);

            return guiContents;
        }
    }
}
