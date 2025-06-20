using System;
using System.Collections.Generic;

namespace OnlineGallery.Models;

public partial class Image
{

    public int ImageId { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ModerationComment { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? FilePath { get; set; }
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    public int? UserId { get; set; }

    public DateTime? DateUploaded { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Transactions> Transactions { get; set; } = new List<Transactions>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual User? User { get; set; }
}
