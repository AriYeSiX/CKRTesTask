using System;
using System.Collections.Generic;
using CKR.Configs;
using CKR.Core.Net;
using CKR.Core.Requests;
using CKR.Features.Dogs.Model;

namespace CKR.Features.Dogs.Api
{
    public sealed class BreedsListRequest : JsonGetRequest<IReadOnlyList<DogBreed>, BreedsListDto>
    {
        private readonly DogApiConfig _config;

        public BreedsListRequest(DogApiConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public override string Description => "dogs:breeds";

        protected override string BuildUrl()
        {
            return _config.BreedsUrl;
        }

        protected override IReadOnlyList<DogBreed> Map(BreedsListDto dto)
        {
            if (dto.data == null)
                throw new ApiException("Breeds response contains no data.");

            var limit = Math.Min(_config.BreedsLimit, dto.data.Length);
            var breeds = new List<DogBreed>(limit);

            for (var i = 0; i < dto.data.Length && breeds.Count < limit; i++)
            {
                var entry = dto.data[i];
                if (entry?.attributes == null || string.IsNullOrEmpty(entry.id))
                    continue;

                breeds.Add(new DogBreed(entry.id, entry.attributes.name));
            }

            return breeds;
        }
    }
}
