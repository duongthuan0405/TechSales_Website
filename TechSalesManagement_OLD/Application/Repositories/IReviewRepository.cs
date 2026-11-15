using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface IReviewRepository
{
    Task AddReviewAsync(Review review);
    Task<(List<Review> reviews, int totalCount, decimal averageRating)> GetReviewsByProductIdAsync(Guid productId, int pageNumber, int pageSize);
    Task<(List<Review> reviews, int totalCount)> GetLatestReviewsAsync(int pageNumber, int pageSize);
    Task<Review?> GetReviewByIdAsync(Guid id);
    Task UpdateReviewAsync(Review review);
    Task AddReviewResponseAsync(ReviewResponse response);
}
