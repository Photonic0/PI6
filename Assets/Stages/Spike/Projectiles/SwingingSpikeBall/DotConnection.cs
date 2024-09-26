public class DotConnection
{
    public readonly Dot dotA;
    public readonly Dot dotB;
    public readonly float length;
    public DotConnection(Dot a, Dot b, float length)
    {
        dotA = a;
        dotB = b;
        this.length = length;
    }
    public DotConnection(Dot a, Dot b)
    {
        dotA = a;
        dotB = b;
        length = (a.position - b.position).magnitude;
    }
    public Dot Other(Dot dot) => dot == dotA ? dotB : dotA;
}
