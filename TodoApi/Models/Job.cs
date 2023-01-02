using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

public class Job
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}