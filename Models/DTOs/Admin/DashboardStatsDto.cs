namespace SharedLife.Models.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalDonors { get; set; }
    public int TotalRecipients { get; set; }
    public int TotalDonationRequests { get; set; }
    public int PendingRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int ActiveDonors { get; set; }
    public int VerifiedDonors { get; set; }
    public int CriticalRequests { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewDonorsThisMonth { get; set; }
    public int NewRequestsThisMonth { get; set; }
    
    // Hospital statistics
    public int TotalHospitals { get; set; }
    public int VerifiedHospitals { get; set; }
    public int PendingHospitalVerifications { get; set; }
    
    // Blood group distribution
    public Dictionary<string, int> DonorsByBloodGroup { get; set; } = new();
    public Dictionary<string, int> RequestsByBloodGroup { get; set; } = new();
    
    // Recent activity
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // "UserRegistered", "DonorRegistered", "RequestCreated", etc.
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
