//
//
// The MIT License (MIT)
//
// Copyright (c) 2026 Federico Mazzanti
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
//

namespace Mafesoft.Localization.Json.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Reads the keys of a JSON vocabulary file as Mafesoft.Localization.Json does: nested objects become dotted keys,
    /// arrays one key per item, comments and trailing commas are allowed, null values are skipped. A small reader of its
    /// own, because an analyzer cannot depend on System.Text.Json.
    /// </summary>
    internal sealed class VocabularyJsonReader
    {
        private readonly string text;
        private int position;

        private VocabularyJsonReader(string text) => this.text = text;

        /// <summary>
        /// The terms of the file in the order of the file, or null with the error when it is not a valid vocabulary.
        /// </summary>
        public static IReadOnlyList<KeyValuePair<string, string>>? Read(string text, out string? error)
        {
            var reader = new VocabularyJsonReader(text);
            var terms = new List<KeyValuePair<string, string>>();
            try
            {
                reader.SkipTrivia();
                if (reader.Peek() != '{')
                {
                    throw reader.Error("the root must be an object");
                }
                reader.ReadValue(null, terms);
                reader.SkipTrivia();
                if (reader.position < reader.text.Length)
                {
                    throw reader.Error("unexpected content after the root object");
                }
            }
            catch (FormatException ex)
            {
                error = ex.Message;
                return null;
            }

            error = null;
            return terms;
        }

        private char Peek() => position < text.Length ? text[position] : '\0';

        private FormatException Error(string message)
        {
            var line = 1;
            for (var i = 0; i < position && i < text.Length; i++)
            {
                if (text[i] == '\n') line++;
            }
            return new FormatException($"{message} (line {line})");
        }

        private void SkipTrivia()
        {
            while (position < text.Length)
            {
                var c = text[position];
                if (char.IsWhiteSpace(c) || c == '﻿')
                {
                    position++;
                }
                else if (c == '/' && position + 1 < text.Length && text[position + 1] == '/')
                {
                    while (position < text.Length && text[position] != '\n') position++;
                }
                else if (c == '/' && position + 1 < text.Length && text[position + 1] == '*')
                {
                    var end = text.IndexOf("*/", position + 2, StringComparison.Ordinal);
                    if (end < 0) throw Error("unterminated comment");
                    position = end + 2;
                }
                else
                {
                    return;
                }
            }
        }

        private void Expect(char c)
        {
            SkipTrivia();
            if (Peek() != c) throw Error($"'{c}' expected");
            position++;
        }

        private void ReadValue(string? key, List<KeyValuePair<string, string>> terms)
        {
            SkipTrivia();
            var c = Peek();
            switch (c)
            {
                case '{':
                    ReadObject(key, terms);
                    break;
                case '[':
                    ReadArray(key, terms);
                    break;
                case '"':
                    Add(key, ReadString(), terms);
                    break;
                default:
                    var literal = ReadLiteral();
                    if (literal != "null")
                    {
                        Add(key, literal, terms);
                    }
                    break;
            }
        }

        private static void Add(string? key, string value, List<KeyValuePair<string, string>> terms)
        {
            if (!string.IsNullOrEmpty(key))
            {
                terms.Add(new KeyValuePair<string, string>(key!, value));
            }
        }

        private static string Combine(string? parent, string name) => parent is null ? name : parent + "." + name;

        private void ReadObject(string? prefix, List<KeyValuePair<string, string>> terms)
        {
            Expect('{');
            SkipTrivia();
            while (Peek() != '}')
            {
                SkipTrivia();
                if (Peek() != '"') throw Error("property name expected");
                var name = ReadString();
                Expect(':');
                ReadValue(Combine(prefix, name), terms);
                SkipTrivia();
                if (Peek() == ',')
                {
                    position++;
                    SkipTrivia();
                }
                else if (Peek() != '}')
                {
                    throw Error("',' or '}' expected");
                }
            }
            position++;
        }

        private void ReadArray(string? prefix, List<KeyValuePair<string, string>> terms)
        {
            Expect('[');
            SkipTrivia();
            var index = 0;
            while (Peek() != ']')
            {
                ReadValue(Combine(prefix, index.ToString(CultureInfo.InvariantCulture)), terms);
                index++;
                SkipTrivia();
                if (Peek() == ',')
                {
                    position++;
                    SkipTrivia();
                }
                else if (Peek() != ']')
                {
                    throw Error("',' or ']' expected");
                }
            }
            position++;
        }

        private string ReadLiteral()
        {
            var start = position;
            while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] is '-' or '+' or '.'))
            {
                position++;
            }
            var literal = text.Substring(start, position - start);
            if (literal.Length == 0) throw Error("value expected");
            if (literal is "true" or "false" or "null" ||
                double.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                return literal;
            }
            throw Error($"invalid value '{literal}'");
        }

        private string ReadString()
        {
            position++; // opening quote
            var builder = new StringBuilder();
            while (true)
            {
                if (position >= text.Length) throw Error("unterminated string");
                var c = text[position++];
                if (c == '"') return builder.ToString();
                if (c != '\\')
                {
                    builder.Append(c);
                    continue;
                }

                if (position >= text.Length) throw Error("unterminated string");
                var escape = text[position++];
                switch (escape)
                {
                    case '"': builder.Append('"'); break;
                    case '\\': builder.Append('\\'); break;
                    case '/': builder.Append('/'); break;
                    case 'b': builder.Append('\b'); break;
                    case 'f': builder.Append('\f'); break;
                    case 'n': builder.Append('\n'); break;
                    case 'r': builder.Append('\r'); break;
                    case 't': builder.Append('\t'); break;
                    case 'u':
                        if (position + 4 > text.Length ||
                            !int.TryParse(text.Substring(position, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
                        {
                            throw Error("invalid \\u escape");
                        }
                        builder.Append((char)code);
                        position += 4;
                        break;
                    default:
                        throw Error($"invalid escape '\\{escape}'");
                }
            }
        }
    }
}
