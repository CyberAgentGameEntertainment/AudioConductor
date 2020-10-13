namespace Sge.Sound
{
    public class SequentialTrackSelector : ITrackSelector
    {
        private int _trackNum = -1;
        private int _currentIndex = -1;
        
        public void Setup(ISoundTrack[] tracks)
        {
            Reset();
            _trackNum = tracks.Length;
        }

        public int NextTrackIndex()
        {
            _currentIndex++;
            if (_currentIndex >= _trackNum)
            {
                _currentIndex = 0;
            }

            return _currentIndex;
        }

        public void Reset()
        {
            _trackNum = -1;
            _currentIndex = -1;
        }
    }
}