using UnityEngine;

namespace CKR.Configs
{
    /// <summary>
    /// Every balance value (rewards, costs, energy, intervals) lives here, so the economy can be
    /// re-tuned without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "CKR/Configs/Economy", order = 1)]
    public sealed class EconomyConfig : ScriptableObject
    {
        [Header("Currency")]
        [SerializeField, Min(0)] private long _startBalance;
        [SerializeField, Min(0)] private long _rewardPerClick = 1;

        [Header("Energy")]
        [SerializeField, Min(1)] private int _maxEnergy = 1000;
        [SerializeField] private bool _startWithFullEnergy = true;
        [SerializeField, Min(0)] private int _startEnergy = 1000;
        [SerializeField, Min(0)] private int _energyPerManualClick = 1;
        [SerializeField, Min(0)] private int _energyPerAutoClick = 1;

        [Header("Energy regeneration")]
        [SerializeField, Min(0)] private int _energyRegenAmount = 10;
        [SerializeField, Min(0.1f)] private float _energyRegenIntervalSeconds = 10f;

        [Header("Auto collect")]
        [SerializeField] private bool _autoCollectEnabled = true;
        [SerializeField, Min(0.1f)] private float _autoCollectIntervalSeconds = 3f;

        public long StartBalance => _startBalance;
        public long RewardPerClick => _rewardPerClick;

        public int MaxEnergy => _maxEnergy;
        public int StartEnergy => _startWithFullEnergy ? _maxEnergy : Mathf.Min(_startEnergy, _maxEnergy);
        public int EnergyPerManualClick => _energyPerManualClick;
        public int EnergyPerAutoClick => _energyPerAutoClick;

        public int EnergyRegenAmount => _energyRegenAmount;
        public float EnergyRegenIntervalSeconds => _energyRegenIntervalSeconds;

        public bool AutoCollectEnabled => _autoCollectEnabled;
        public float AutoCollectIntervalSeconds => _autoCollectIntervalSeconds;

        private void OnValidate()
        {
            _startEnergy = Mathf.Clamp(_startEnergy, 0, _maxEnergy);
        }
    }
}
