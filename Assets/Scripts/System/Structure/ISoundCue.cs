namespace Sge.Sound
{
    public enum SoundCuePlayType
    {
        Sequential,
        Random,
    }
    
    public interface ISoundCue
    {
        string Name { get; }
        string Category { get; }
        ISoundTrack[] Tracks { get; }
        SoundCuePlayType PlayType { get; }
    }
}