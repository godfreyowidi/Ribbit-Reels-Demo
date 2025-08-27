using System.Net;
using Microsoft.EntityFrameworkCore;
using RibbitReels.Api.DTOs;
using RibbitReels.Data;
using RibbitReels.Data.DTOs;
using RibbitReels.Data.Models;
using RibbitReels.Services.Interfaces;
using RibbitReels.Services.Shared;

namespace RibbitReels.Services.Implementations;

public class LeafService : ILeafService
{
    private readonly AppDbContext _appDbContext;
    private readonly IAzureBlobRepository _blobRepository;

    public LeafService(AppDbContext appDbContext, IAzureBlobRepository blobRepository)
    {
        _appDbContext = appDbContext;
        _blobRepository = blobRepository;
    }

    public async Task<OperationResult<LeafResponse>> CreateManualLeafAsync(Guid branchId, CreateManualLeafRequest request)
    {
        try
        {
            var branch = await _appDbContext.Branches.FindAsync(branchId);
            if (branch == null)
                return OperationResult<LeafResponse>.Fail("Branch not found.", HttpStatusCode.NotFound);

            var nextOrder = request.Order > 0
                ? request.Order
                : await _appDbContext.Leafs.CountAsync(l => l.BranchId == branchId) + 1;

            var leaf = new Leaf
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                Title = string.IsNullOrWhiteSpace(request.Title)
                    ? "Untitled Video"
                    : request.Title[..Math.Min(255, request.Title.Length)],
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description[..Math.Min(2000, request.Description.Length)],
                Source = "Manual",
                Status = "Published",
                Order = nextOrder,
                CreatedAt = DateTime.UtcNow
            };

            if (request.VideoFile != null)
            {
                // we upload to Azure Blob
                var uploadResult = await _blobRepository.UploadVideoAsync(request.VideoFile, leaf.Id.ToString());
                if (!uploadResult.IsSuccessful)
                    return OperationResult<LeafResponse>.Fail(uploadResult.FailureMessage ?? "Video upload failed.", HttpStatusCode.BadRequest);

                // then store blob metadata
                leaf.VideoFileName = request.VideoFile.FileName;
                leaf.VideoContentType = request.VideoFile.ContentType;
                leaf.VideoBlobPath = uploadResult.Value;
            }

            _appDbContext.Leafs.Add(leaf);
            await _appDbContext.SaveChangesAsync();

            var response = await MapToResponseAsync(leaf);
            return OperationResult<LeafResponse>.Success(response, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return OperationResult<LeafResponse>.Fail(ex, "Failed to create manual leaf.");
        }
    }

    public async Task<OperationResult<LeafResponse>> CreateYouTubeLeafAsync(Guid branchId, YouTubeVideo video)
    {
        try
        {
            var branch = await _appDbContext.Branches.FindAsync(branchId);
            if (branch == null)
                return OperationResult<LeafResponse>.Fail("Branch not found.", HttpStatusCode.NotFound);

            // prevent duplicates
            var exists = await _appDbContext.Leafs
                .AnyAsync(l => l.BranchId == branchId && l.YouTubeVideoId == video.VideoId);
            if (exists)
                return OperationResult<LeafResponse>.Fail("This YouTube video already exists in this branch.", HttpStatusCode.Conflict);

            var nextOrder = await _appDbContext.Leafs.CountAsync(l => l.BranchId == branchId) + 1;

            var leaf = new Leaf
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                Title = string.IsNullOrWhiteSpace(video.Title) ? "Untitled Video" : video.Title[..Math.Min(255, video.Title.Length)],
                Description = string.IsNullOrWhiteSpace(video.Description) ? null : video.Description[..Math.Min(2000, video.Description.Length)],
                ThumbnailUrl = video.ThumbnailUrl,
                YouTubeVideoId = video.VideoId,
                Source = "YouTube",
                Status = "Published",
                Order = nextOrder,
                CreatedAt = DateTime.UtcNow
            };

            _appDbContext.Leafs.Add(leaf);
            await _appDbContext.SaveChangesAsync();

            var response = await MapToResponseAsync(leaf);
            return OperationResult<LeafResponse>.Success(response, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return OperationResult<LeafResponse>.Fail(ex, "Failed to create YouTube leaf.");
        }
    }

