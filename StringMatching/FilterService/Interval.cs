public class Interval
{ 
    public int Start { get; set; }
    public int End { get; set; }

    public Interval(int start, int end)
    {
        Start = start;
        End = end;
    }

    public int Length()
    {
        return End - Start + 1;
    }

    public bool OverlapsWith(Interval other)
    {
        return Start <= other.End &&
               End >= other.Start;
    }

    public bool OverlapsWith(int point)
    {
        return Start <= point && point <= End;
    }

    public override string ToString()
    {
        return Start + ":" + End;
    }

    public static int Compare(Interval first, Interval second)
    {
        int comparison = first.Start - second.Start;
        return comparison != 0 ? comparison : first.End - second.End;
    }
}
