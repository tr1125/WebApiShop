using Entities;
using Repositories;


namespace Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _repository;
        public RatingService(IRatingRepository repository)
        {
            _repository = repository;
        }


        public async Task<Rating> AddRating(Rating rating)
        {
            Rating ratingRes = await _repository.AddRating(rating);
            return ratingRes;
        }

    }
}
