using System;

namespace CKR.Features.Dogs.Api
{
    /// <summary>
    /// Wire format of https://dogapi.dog/api/v2 (JSON:API style envelope).
    /// </summary>
    [Serializable]
    public sealed class BreedsListDto
    {
        public BreedDto[] data;
    }

    [Serializable]
    public sealed class BreedSingleDto
    {
        public BreedDto data;
    }

    [Serializable]
    public sealed class BreedDto
    {
        public string id;
        public string type;
        public BreedAttributesDto attributes;
    }

    [Serializable]
    public sealed class BreedAttributesDto
    {
        public string name;
        public string description;
        public bool hypoallergenic;
        public BreedLifeDto life;
    }

    [Serializable]
    public sealed class BreedLifeDto
    {
        public int min;
        public int max;
    }
}
