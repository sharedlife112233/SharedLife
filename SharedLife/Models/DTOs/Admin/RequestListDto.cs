namespace SharedLife.Models.DTOs.Admin;

public class RequestListDto
{
    public int Id { get; set; }
    public int RecipientId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public string DonationType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string UrgencyLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? HospitalName { get; set; }
    public string? City { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime? RequiredDateTime { get; set; }
    public int MatchedDonorsCount { get; set; }
    public int AcceptedDonorsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class RequestListResponseDto
{
    public List<RequestListDto> Requests { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UpdateRequestStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
