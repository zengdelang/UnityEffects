public class ACEmit : Interval
{
    private string m_Keyword;

    public ACEmit(int start, int end, string keyword) : base(start, end)
    {     
        m_Keyword = keyword;
    }

    public string GetKeyword()
    {
        return m_Keyword;
    }

    public override string ToString()
    {
        return base.ToString() + "=" + m_Keyword;
    }
}
