using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FakeHubApi.Model.Entity;

public class Repository
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public RepositoryOwnedBy OwnedBy { get; set; }
    
    public int OwnerId { get; set; } //this could be id of either user or organization hence the owned by type

    public Badge Badge { get; set; }
}