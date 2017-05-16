using System;
using System.Collections.Generic;
using UnityEngine;

public static class StringSearching
{
    /// <summary>
    /// brute force string searching
    /// </summary>
    /// <returns></returns>
    public static int BF_Search(string text, string pattern, int startIndex = 0)
    {
        if (text == null)
            throw new ArgumentNullException("text");

        if(pattern == null)
            throw new ArgumentException("pattern");

        if (startIndex < 0 || startIndex > text.Length)
            throw new ArgumentOutOfRangeException("startIndex", "Argument is out of range.");

        int textLength    = text.Length;
        int patternLength = pattern.Length;
        if (textLength - startIndex < patternLength)
            return -1;

        int i = startIndex;
        int j = 0;
        while (i < textLength && j < patternLength)
        {
            if (text[i] == pattern[j])
            {
                ++i;
                ++j;
            }
            else
            {
                i = i - j + 1;
                j = 0;
            }
        }

        if (j >= patternLength)
            return i - j;
        
        return -1;
    }

    /// <summary>
    /// KMP string searching
    /// </summary>
    /// <param name="text"></param>
    /// <param name="pattern"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static int KMP_Search(string text, string pattern, int startIndex = 0)
    {
        if (text == null)
            throw new ArgumentNullException("text");

        if (pattern == null)
            throw new ArgumentException("pattern");

        if (startIndex < 0 || startIndex > text.Length)
            throw new ArgumentOutOfRangeException("startIndex", "Argument is out of range.");

        int textLength = text.Length;
        int patternLength = pattern.Length;
        if (textLength - startIndex < patternLength)
            return -1;

        var next = KMP_GetNext(pattern);

        int i = startIndex;
        int j = 0;
        while (i < textLength && j < patternLength)
        {
            if (text[i] == pattern[j])
            {
                ++i;
                ++j;
            }
            else
            {
                j = next[j];
                if (j == -1)
                {
                    ++i;
                    ++j;
                }
            }
        }

        if (j >= patternLength)
            return i - j;

        return -1;
    }

    private static int[] KMP_GetNext(string pattern)
    {
        var next = new int[pattern.Length];
        next[0] = -1;

        for (int i = 0, j = -1, length = pattern.Length - 1; i < length;)
        {
            if (j == -1 || pattern[i] == pattern[j])
            {
                ++i;
                ++j;

                if (pattern[i] == pattern[j])
                {
                    next[i] = next[j];
                }
                else
                {
                    next[i] = j;
                }               
            }
            else
            {
                j = next[j];
            }
        }

        return next;
    }

    /// <summary>
    /// BM string searching
    /// </summary>
    /// <param name="text"></param>
    /// <param name="pattern"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static int BM_Search(string text, string pattern, int startIndex = 0)
    {
        if (text == null)
            throw new ArgumentNullException("text");

        if (pattern == null)
            throw new ArgumentException("pattern");

        if (startIndex < 0 || startIndex > text.Length)
            throw new ArgumentOutOfRangeException("startIndex", "Argument is out of range.");

        int textLength = text.Length;
        int patternLength = pattern.Length;
        if (textLength - startIndex < patternLength)
            return -1;

        var badCharMoveMap = BM_GetBadCharMoveLength(pattern);
        var goodSuffixes   = BM_GetGoodSuffixes(pattern);

        int j = patternLength - 1;
        int i = startIndex + j;

        while (i < textLength && j >= 0)
        {
            if (text[i] == pattern[j])
            {
                --i;
                --j;
            }
            else
            {
                int badCharMove;
                if (!badCharMoveMap.TryGetValue(text[i], out badCharMove))
                {
                    badCharMove = patternLength;
                }

                i = i - j + Mathf.Max(goodSuffixes[j], badCharMove - (patternLength - 1 - j));
                j = patternLength - 1;
                i += j;
            }         
        }

        if (j == -1)
            return i + 1;

        return -1;
    }

