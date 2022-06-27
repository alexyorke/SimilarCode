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

    public static List<int> ConvertSnippetToVector(string code)
    {
        var arr = new int[programmingChars.Count];
        foreach (var c in code)
        {
            var i = programmingChars.IndexOf(c);
            if (i != -1)
            {
                arr[i]++;
            }
        }

        return new List<int>(arr);
    }
}