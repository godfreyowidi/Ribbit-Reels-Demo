using System.Net;
using Microsoft.EntityFrameworkCore;
using RibbitReels.Data;
using RibbitReels.Data.DTOs;
using RibbitReels.Data.Models;
using RibbitReels.Services.Interfaces;
using RibbitReels.Services.Shared;

namespace RibbitReels.Services.Implementations;

public class LearningProgressService : ILearningProgressService
{
    private readonly AppDbContext _appDbContext;

    public LearningProgressService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<OperationResult<LearningProgressResponse>> GetProgressAsync(Guid userId, Guid branchId)
    {
        var branch = await _appDbContext.Branches
            .Include(b => b.Leafs)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null)
            return OperationResult<LearningProgressResponse>.Fail("Branch not found", HttpStatusCode.NotFound);

        var progress = await _appDbContext.LearningProgress
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.BranchId == branchId);

        if (progress == null)
            return OperationResult<LearningProgressResponse>.Fail("No progress found", HttpStatusCode.NotFound);

        var completedLeafs = progress?.CompletedLeafIds ?? new List<Guid>();
        var totalLeafs = branch.Leafs.Count;
        var validCompleted = completedLeafs.Intersect(branch.Leafs.Select(l => l.Id)).ToList();


        var percentageCompleted = totalLeafs == 0
            ? 0
            : validCompleted.Count / (double)totalLeafs * 100;

        return OperationResult<LearningProgressResponse>.Success(new LearningProgressResponse
        {
            UserId = userId,
            BranchId = branchId,
            CompletedLeafIds = validCompleted,
            CompletedAt = progress?.CompletedAt,
            PercentageCompleted = Math.Round(percentageCompleted, 2)
        });
    }


    public async Task<OperationResult<LearningProgressResponse>> UpdateProgressAsync(UpdateProgressRequest request)
    {
        var branch = await _appDbContext.Branches
            .Include(b => b.Leafs)
            .FirstOrDefaultAsync(b => b.Id == request.BranchId);

        if (branch == null)
            return OperationResult<LearningProgressResponse>.Fail("Branch not found", HttpStatusCode.NotFound);

        var validLeafIds = branch.Leafs.Select(l => l.Id).ToHashSet();
        var incomingLeafIds = (request.CompletedLeafIds ?? new List<Guid>())
            .Where(validLeafIds.Contains)
            .Distinct()
            .ToList();

        var progress = await _appDbContext.LearningProgress
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.BranchId == request.BranchId);

        if (progress == null)
        {
            progress = new LearningProgress
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                BranchId = request.BranchId,
                CompletedLeafIds = incomingLeafIds
            };
            _appDbContext.LearningProgress.Add(progress);
        }
        else
        {
            progress.CompletedLeafIds = progress.CompletedLeafIds
                .Union(incomingLeafIds)
                .Where(validLeafIds.Contains)
                .Distinct()
                .ToList();

            _appDbContext.LearningProgress.Update(progress);
        }

        // mark branch completed if ALL leafs are all watched
        if (branch.Leafs.All(l => progress.CompletedLeafIds.Contains(l.Id)))
            progress.CompletedAt ??= DateTime.UtcNow;

        await _appDbContext.SaveChangesAsync();

        var percentageCompleted = branch.Leafs.Count == 0 
            ? 0 
            : progress.CompletedLeafIds.Count / (double)branch.Leafs.Count * 100;

        return OperationResult<LearningProgressResponse>.Success(new LearningProgressResponse
        {
            UserId = request.UserId,
            BranchId = request.BranchId,
            CompletedLeafIds = progress.CompletedLeafIds,
            CompletedAt = progress.CompletedAt,
            PercentageCompleted = Math.Round(percentageCompleted, 2)
        });
    }

    public async Task<OperationResult<List<CompletedBranchResponse>>> GetCompletedBranchesAsync(Guid userId)
    {
        var completed = await _appDbContext.LearningProgress
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.CompletedAt != null)
            .Select(p => new CompletedBranchResponse
            {
                BranchId = p.BranchId,
                CompletedAt = p.CompletedAt
            })
            .ToListAsync();

        return OperationResult<List<CompletedBranchResponse>>.Success(completed);
    }
}
