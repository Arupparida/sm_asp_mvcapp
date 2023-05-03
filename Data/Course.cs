﻿using System;
using System.Collections.Generic;

namespace SchoolManagementApp.MVC.Data;

public partial class Course
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string? Code { get; set; }
}
