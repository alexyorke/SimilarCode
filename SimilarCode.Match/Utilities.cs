using System.Collections.Generic;

namespace SimilarCode.Match;

public static class Utilities
{
    private static List<char> programmingChars = new List<char>
    {
        '!',
        '"',
        '#',
        '$',
        '%',
        '&',
        '\'',
        '(',
        ')',
        '*',
        '+',
        ',',
        '-',
        '.',
        '/',
        '0',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        ':',
        ';',
        '<',
        '=',
        '>',
        '?',
        '@',
        '[',
        '\\',
        ']',
        '^',
        '_',
        '`',
        'a',
        'b',
        'c',
        'd',
        'e',
        'f',
        'g',
        'h',
        'i',
        'j',
        'k',
        'l',
        'm',
        'n',
        'o',
        'p',
        'q',
        'r',
        's',
        't',
        'u',
        'v',
        'w',
        'x',
        'y',
        'z',
        '{',
        '|',
        '}',
        '~'
    };

    public static List<ushort> ConvertSnippetToVector(string code)
    {
        var arr = new ushort[programmingChars.Count];
        foreach (var c in code)
        {
            if (programmingChars.Contains(c))
            {
                unchecked
                {
                    arr[programmingChars.IndexOf(c)]++;
                }
            }
        }

        return new List<ushort>(arr);
    }
}