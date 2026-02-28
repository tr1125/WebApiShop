using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly WebApiShopContext _webApiShopContext;

        public RatingRepository(WebApiShopContext webApiShopContext)
        {
            _webApiShopContext = webApiShopContext;
        }


        public async Task<Rating> AddRating(Rating rating)
        {
            await _webApiShopContext.Rating.AddAsync(rating);
            await _webApiShopContext.SaveChangesAsync();
            return rating;

        }
    }
}
