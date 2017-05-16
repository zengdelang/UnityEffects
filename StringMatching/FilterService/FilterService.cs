using System.Collections.Generic;
using System.Text;

public class FilterService
{
    private ACTrie        m_ACTrie          = new ACTrie();
    private StringBuilder m_FilterTextCache = new StringBuilder();
    private List<ACEmit>  m_ParseResults    = new List<ACEmit>();

    public void TestContruct()
    {
        m_ACTrie.AddKeyword("she");
        m_ACTrie.AddKeyword("he");
        m_ACTrie.AddKeyword("her");
        m_ACTrie.AddKeyword("123");
        m_ACTrie.AddKeyword("hers");
        m_ACTrie.ConstructFailingStates();
    }

    public string FilterKeywords(string text)
    {
        m_ACTrie.ParseText(text, GetParseResultsList());
        if (m_ParseResults.Count == 0)
            return text;

        var start = m_ParseResults[0].Start;
        var end   = m_ParseResults[0].End;

        var filterTextCache = GetFilterTextCache(text);
        filterTextCache.Length = 0;
        AppendFilterChar(filterTextCache, text, start, 0);

        for (int i = 1, count = m_ParseResults.Count; i < count; ++i)
        {
            var interval = m_ParseResults[i];
            if (interval.Start <= end && interval.End >= end)
            {
                end = interval.End;
            }
            else
            {
                AppendFilterChar(filterTextCache, text, start, end - start + 1);
                start = interval.Start;
                end   = interval.End;
            }
        }
        
        AppendFilterChar(filterTextCache, text, start, end - start + 1);
        AppendFilterChar(filterTextCache, text, text.Length, 0);

        if (m_ParseResults.Count > 64)
            m_ParseResults = null;

        return filterTextCache.ToString();
    }

    private void AppendFilterChar(StringBuilder filterTextCache, string text, int start, int length)
    {
        if (filterTextCache.Length < start)
        {
            for (int i = filterTextCache.Length; i < start; ++i)
            {
                filterTextCache.Append(text[i]);
            }
        }

        for (int i = 0; i < length; ++i)
        {
            filterTextCache.Append('*');
        }
    }

    private StringBuilder GetFilterTextCache(string text)
    {
        if (text.Length <= 256)
        {
            return m_FilterTextCache ?? (m_FilterTextCache = new StringBuilder(text.Length));
        }
        return new StringBuilder(text.Length);
    }

    private List<ACEmit> GetParseResultsList()
    {
        return m_ParseResults ?? (m_ParseResults = new List<ACEmit>());
    } 
}
