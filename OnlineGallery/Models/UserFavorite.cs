using System;
using System.Collections.Generic;

namespace OnlineGallery.Models;

public partial class UserFavorite
{
    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int ImageId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }
}
