using System;
using System.Linq;
using System.Threading.Tasks;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Common;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(
        IReviewRepository reviewRepository,
        IOrderRepository orderRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddReviewAsync(AddReviewParams parameters)
    {
        // 1. BR132: Validation Rules (Rating stars)
        if (parameters.RatingStars < 1 || parameters.RatingStars > 5)
        {
            throw new BadRequestException(MessageConstants.MSG49);
        }

        if (parameters.ProductId == Guid.Empty || parameters.OrderId == Guid.Empty)
        {
            throw new BadRequestException("Product ID and Order ID are required.");
        }

        // 2. Purchase Validation (Ensure order is DELIVERED and contains this product)
        var order = await _orderRepository.GetOrderDetailsByIdAsync(parameters.OrderId);

        if (order == null || order.userId != parameters.UserId)
        {
            throw new NotFoundException(MessageConstants.MSG43); // Order not found
        }

        // Ensure delivered
        if (order.status != OrderStatus.DELIVERED)
        {
            throw new BadRequestException("You can only review products from successfully delivered orders.");
        }

        // Ensure product exists in order
        bool hasPurchased = order.items != null && order.items.Any(i => i.product_id == parameters.ProductId);
        if (!hasPurchased)
        {
            throw new BadRequestException("You can only review products you actually purchased in this order.");
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var review = new Review
            {
                userId = parameters.UserId,
                productId = parameters.ProductId,
                rating = parameters.RatingStars,
                comment = parameters.ReviewComment,
                status = ReviewStatus.VISIBLE
            };

            // BR134: Store Review
            await _reviewRepository.AddReviewAsync(review);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ProductReviewsResult> GetProductReviewsAsync(GetProductReviewsParams parameters)
    {
        if (parameters.ProductId == Guid.Empty)
        {
            throw new BadRequestException("Product ID is required.");
        }

        if (parameters.PageNumber <= 0) parameters.PageNumber = 1;
        if (parameters.PageSize <= 0) parameters.PageSize = 10;

        // BR137: Perform review search and calculations
        var (reviews, totalCount, averageRating) = await _reviewRepository.GetReviewsByProductIdAsync(
            parameters.ProductId,
            parameters.PageNumber,
            parameters.PageSize
        );

        return new ProductReviewsResult
        {
            Reviews = reviews,
            TotalCount = totalCount,
            AverageRating = averageRating
        };
    }

    public async Task<(List<Review> reviews, int totalCount)> GetLatestReviewsAsync(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;

        return await _reviewRepository.GetLatestReviewsAsync(pageNumber, pageSize);
    }

    public async Task ReplyToReviewAsync(Guid reviewId, string replyContent, Guid staffId)
    {
        if (string.IsNullOrWhiteSpace(replyContent))
        {
            throw new BadRequestException(MessageConstants.MSG68);
        }

        var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
        if (review == null)
        {
            throw new NotFoundException("Review not found.");
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var response = new ReviewResponse(reviewId, staffId, replyContent);
            await _reviewRepository.AddReviewResponseAsync(response);

            // Ghi log hành động (Part of Observer logic)
            var auditLog = new AuditLog(
                staffId,
                "REPLY_REVIEW",
                "Reviews",
                reviewId.ToString()
            )
            {
                newValues = System.Text.Json.JsonSerializer.Serialize(new { replyContent = replyContent })
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task HideReviewAsync(Guid reviewId, string reason, Guid staffId)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BadRequestException(MessageConstants.MSG70);
        }

        var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
        if (review == null)
        {
            throw new NotFoundException("Review not found.");
        }

        try
        {
            await _unitOfWork.BeginAsync();

            review.status = ReviewStatus.HIDDEN;
            review.violationReason = reason;

            await _reviewRepository.UpdateReviewAsync(review);

            var auditLog = new AuditLog(
                staffId,
                "HIDE_REVIEW",
                "Reviews",
                reviewId.ToString()
            )
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { status = ReviewStatus.VISIBLE.ToString() }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { status = ReviewStatus.HIDDEN.ToString(), violationReason = reason }),
                affectedColumns = "status,violationReason"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
