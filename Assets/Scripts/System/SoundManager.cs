using System.Collections.Generic;
using Sge.Sound.Internal;

namespace Sge.Sound
{
    public sealed partial class SoundManager
    {
        private static SoundManager _instance;
        public static SoundManager Instance => _instance ?? (_instance = new SoundManager());

        private readonly ISoundControllerProvider _provider = new AudioClipSoundProvider();
        private readonly List<ISoundController> _activeControllerList = new List<ISoundController>();

        public void Setup(ISoundCueSheet sheet)
        {
            _provider.Setup(sheet);
        }

        public ISoundController RentController(ISoundCue cue)
        {
            var controller = _provider.RentController(cue);
            _activeControllerList.Add(controller);
            return controller;
        }

        public void ReturnController(ISoundController controller)
        {
            _activeControllerList.Remove(controller);
        }
    }
}