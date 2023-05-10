using System;
using System.Collections.Generic;

namespace SchoolManagementApp.MVC.Data;

public partial class Class
{
    public int Id { get; set; }

    public int? LectureId { get; set; }

    public int? CourseId { get; set; }

    public TimeSpan? Time { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Lecturer? Lecture { get; set; }
}
