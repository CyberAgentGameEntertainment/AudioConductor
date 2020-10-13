namespace Sge.Sound.Internal
{
    /// <summary>
    /// サウンドプロバイダーインターフェース
    /// </summary>
    internal interface ISoundControllerProvider
    {
        void Setup(ISoundCueSheet sheet);
        ISoundController RentController(ISoundCue cue);
        void ReturnController(ISoundController controller);
    }
}