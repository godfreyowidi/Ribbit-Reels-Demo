using System.Net;
using Microsoft.EntityFrameworkCore;
using RibbitReels.Data;
using RibbitReels.Data.DTOs;
using RibbitReels.Data.Models;
using RibbitReels.Services.Interfaces;
using RibbitReels.Services.Shared;

namespace RibbitReels.Services.Implementations;

public class UserBranchAssignmentService : IUserBranchAssignmentService
{
    private readonly AppDbContext _appDbContext;

    public UserBranchAssignmentService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<OperationResult<UserBranchAssignmentResponse>> AssignBranchAsync(InternalAssignBranchRequest request)
    {
        // check if user exists
        var user = await _appDbContext.Users.FindAsync(request.UserId);
        if (user == null)
            return OperationResult<UserBranchAssignmentResponse>.Fail("User not found", HttpStatusCode.NotFound);

        // check if branch exists
        var branch = await _appDbContext.Branches.FindAsync(request.BranchId);
        if (branch == null)
            return OperationResult<UserBranchAssignmentResponse>.Fail("Branch not found", HttpStatusCode.NotFound);

        // check if admin exists
        var manager = await _appDbContext.Users.FindAsync(request.AssignedByManagerId);
        if (manager == null || manager.Role != UserRole.Admin)
            return OperationResult<UserBranchAssignmentResponse>.Fail("Invalid assigning manager", HttpStatusCode.Forbidden);

        // prevent duplicate assignment - of the same branch
        var existingAssignment = await _appDbContext.UserBranchAssignment
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.BranchId == request.BranchId);

        if (existingAssignment != null)
            return OperationResult<UserBranchAssignmentResponse>.Fail("Branch already assigned to user", HttpStatusCode.Conflict);

        // create new assignment
        var assignment = new UserBranchAssignment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            BranchId = request.BranchId,
            AssignedByManagerId = request.AssignedByManagerId
        };

        _appDbContext.UserBranchAssignment.Add(assignment);
        await _appDbContext.SaveChangesAsync();

        return OperationResult<UserBranchAssignmentResponse>.Success(new UserBranchAssignmentResponse
        {
            UserId = assignment.UserId,
            BranchId = assignment.BranchId,
            AssignedByManagerId = assignment.AssignedByManagerId
        });
    }

    public async Task<OperationResult<List<UserBranchAssignmentResponse>>> GetAssignmentsByManagerAsync(Guid managerId)
    {
        var assignments = await _appDbContext.UserBranchAssignment
            .Where(x => x.AssignedByManagerId == managerId)
            .Select(a => new UserBranchAssignmentResponse
            {
                UserId = a.UserId,
                BranchId = a.BranchId,
                AssignedByManagerId = a.AssignedByManagerId
            })
            .ToListAsync();

        return OperationResult<List<UserBranchAssignmentResponse>>.Success(assignments);
    }

    public async Task<OperationResult<List<UserBranchAssignmentResponse>>> GetAssignmentsByUserAsync(Guid userId)
    {
        var assignments = await _appDbContext.UserBranchAssignment
            .Where(x => x.UserId == userId)
            .Include(a => a.Branch)
                .ThenInclude(b => b.Leafs)
            .Select(a => new UserBranchAssignmentResponse
            {
                UserId = a.UserId,
                BranchId = a.BranchId,
                AssignedByManagerId = a.AssignedByManagerId,
                Branch = new BranchResponse
                {
                    Id = a.Branch.Id,
                    Title = a.Branch.Title,
                    Description = a.Branch.Description,
                    Leafs = a.Branch.Leafs
                        .OrderBy(l => l.Order)
                        .Select(l => new LeafResponse
                        {
                            Id = l.Id,
                            BranchId = l.BranchId,
                            Title = l.Title,
                            Order = l.Order
                        }).ToList()
                }
            })
            .ToListAsync();

        return OperationResult<List<UserBranchAssignmentResponse>>.Success(assignments);
    }

    public async Task<OperationResult<List<UserBranchAssignmentResponse>>> GetAllAssignmentsAsync()
    {
        try
        {
            var assignments = await _appDbContext.UserBranchAssignment
                .Include(a => a.Branch)
                    .ThenInclude(b => b.Leafs)
                .Select(a => new UserBranchAssignmentResponse
                {
                    UserId = a.UserId,
                    BranchId = a.BranchId,
                    AssignedByManagerId = a.AssignedByManagerId,
                    Branch = new BranchResponse
                    {
                        Id = a.Branch.Id,
                        Title = a.Branch.Title,
                        Description = a.Branch.Description,
                        Leafs = a.Branch.Leafs
                            .OrderBy(l => l.Order)
                            .Select(l => new LeafResponse
                            {
                                Id = l.Id,
                                BranchId = l.BranchId,
                                Title = l.Title,
                                Order = l.Order
                            }).ToList()
                    }
                })
                .ToListAsync();

            return OperationResult<List<UserBranchAssignmentResponse>>.Success(assignments);
        }
        catch (Exception ex)
        {
            return OperationResult<List<UserBranchAssignmentResponse>>.Fail(ex, "Failed to fetch all assignments.");
        }
    }

    public async Task<OperationResult<bool>> UnassignBranchAsync(Guid userId, Guid branchId)
    {
        try
        {
            var assignment = await _appDbContext.UserBranchAssignment
                .FirstOrDefaultAsync(a => a.UserId == userId && a.BranchId == branchId);

            if (assignment == null)
                return OperationResult<bool>.Fail("Assignment not found.", HttpStatusCode.NotFound);

            _appDbContext.UserBranchAssignment.Remove(assignment);
            await _appDbContext.SaveChangesAsync();

            return OperationResult<bool>.Success(true, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail(ex, "Failed to unassign branch.");
        }
    }
}