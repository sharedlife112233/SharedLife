namespace SharedLife.Models.DTOs.Hospital;

public class HospitalDashboardDto
{
    // Verification Stats
    public int PendingVerifications { get; set; }
    public int TotalDonorsVerified { get; set; }
    public int VerifiedThisMonth { get; set; }
    
    // Request Stats
    public int PendingRequests { get; set; }
    public int ActiveRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int TotalRequestsProcessed { get; set; }
    
    // Donor Stats by Blood Group
    public Dictionary<string, int> DonorsByBloodGroup { get; set; } = new();
    
    // Recent Activity
    public List<RecentVerificationDto> RecentVerifications { get; set; } = new();
    public List<RecentRequestActivityDto> RecentRequests { get; set; } = new();
}

public class RecentVerificationDto
{
    public int DonorId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
}

public class RecentRequestActivityDto
{
    public int RequestId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string DonationType { get; set; } = string.Empty;
    public string UrgencyLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