    public async Task<OperationResult<LeafResponse>> GetLeafByIdAsync(Guid id)
    {
        try
        {
            var leaf = await _appDbContext.Leafs
                .Include(l => l.Branch)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leaf == null)
                return OperationResult<LeafResponse>.Fail("Leaf not found.", HttpStatusCode.NotFound);

            var response = await MapToResponseAsync(leaf);
            return OperationResult<LeafResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return OperationResult<LeafResponse>.Fail($"An error occurred while retrieving the leaf: {ex.Message}", HttpStatusCode.BadRequest);
        }
    }

    public async Task<OperationResult<List<LeafResponse>>> GetLeafsAsync()
    {
        try
        {
            var leaves = await _appDbContext.Leafs
                .Include(l => l.Branch)
                .OrderBy(l => l.Order)
                .ToListAsync();

            var responses = new List<LeafResponse>();
            foreach (var leaf in leaves)
                responses.Add(await MapToResponseAsync(leaf));

            return OperationResult<List<LeafResponse>>.Success(responses);
        }
        catch (Exception)
        {
            return OperationResult<List<LeafResponse>>.Fail("Failed to retrieve leaves.", HttpStatusCode.BadRequest);
        }
    }

    public async Task<OperationResult<List<LeafResponse>>> GetLeafsByBranchIdAsync(Guid branchId)
    {
        try
        {
            var leaves = await _appDbContext.Leafs
                .Where(l => l.BranchId == branchId)
                .OrderBy(l => l.Order)
                .ToListAsync();

            var responses = new List<LeafResponse>();
            foreach (var leaf in leaves)
                responses.Add(await MapToResponseAsync(leaf));

            return OperationResult<List<LeafResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            return OperationResult<List<LeafResponse>>.Fail($"Failed to retrieve leaves for the specified branch. {ex.Message}", HttpStatusCode.BadRequest);
        }
    }

    public async Task<OperationResult<LeafResponse>> UpdateLeafAsync(Guid id, Leaf updatedLeaf)
    {
        try
        {
            var existingLeaf = await _appDbContext.Leafs.FindAsync(id);
            if (existingLeaf == null)
                return OperationResult<LeafResponse>.Fail("Leaf not found", HttpStatusCode.NotFound);

            // Update metadata
            existingLeaf.Title = updatedLeaf.Title ?? existingLeaf.Title;
            existingLeaf.Description = updatedLeaf.Description ?? existingLeaf.Description;
            existingLeaf.Order = updatedLeaf.Order != 0 ? updatedLeaf.Order : existingLeaf.Order;
            existingLeaf.Status = updatedLeaf.Status ?? existingLeaf.Status;

            await _appDbContext.SaveChangesAsync();

            var response = await MapToResponseAsync(existingLeaf);
            return OperationResult<LeafResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return OperationResult<LeafResponse>.Fail(ex, "An error occurred while updating the leaf.");
        }
    }

    public async Task<OperationResult<bool>> DeleteLeafAsync(Guid id)
    {
        var leaf = await _appDbContext.Leafs.FindAsync(id);

        if (leaf == null)
            return OperationResult<bool>.Fail("Leaf not found", HttpStatusCode.NotFound);

        _appDbContext.Leafs.Remove(leaf);

        try
        {
            await _appDbContext.SaveChangesAsync();
            return OperationResult<bool>.Success(true, HttpStatusCode.OK);
        }
        catch (Exception)
        {
            return OperationResult<bool>.Fail("An error occurred while deleting the leaf", HttpStatusCode.BadRequest);
        }
    }

    private async Task<LeafResponse> MapToResponseAsync(Leaf leaf)
    {
        string? videoUrl = null;
        if (!string.IsNullOrEmpty(leaf.VideoBlobPath))
        {
            var sasResult = await _blobRepository.GetVideoUrlAsync(leaf.VideoBlobPath);
            if (sasResult.IsSuccessful)
                videoUrl = sasResult.Value;
        }

        return new LeafResponse
        {
            Id = leaf.Id,
            BranchId = leaf.BranchId,
            Title = leaf.Title,
            Description = leaf.Description ?? string.Empty,
            Order = leaf.Order,
            Source = leaf.Source,
            Status = leaf.Status,
            VideoUrl = videoUrl,
            YouTubeVideoId = leaf.YouTubeVideoId,
            ThumbnailUrl = leaf.ThumbnailUrl,
            CreatedAt = leaf.CreatedAt
        };
    }
}
