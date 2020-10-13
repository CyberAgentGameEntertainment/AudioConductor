namespace Sge.Sound
{
    public interface ITrackSelector
    {
        void Setup(ISoundTrack[] tracks);
        int NextTrackIndex();
        void Reset();
    }
}