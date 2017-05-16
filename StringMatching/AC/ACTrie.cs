using System.Collections.Generic;

/// <summary>
/// 字符串多模式匹配 --- AC自动机
/// </summary>
public class ACTrie
{
    private ACTrieConfig m_ACTrieConfig;
    private ACState      m_RootState;
    private bool         m_FialingStatesConstructed;

    public ACTrie(ACTrieConfig trieConfig)
    {
        m_RootState = new ACState(true);
        m_ACTrieConfig = trieConfig;
    }

    public ACTrie() : this(new ACTrieConfig())
    {

    }

    public void SetCaseInsensitive(bool caseInsensitive)
    {
        m_ACTrieConfig.CaseInsensitive = caseInsensitive;
    }
 
    public void AddKeyword(string keyword, bool caseInsensitive = true)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return;
        }

        ACState currentState = m_RootState;
        foreach (var c in keyword)
        {
            currentState = currentState.AddState(caseInsensitive ? char.ToLower(c) : c);
        }
        currentState.AddEmit(keyword);
    }

    public List<ACEmit> ParseText(string text)
    {
        ConstructFailingStates();

        int pos = 0;
        ACState currentState        = m_RootState;
        List<ACEmit> collectedEmits = new List<ACEmit>();
        foreach (var c in text)
        {
            currentState = GetState(currentState, m_ACTrieConfig.CaseInsensitive ? char.ToLower(c) : c);
            StoreEmits(pos, currentState, collectedEmits);
            ++pos;
        }

        collectedEmits.Sort(Interval.Compare);
        return collectedEmits;
    }

    public void ParseText(string text, List<ACEmit> collectedEmits)
    {
        ConstructFailingStates();

        int pos = 0;
        ACState currentState = m_RootState;
        collectedEmits.Clear();
        foreach (var c in text)
        {
            currentState = GetState(currentState, m_ACTrieConfig.CaseInsensitive ? char.ToLower(c) : c);
            StoreEmits(pos, currentState, collectedEmits);
            ++pos;
        }

        collectedEmits.Sort(Interval.Compare);
    }

    public bool ContainKeyword(string text)
    {
        ConstructFailingStates();

        ACState currentState = m_RootState;
        foreach (var c in text)
        {
            currentState = GetState(currentState, m_ACTrieConfig.CaseInsensitive ? char.ToLower(c) : c);
            if (HasEmits(currentState))
            {
                return true;
            }
        }
        return false;
    }

    private static ACState GetState(ACState currentState, char c)
    {
        ACState newCurrentState = currentState.NextState(c);
        while (newCurrentState == null)
        {
            currentState = currentState.GetFailingState();
            newCurrentState = currentState.NextState(c);
        }
        return newCurrentState;
    }

    public void ConstructFailingStates()
    {
        if (m_FialingStatesConstructed)
            return;

        m_FialingStatesConstructed = true;
        Queue<ACState> queue = new Queue<ACState>();

        var childStates = m_RootState.GetStates();
        if (childStates != null)
        {
            foreach (var depthOneState in childStates)
            {
                depthOneState.SetFailingState(m_RootState);
                queue.Enqueue(depthOneState);
            }
        }

        while (queue.Count > 0)
        {
            ACState currentState = queue.Dequeue();
            var transitions = currentState.GetTransitions();
            if (transitions != null)
            {
                foreach (var transition in transitions)
                {
                    ACState targetState = currentState.NextState(transition);
                    queue.Enqueue(targetState);

                    ACState traceFailureState = currentState.GetFailingState();
                    while (traceFailureState.NextState(transition) == null)
                    {
                        traceFailureState = traceFailureState.GetFailingState();
                    }
                    ACState newFailureState = traceFailureState.NextState(transition);
                    targetState.SetFailingState(newFailureState);

                    targetState.AddEmits(newFailureState.GetEmits());
                }
            }
        }
    }

    private void StoreEmits(int pos, ACState currentState, List<ACEmit> collectedEmits)
    {
        var emits = currentState.GetEmits();
        if (emits != null)
        {
            while (emits.MoveNext())
            {
                collectedEmits.Add(new ACEmit(pos - emits.Current.Length + 1, pos, emits.Current));
            }
        }
    }

    private bool HasEmits(ACState currentState)
    {
        var emits = currentState.GetEmits();
        if (emits != null && emits.MoveNext())
        {
            return true;
        }
        return false;
    }
}
