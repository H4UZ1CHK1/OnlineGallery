using System;
using System.Collections.Generic;

namespace OnlineGallery.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public DateTime? DateCreated { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
