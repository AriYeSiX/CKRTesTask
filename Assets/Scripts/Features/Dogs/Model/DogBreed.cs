namespace CKR.Features.Dogs.Model
{
    public sealed class DogBreed
    {
        public DogBreed(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
    }

    public sealed class DogBreedDetails
    {
        public DogBreedDetails(string id, string name, string description, int lifeMin, int lifeMax,
            bool hypoallergenic)
        {
            Id = id;
            Name = name;
            Description = description;
            LifeMin = lifeMin;
            LifeMax = lifeMax;
            Hypoallergenic = hypoallergenic;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int LifeMin { get; }
        public int LifeMax { get; }
        public bool Hypoallergenic { get; }
    }
}
