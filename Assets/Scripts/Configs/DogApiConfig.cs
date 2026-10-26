using UnityEngine;

namespace CKR.Configs
{
    [CreateAssetMenu(fileName = "DogApiConfig", menuName = "CKR/Configs/Dog Api", order = 4)]
    public sealed class DogApiConfig : ScriptableObject
    {
        [SerializeField] private string _baseUrl = "https://dogapi.dog/api/v2";

        [Tooltip("How many breeds to show; the API returns pages of 10 by default.")]
        [SerializeField, Min(1)] private int _breedsLimit = 10;

        [Header("Presentation")]
        [Tooltip("{0} - ordinal number, {1} - breed name.")]
        [SerializeField] private string _rowFormat = "{0} - {1}";
        [SerializeField] private string _emptyDescription = "Описание для этой породы отсутствует.";
        [SerializeField] private string _errorMessage = "Не удалось загрузить данные";

        [Tooltip("{0} - minimum years, {1} - maximum years.")]
        [SerializeField] private string _lifeSpanFormat = "Продолжительность жизни: {0}-{1} лет";
        [SerializeField] private string _hypoallergenicLabel = "Гипоаллергенная порода";
        [SerializeField] private string _confirmText = "OK";

        public string BreedsUrl => $"{_baseUrl.TrimEnd('/')}/breeds";
        public string BreedUrl(string breedId) => $"{_baseUrl.TrimEnd('/')}/breeds/{breedId}";

        public int BreedsLimit => _breedsLimit;
        public string RowFormat => _rowFormat;
        public string EmptyDescription => _emptyDescription;
        public string ErrorMessage => _errorMessage;
        public string LifeSpanFormat => _lifeSpanFormat;
        public string HypoallergenicLabel => _hypoallergenicLabel;
        public string ConfirmText => _confirmText;
    }
}
