using System;
using CKR.Configs;
using CKR.Core.Net;
using CKR.Core.Requests;
using CKR.Features.Dogs.Model;

namespace CKR.Features.Dogs.Api
{
    public sealed class BreedDetailsRequest : JsonGetRequest<DogBreedDetails, BreedSingleDto>
    {
        private readonly DogApiConfig _config;
        private readonly string _breedId;

        public BreedDetailsRequest(DogApiConfig config, string breedId)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrEmpty(breedId))
                throw new ArgumentException("Breed id must not be empty.", nameof(breedId));

            _breedId = breedId;
        }

        public override string Description => "dogs:breed:" + _breedId;

        protected override string BuildUrl()
        {
            return _config.BreedUrl(_breedId);
        }

        protected override DogBreedDetails Map(BreedSingleDto dto)
        {
            var breed = dto.data;

            if (breed?.attributes == null)
                throw new ApiException($"Breed '{_breedId}' response contains no attributes.");

            var description = string.IsNullOrWhiteSpace(breed.attributes.description)
                ? _config.EmptyDescription
                : breed.attributes.description;

            var life = breed.attributes.life;

            return new DogBreedDetails(
                string.IsNullOrEmpty(breed.id) ? _breedId : breed.id,
                breed.attributes.name,
                description,
                life?.min ?? 0,
                life?.max ?? 0,
                breed.attributes.hypoallergenic);
        }
    }
}
