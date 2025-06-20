using System;
using System.Collections.Generic;

namespace OnlineGallery.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int? UserId { get; set; }

    public int? ImageId { get; set; }

    public string? CommentText { get; set; }

    public DateTime? DateCreated { get; set; }

    public virtual Image? Image { get; set; }

    public virtual User? User { get; set; }
}
