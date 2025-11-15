using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User : ITableEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    public string PasswordHash { get; set; }  

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "Customer";



    // Azure Table Properties - Ignored by EF
    [NotMapped]
    public string PartitionKey { get; set; } = "User";

    [NotMapped]
    public string RowKey { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public DateTimeOffset? Timestamp { get; set; }

    [NotMapped]
    public ETag ETag { get; set; }
}