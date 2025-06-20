using System;
using System.Collections.Generic;

namespace OnlineGallery.Models;

public partial class Favorite
{
    public int FavoriteId { get; set; }

    public int? UserId { get; set; }

    public int? ImageId { get; set; }

    public DateTime? DateAdded { get; set; }

    public virtual Image? Image { get; set; }

    public virtual User? User { get; set; }
}
