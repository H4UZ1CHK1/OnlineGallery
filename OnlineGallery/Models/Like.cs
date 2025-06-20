using System;
using System.Collections.Generic;

namespace OnlineGallery.Models;

public partial class Like
{
    public int LikeId { get; set; }

    public int? UserId { get; set; }

    public int? ImageId { get; set; }

    public DateTime? DateLiked { get; set; }

    public virtual Image? Image { get; set; }

    public virtual User? User { get; set; }
}
