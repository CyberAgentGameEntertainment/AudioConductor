namespace Sge.Sound
{
    public interface ISoundCueSheet
    {
        string Name { get; }
        ISoundCue[] Cues { get; }
    }
}