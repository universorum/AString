namespace Astra.Text.Models;

internal record struct Segment
{
    public SegmentType Type          { get; init; }
    public Range       Range         { get; init; }
    public int         ArgumentIndex { get; init; }
    public int         Alignment     { get; init; }
}