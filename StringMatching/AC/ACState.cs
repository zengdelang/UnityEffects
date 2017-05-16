using System.Collections.Generic;

public class ACState
{
    /// <summary>
    /// 是否是根节点
    /// </summary>
    private bool            m_IsRootState;
    /// <summary>
    /// 匹配失败时跳转的状态
    /// </summary>
    private ACState         m_FailingState;
    /// <summary>
    /// 包含命中的字符串集
    /// </summary>
    private HashSet<string> m_Emits;
    /// <summary>
    /// 匹配成功的状态迁移
    /// </summary>
    private Dictionary<char, ACState> m_SuccessState;

    public ACState(bool isRootState = false)
    {
        m_IsRootState = isRootState;
    }

    public void AddEmit(string keyword)
    {
        if (m_Emits == null)
        {
            m_Emits = new HashSet<string>();
        }
        m_Emits.Add(keyword);
    }

    public void AddEmits(IEnumerator<string> emits)
    {
        if (emits != null)
        {
            while (emits.MoveNext())
            {
                AddEmit(emits.Current);
            }
        }
    }

    public IEnumerator<string> GetEmits()
    {
        if(m_Emits != null)
            return m_Emits.GetEnumerator();
        return null;
    }

    public ACState GetFailingState()
    {
        return m_FailingState;
    }

    public void SetFailingState(ACState failingState)
    {
        m_FailingState = failingState;
    }

    protected ACState NextState(char c, bool ignoreRootState)
    {
        ACState nextState = null;
        if ((m_SuccessState == null || !m_SuccessState.TryGetValue(c, out nextState)) && m_IsRootState && !ignoreRootState)
        {
            nextState = this;
        }
        return nextState;
    }
  
    public ACState NextState(char c)
    {
        return NextState(c, false);
    }

    public ACState NextStateIgnoreRootState(char c)
    {
        return NextState(c, true);
    }
 
    public ACState AddState(char c)
    {
        ACState nextState = NextStateIgnoreRootState(c);
        if (nextState == null)
        {
            nextState = new ACState();
            if(m_SuccessState == null)
                m_SuccessState = new Dictionary<char, ACState>();
            m_SuccessState.Add(c, nextState);
        }
        return nextState;
    }
 
    public IEnumerable<ACState> GetStates()
    {
        if (m_SuccessState != null)
            return m_SuccessState.Values;
        return null;
    }

    public IEnumerable<char> GetTransitions()
    {
        if (m_SuccessState != null)
            return m_SuccessState.Keys;
        return null;
    }
}
