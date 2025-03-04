﻿using System;
using System.Collections.Generic;

namespace EntityFramework.Models;

public partial class Hotel
{
    public decimal HotelId { get; set; }

    public string? HotelName { get; set; }

    public string? CityName { get; set; }

    public string? HotelGrade { get; set; }

    public string? HotelLocation { get; set; }

    public string? HotelIntroduction { get; set; }

    public virtual City? CityNameNavigation { get; set; }

    public virtual ICollection<HotelRoomType> HotelRoomTypes { get; set; } = new List<HotelRoomType>();

    public virtual ICollection<TourGroup> Groups { get; set; } = new List<TourGroup>();
}