    private static int[] BM_GetSuffix(string pattern)
    {
        int patternLength = pattern.Length;
        int[] suffixes = new int[patternLength];
     
        suffixes[patternLength - 1] = patternLength;
        int cur = patternLength - 1;  
        int pre = 0;                

        for (int i = patternLength - 2; i >= 0; --i)
        {
            int j = i;

            while (j >= 0 && pattern[j] == pattern[patternLength - 1 - (i - j)])
                --j;

            //需要加bmGLength[i + (tLen - 1 - pre)] < i - cur否则
            //11aaa12caa情况下就不对
            if (i > cur && suffixes[i + (patternLength - 1 - pre)] < i - cur)
            {
                suffixes[i] = suffixes[i + (patternLength - 1 - pre)];
                continue;
            }

            cur = Mathf.Min(cur, i);
            pre = i;

            while (cur >= 0 && pattern[cur] == pattern[patternLength - 1 - (pre - cur)])
                --cur;

            suffixes[i] = pre - cur;
        }

        return suffixes;
    }

    private static int[] BM_GetGoodSuffixes(string pattern)
    {
        var suffixes = BM_GetSuffix(pattern);
        int patternLength = pattern.Length;
        var goodSuffixes  = new int[patternLength];
     
        for (int i = 0; i < patternLength; ++i)
        {
            goodSuffixes[i] = patternLength;
        }

        int j = 0;
        for (int i = patternLength - 2; i >= 0; --i)
        {
            if (suffixes[i] == i + 1)
            {
                for (; j < patternLength - 1 - i; ++j)
                {
                    if (goodSuffixes[j] == patternLength)
                        goodSuffixes[j] = patternLength - 1 - i;
                }
            }
        }

        for (int i = 0; i < patternLength - 1; ++i)
        {
            goodSuffixes[patternLength - 1 - suffixes[i]] = patternLength - 1 - i;
        }

        return goodSuffixes;
    }

    private static Dictionary<char, int> BM_GetBadCharMoveLength(string pattern)
    {
        var moveLengthMap = new Dictionary<char, int>();
        var maxSubLength = pattern.Length - 1;
        for (int i = 0, length = maxSubLength + 1; i < length; ++i)
        {
            moveLengthMap[pattern[i]] = maxSubLength - i;
        }
        return moveLengthMap;
    }

    /// <summary>
    /// Sunday string searching
    /// </summary>
    /// <param name="text"></param>
    /// <param name="pattern"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static int Sunday_Search(string text, string pattern, int startIndex = 0)
    {
        if (text == null)
            throw new ArgumentNullException("text");

        if (pattern == null)
            throw new ArgumentException("pattern");

        if (startIndex < 0 || startIndex > text.Length)
            throw new ArgumentOutOfRangeException("startIndex", "Argument is out of range.");

        int textLength = text.Length;
        int patternLength = pattern.Length;
        if (textLength - startIndex < patternLength)
            return -1;

        var moveLengthMap = Sunday_GetMoveLength(pattern);

        int i = startIndex;
        int j = 0;
        while (i < textLength && j < patternLength)
        {
            if (text[i] == pattern[j])
            {
                ++i;
                ++j;
            }
            else
            {
                var nextCharIndex = i - j + patternLength;
                if (nextCharIndex >= textLength)
                    return -1;

                int moveDelta;
                if (moveLengthMap.TryGetValue(text[nextCharIndex], out moveDelta))
                {
                    i = i - j + moveDelta;
                }
                else
                {
                    i = nextCharIndex + 1;
                }

                j = 0;
            }
        }

        if (j >= patternLength)
            return i - j;

        return -1;
    }

    private static Dictionary<char, int> Sunday_GetMoveLength(string pattern)
    {
        var moveLengthMap = new Dictionary<char, int>();
        for (int i = 0, length = pattern.Length; i < length; ++i)
        {
            moveLengthMap[pattern[i]] = length - i;
        }
        return moveLengthMap;
    } 
}
