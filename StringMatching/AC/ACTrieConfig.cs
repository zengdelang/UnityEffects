public class ACTrieConfig
{
    /// <summary>
    /// 默认大小写不敏感
    /// 如关键字存在fuck
    /// 待解析串中如果有FuCk这样也能匹配到
    /// </summary>
    private bool m_CaseInsensitive = true;

    public bool CaseInsensitive
    {
        get { return m_CaseInsensitive;  }
        set { m_CaseInsensitive = value; }
    }
}
