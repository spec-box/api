namespace tms.WebApi.Model;

public class UploadData
{
    public UploadDataEntity[] Entities { get; set; } = null!;
}

public class UploadDataEntity
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
