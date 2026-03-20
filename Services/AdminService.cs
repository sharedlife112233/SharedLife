using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLife.Data;
using SharedLife.Models.DTOs.Admin;
using SharedLife.Models.Entities;
using SharedLife.Models.Enums;
using SharedLife.Services.Interfaces;
using SharedLife.Utilities;

namespace SharedLife.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminService> _logger;

    private static readonly Dictionary<BloodGroup, string> BloodGroupDisplayNames = new()
    {
        { BloodGroup.APositive, "A+" },
        { BloodGroup.ANegative, "A-" },
        { BloodGroup.BPositive, "B+" },
        { BloodGroup.BNegative, "B-" },
        { BloodGroup.ABPositive, "AB+" },
        { BloodGroup.ABNegative, "AB-" },
        { BloodGroup.OPositive, "O+" },
        { BloodGroup.ONegative, "O-" }
    };

    public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Dashboard

    public async Task<(bool Success, string Message, DashboardStatsDto? Data)> GetDashboardStatsAsync()
    {
        try
        {
            var now = TimeHelper.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var stats = new DashboardStatsDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalDonors = await _context.Donors.CountAsync(),
                TotalRecipients = await _context.Recipients.CountAsync(),
                TotalDonationRequests = await _context.DonationRequests.CountAsync(),
                PendingRequests = await _context.DonationRequests.CountAsync(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.Sent),
                CompletedRequests = await _context.DonationRequests.CountAsync(r => r.Status == RequestStatus.Completed),
                ActiveDonors = await _context.Donors.CountAsync(d => d.IsAvailable),
                VerifiedDonors = await _context.Donors.CountAsync(d => d.VerifiedAt != null),
                CriticalRequests = await _context.DonationRequests.CountAsync(r => r.UrgencyLevel == UrgencyLevel.Critical && r.Status != RequestStatus.Completed && r.Status != RequestStatus.Cancelled),
                NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= startOfMonth),
                NewDonorsThisMonth = await _context.Donors.CountAsync(d => d.CreatedAt >= startOfMonth),
                NewRequestsThisMonth = await _context.DonationRequests.CountAsync(r => r.CreatedAt >= startOfMonth),
                
                // Hospital statistics
                TotalHospitals = await _context.Hospitals.CountAsync(),
                VerifiedHospitals = await _context.Hospitals.CountAsync(h => h.IsVerified),
                PendingHospitalVerifications = await _context.Hospitals.CountAsync(h => !h.IsVerified),
            };

            // Blood group distribution for donors
            var donorsByBloodGroup = await _context.Donors
                .GroupBy(d => d.BloodGroup)
                .Select(g => new { BloodGroup = g.Key, Count = g.Count() })
                .ToListAsync();
            
            foreach (var item in donorsByBloodGroup)
            {
                var displayName = BloodGroupDisplayNames.GetValueOrDefault(item.BloodGroup, item.BloodGroup.ToString());
                stats.DonorsByBloodGroup[displayName] = item.Count;
            }

            // Blood group distribution for requests
            var requestsByBloodGroup = await _context.DonationRequests
                .Where(r => r.Status != RequestStatus.Cancelled)
                .GroupBy(r => r.BloodGroup)
                .Select(g => new { BloodGroup = g.Key, Count = g.Count() })
                .ToListAsync();
            
            foreach (var item in requestsByBloodGroup)
            {
                var displayName = BloodGroupDisplayNames.GetValueOrDefault(item.BloodGroup, item.BloodGroup.ToString());
                stats.RequestsByBloodGroup[displayName] = item.Count;
            }

            // Recent activities (last 10)
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentActivityDto
                {
                    Id = u.Id,
                    Type = "UserRegistered",
                    Description = $"New user registered: {u.FullName}",
                    UserName = u.FullName,
                    Timestamp = u.CreatedAt
                })
                .ToListAsync();

            var recentRequests = await _context.DonationRequests
                .Include(r => r.Recipient)
                    .ThenInclude(rec => rec.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentActivityDto
                {
                    Id = r.Id,
                    Type = "RequestCreated",
                    Description = $"New {r.DonationType} request for {r.BloodGroup}",
                    UserName = r.Recipient.User.FullName,
                    Timestamp = r.CreatedAt
                })
                .ToListAsync();

            stats.RecentActivities = recentUsers.Concat(recentRequests)
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            return (true, "Dashboard stats retrieved successfully", stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return (false, "Failed to retrieve dashboard stats", null);
        }
    }

    #endregion

    #region User Management

    public async Task<(bool Success, string Message, UserListResponseDto? Data)> GetAllUsersAsync(int page, int pageSize, string? search, string? role)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u => 
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            // Apply role filter
            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, out var userRole))
            {
                query = query.Where(u => u.Role == userRole);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserManagementDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber ?? "",
                    Role = u.Role.ToString(),
                    BloodGroup = u.BloodGroup.HasValue ? u.BloodGroup.Value.ToString() : null,
                    IsActive = u.IsActive,
                    IsVerified = u.IsVerified,
                    CreatedAt = u.CreatedAt,
                    HasDonorProfile = _context.Donors.Any(d => d.UserId == u.Id),
                    HasRecipientProfile = _context.Recipients.Any(r => r.UserId == u.Id)
                })
                .ToListAsync();

            return (true, "Users retrieved successfully", new UserListResponseDto
            {
                Users = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return (false, "Failed to retrieve users", null);
        }
    }

    public async Task<(bool Success, string Message, UserDetailsDto? Data)> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            var dto = new UserDetailsDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? "",
                Address = user.Address,
                Role = user.Role.ToString(),
                BloodGroup = user.BloodGroup.HasValue ? user.BloodGroup.Value.ToString() : null,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            // Get donor info if exists
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (donor != null)
            {
                dto.DonorInfo = new DonorInfoDto
                {
                    Id = donor.Id,
                    BloodGroup = donor.BloodGroup.ToString(),
                    Status = donor.Status.ToString(),
                    IsAvailable = donor.IsAvailable,
                    TotalBloodDonations = donor.TotalBloodDonations,
                    LastBloodDonationDate = donor.LastBloodDonationDate,
                    WillingToDonatePlasma = donor.WillingToDonatePlasma,
                    WillingToDonatePlatelets = donor.WillingToDonatePlatelets,
                    WillingToDonateOrgan = donor.WillingToDonateOrgan,
                    VerifiedAt = donor.VerifiedAt,
                    CreatedAt = donor.CreatedAt
                };
            }

            // Get recipient info if exists
            var recipient = await _context.Recipients.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recipient != null)
            {
                var requestCounts = await _context.DonationRequests
                    .Where(r => r.RecipientId == recipient.Id)
                    .GroupBy(r => r.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                dto.RecipientInfo = new RecipientInfoDto
                {
                    Id = recipient.Id,
                    BloodGroup = recipient.BloodGroup.ToString(),
                    MedicalCondition = recipient.MedicalCondition,
                    HospitalName = recipient.HospitalName,
                    City = recipient.City,
                    IsVerified = recipient.IsVerified,
                    CreatedAt = recipient.CreatedAt,
                    TotalRequests = requestCounts.Sum(r => r.Count),
                    PendingRequests = requestCounts.Where(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.Sent).Sum(r => r.Count)
                };
            }

            return (true, "User retrieved successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return (false, "Failed to retrieve user", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;
            
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;
            
            if (dto.Address != null)
                user.Address = dto.Address;
            
            if (!string.IsNullOrWhiteSpace(dto.Role) && Enum.TryParse<UserRole>(dto.Role, out var role))
                user.Role = role;
            
            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;
            
            if (dto.IsVerified.HasValue)
                user.IsVerified = dto.IsVerified.Value;

            user.UpdatedAt = TimeHelper.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin updated user {UserId}", userId);
            return (true, "User updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return (false, "Failed to update user");
        }
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Prevent deleting admin users
            if (user.Role == UserRole.Admin)
            {
                return (false, "Cannot delete admin users");
            }

            // Soft delete - set IsActive to false
            user.IsActive = false;
            user.UpdatedAt = TimeHelper.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin soft-deleted user {UserId}", userId);
            return (true, "User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return (false, "Failed to delete user");
        }
    }

    public async Task<(bool Success, string Message)> VerifyUserAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            user.IsVerified = true;
            user.UpdatedAt = TimeHelper.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin verified user {UserId}", userId);
            return (true, "User verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying user {UserId}", userId);
            return (false, "Failed to verify user");
        }
    }

    #endregion

    #region Donor Management

    public async Task<(bool Success, string Message, DonorListResponseDto? Data)> GetAllDonorsAsync(int page, int pageSize, string? search, string? bloodGroup, bool? isAvailable)
    {
        try
        {
            var query = _context.Donors
                .Include(d => d.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(d => 
                    d.User.FullName.ToLower().Contains(search) ||
                    d.User.Email.ToLower().Contains(search) ||
                    (d.User.PhoneNumber != null && d.User.PhoneNumber.Contains(search)));
            }

            // Apply blood group filter
            if (!string.IsNullOrWhiteSpace(bloodGroup) && Enum.TryParse<BloodGroup>(bloodGroup, out var bg))
            {
                query = query.Where(d => d.BloodGroup == bg);
            }

            // Apply availability filter
            if (isAvailable.HasValue)
            {
                query = query.Where(d => d.IsAvailable == isAvailable.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donorRows = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    FullName = d.User.FullName,
                    Email = d.User.Email,
                    PhoneNumber = d.User.PhoneNumber ?? "",
                    BloodGroup = d.BloodGroup,
                    Status = d.Status,
                    IsAvailable = d.IsAvailable,
                    IsVerified = d.VerifiedAt != null,
                    TotalBloodDonations = d.TotalBloodDonations,
                    LastBloodDonationDate = d.LastBloodDonationDate,
                    WillingToDonatePlasma = d.WillingToDonatePlasma,
                    WillingToDonatePlatelets = d.WillingToDonatePlatelets,
                    WillingToDonateOrgan = d.WillingToDonateOrgan,
                    WillingToDonateBoneMarrow = d.WillingToDonateBoneMarrow,
                    WillingToDonateEye = d.WillingToDonateEye,
                    Weight = d.Weight,
                    Height = d.Height,
                    HasChronicDisease = d.HasChronicDisease,
                    HasInfectiousDisease = d.HasInfectiousDisease,
                    CreatedAt = d.CreatedAt,
                    VerifiedAt = d.VerifiedAt
                })
                .ToListAsync();

            var donors = donorRows.Select(d => new DonorListDto
            {
                Id = d.Id,
                UserId = d.UserId,
                FullName = d.FullName,
                Email = d.Email,
                PhoneNumber = d.PhoneNumber,
                BloodGroup = d.BloodGroup.ToString(),
                BloodGroupDisplay = BloodGroupDisplayNames.GetValueOrDefault(d.BloodGroup, d.BloodGroup.ToString()),
                Status = d.Status.ToString(),
                IsAvailable = d.IsAvailable,
                IsVerified = d.IsVerified,
                TotalBloodDonations = d.TotalBloodDonations,
                LastBloodDonationDate = d.LastBloodDonationDate,
                WillingToDonatePlasma = d.WillingToDonatePlasma,
                WillingToDonatePlatelets = d.WillingToDonatePlatelets,
                WillingToDonateOrgan = d.WillingToDonateOrgan,
                WillingToDonateBoneMarrow = d.WillingToDonateBoneMarrow,
                WillingToDonateEye = d.WillingToDonateEye,
                Weight = d.Weight,
                Height = d.Height,
                HasChronicDisease = d.HasChronicDisease,
                HasInfectiousDisease = d.HasInfectiousDisease,
                CreatedAt = d.CreatedAt,
                VerifiedAt = d.VerifiedAt
            }).ToList();

            return (true, "Donors retrieved successfully", new DonorListResponseDto
            {
                Donors = donors,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting donors");
            return (false, "Failed to retrieve donors", null);
        }
    }

    public async Task<(bool Success, string Message)> VerifyDonorAsync(int donorId)
    {
        try
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Id == donorId);
            if (donor == null)
            {
                return (false, "Donor not found");
            }

            donor.VerifiedAt = TimeHelper.Now;
            donor.Status = DonorStatus.Active;
            donor.UpdatedAt = TimeHelper.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin verified donor {DonorId}", donorId);
            return (true, "Donor verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying donor {DonorId}", donorId);
            return (false, "Failed to verify donor");
        }
    }

    #endregion

    #region Recipient Management

    public async Task<(bool Success, string Message, RecipientListResponseDto? Data)> GetAllRecipientsAsync(int page, int pageSize, string? search, string? bloodGroup)
    {
        try
        {
            var query = _context.Recipients
                .Include(r => r.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r => 
                    r.User.FullName.ToLower().Contains(search) ||
                    r.User.Email.ToLower().Contains(search) ||
                    (r.User.PhoneNumber != null && r.User.PhoneNumber.Contains(search)) ||
                    (r.HospitalName != null && r.HospitalName.ToLower().Contains(search)));
            }

            // Apply blood group filter
            if (!string.IsNullOrWhiteSpace(bloodGroup) && Enum.TryParse<BloodGroup>(bloodGroup, out var bg))
            {
                query = query.Where(r => r.BloodGroup == bg);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var recipientIds = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => r.Id)
                .ToListAsync();

            var recipients = new List<RecipientListDto>();
            foreach (var id in recipientIds)
            {
                var r = await _context.Recipients
                    .Include(rec => rec.User)
                    .FirstAsync(rec => rec.Id == id);
                
                var requestCounts = await _context.DonationRequests
                    .Where(req => req.RecipientId == id)
                    .GroupBy(req => req.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                recipients.Add(new RecipientListDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    FullName = r.User.FullName,
                    Email = r.User.Email,
                    PhoneNumber = r.User.PhoneNumber ?? "",
                    BloodGroup = r.BloodGroup.ToString(),
                    BloodGroupDisplay = BloodGroupDisplayNames.GetValueOrDefault(r.BloodGroup, r.BloodGroup.ToString()),
                    MedicalCondition = r.MedicalCondition,
                    HospitalName = r.HospitalName,
                    City = r.City,
                    IsVerified = r.IsVerified,
                    CreatedAt = r.CreatedAt,
                    VerifiedAt = r.VerifiedAt,
                    TotalRequests = requestCounts.Sum(c => c.Count),
                    PendingRequests = requestCounts.Where(c => c.Status == RequestStatus.Pending || c.Status == RequestStatus.Sent).Sum(c => c.Count),
                    CompletedRequests = requestCounts.Where(c => c.Status == RequestStatus.Completed).Sum(c => c.Count)
                });
            }

            return (true, "Recipients retrieved successfully", new RecipientListResponseDto
            {
                Recipients = recipients,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recipients");
            return (false, "Failed to retrieve recipients", null);
        }
    }

    public async Task<(bool Success, string Message)> VerifyRecipientAsync(int recipientId)
    {
        try
        {
            var recipient = await _context.Recipients.FirstOrDefaultAsync(r => r.Id == recipientId);
            if (recipient == null)
            {
                return (false, "Recipient not found");
            }

            recipient.IsVerified = true;
            recipient.VerifiedAt = TimeHelper.Now;
            recipient.UpdatedAt = TimeHelper.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin verified recipient {RecipientId}", recipientId);
            return (true, "Recipient verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying recipient {RecipientId}", recipientId);
            return (false, "Failed to verify recipient");
        }
    }

    #endregion

    #region Request Management

    public async Task<(bool Success, string Message, RequestListResponseDto? Data)> GetAllRequestsAsync(int page, int pageSize, string? status, string? urgencyLevel)
    {
        try
        {
            var query = _context.DonationRequests
                .Include(r => r.Recipient)
                    .ThenInclude(rec => rec.User)
                .AsQueryable();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RequestStatus>(status, out var requestStatus))
            {
                query = query.Where(r => r.Status == requestStatus);
            }

            // Apply urgency level filter
            if (!string.IsNullOrWhiteSpace(urgencyLevel) && Enum.TryParse<UrgencyLevel>(urgencyLevel, out var urgency))
            {
                query = query.Where(r => r.UrgencyLevel == urgency);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var requestRows = await query
                .OrderByDescending(r => r.UrgencyLevel)
                .ThenByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    Id = r.Id,
                    RecipientId = r.RecipientId,
                    RecipientName = r.Recipient.User.FullName,
                    RecipientEmail = r.Recipient.User.Email,
                    BloodGroup = r.BloodGroup,
                    DonationType = r.DonationType,
                    Quantity = r.Quantity,
                    UrgencyLevel = r.UrgencyLevel,
                    Status = r.Status,
                    HospitalName = r.HospitalName,
                    City = r.City,
                    ContactName = r.ContactName,
                    ContactPhone = r.ContactPhone,
                    RequiredDateTime = r.RequiredDateTime,
                    MatchedDonorsCount = r.MatchedDonorsCount,
                    AcceptedDonorsCount = r.AcceptedDonorsCount,
                    CreatedAt = r.CreatedAt,
                    CompletedAt = r.CompletedAt
                })
                .ToListAsync();

            var requests = requestRows.Select(r => new RequestListDto
            {
                Id = r.Id,
                RecipientId = r.RecipientId,
                RecipientName = r.RecipientName,
                RecipientEmail = r.RecipientEmail,
                BloodGroup = r.BloodGroup.ToString(),
                BloodGroupDisplay = BloodGroupDisplayNames.GetValueOrDefault(r.BloodGroup, r.BloodGroup.ToString()),
                DonationType = r.DonationType.ToString(),
                Quantity = r.Quantity,
                UrgencyLevel = r.UrgencyLevel.ToString(),
                Status = r.Status.ToString(),
                HospitalName = r.HospitalName,
                City = r.City,
                ContactName = r.ContactName,
                ContactPhone = r.ContactPhone,
                RequiredDateTime = r.RequiredDateTime,
                MatchedDonorsCount = r.MatchedDonorsCount,
                AcceptedDonorsCount = r.AcceptedDonorsCount,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt
            }).ToList();

            return (true, "Requests retrieved successfully", new RequestListResponseDto
            {
                Requests = requests,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting requests");
            return (false, "Failed to retrieve requests", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateRequestStatusAsync(int requestId, UpdateRequestStatusDto dto)
    {
        try
        {
            var request = await _context.DonationRequests.FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null)
            {
                return (false, "Request not found");
            }

            if (!Enum.TryParse<RequestStatus>(dto.Status, out var newStatus))
            {
                return (false, "Invalid status");
            }

            request.Status = newStatus;
            request.UpdatedAt = TimeHelper.Now;
            
            if (newStatus == RequestStatus.Completed)
            {
                request.CompletedAt = TimeHelper.Now;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin updated request {RequestId} status to {Status}", requestId, dto.Status);
            return (true, "Request status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating request {RequestId} status", requestId);
            return (false, "Failed to update request status");
        }
    }

    #endregion

    #region Hospital Management

    public async Task<(bool Success, string Message, HospitalListResponseDto? Data)> GetAllHospitalsAsync(int page, int pageSize, string? search, bool? isVerified)
    {
        try
        {
            var query = _context.Hospitals
                .Include(h => h.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(h =>
                    h.HospitalName.ToLower().Contains(search) ||
                    h.RegistrationNumber.ToLower().Contains(search) ||
                    h.City.ToLower().Contains(search) ||
                    h.ContactEmail.ToLower().Contains(search));
            }

            // Apply verified filter
            if (isVerified.HasValue)
            {
                query = query.Where(h => h.IsVerified == isVerified.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var hospitals = await query
                .OrderByDescending(h => h.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HospitalListDto
                {
                    Id = h.Id,
                    UserId = h.UserId,
                    HospitalName = h.HospitalName,
                    RegistrationNumber = h.RegistrationNumber,
                    HospitalType = h.HospitalType,
                    ContactEmail = h.ContactEmail,
                    ContactPhone = h.ContactPhone,
                    City = h.City,
                    State = h.State,
                    Address = h.Address,
                    HasBloodBank = h.HasBloodBank,
                    HasOrganTransplant = h.HasOrganTransplant,
                    HasEyeBank = h.HasEyeBank,
                    HasBoneMarrowRegistry = h.HasBoneMarrowRegistry,
                    IsVerified = h.IsVerified,
                    IsActive = h.IsActive,
                    VerifiedAt = h.VerifiedAt,
                    VerifiedByAdminId = h.VerifiedByAdminId,
                    VerificationNotes = h.VerificationNotes,
                    TotalDonorsVerified = h.TotalDonorsVerified,
                    TotalRequestsProcessed = h.TotalRequestsProcessed,
                    CreatedAt = h.CreatedAt,
                    UpdatedAt = h.UpdatedAt
                })
                .ToListAsync();

            return (true, "Hospitals retrieved successfully", new HospitalListResponseDto
            {
                Hospitals = hospitals,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hospitals");
            return (false, "Failed to retrieve hospitals", null);
        }
    }

    public async Task<(bool Success, string Message)> VerifyHospitalAsync(int hospitalId, string? notes)
    {
        try
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.Id == hospitalId);
            if (hospital == null)
            {
                return (false, "Hospital not found");
            }

            if (hospital.IsVerified)
            {
                return (false, "Hospital is already verified");
            }

            hospital.IsVerified = true;
            hospital.VerifiedAt = TimeHelper.Now;
            hospital.VerificationNotes = notes;
            hospital.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin verified hospital {HospitalId}", hospitalId);
            return (true, "Hospital verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying hospital {HospitalId}", hospitalId);
            return (false, "Failed to verify hospital");
        }
    }

    #endregion
}
