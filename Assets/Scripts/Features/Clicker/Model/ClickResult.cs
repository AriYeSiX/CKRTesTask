namespace CKR.Features.Clicker.Model
{
    public enum ClickSource
    {
        Manual,
        Auto
    }

    /// <summary>
    /// Outcome of a single click attempt. A rejected click (no energy) is still reported,
    /// so the view can give the player a reason why nothing happened.
    /// </summary>
    public readonly struct ClickResult
    {
        public readonly ClickSource Source;
        public readonly bool IsRewarded;
        public readonly long Reward;
        public readonly int EnergySpent;

        private ClickResult(ClickSource source, bool isRewarded, long reward, int energySpent)
        {
            Source = source;
            IsRewarded = isRewarded;
            Reward = reward;
            EnergySpent = energySpent;
        }

        public static ClickResult Rewarded(ClickSource source, long reward, int energySpent)
        {
            return new ClickResult(source, true, reward, energySpent);
        }

        public static ClickResult Rejected(ClickSource source)
        {
            return new ClickResult(source, false, 0, 0);
        }
    }
}
