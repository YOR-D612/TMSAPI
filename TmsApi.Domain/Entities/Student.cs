using TmsApi.Domain.Entities;

namespace TmsApi.Domain.Entities;
public class Student
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public double GPA { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public uint Version { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();
}