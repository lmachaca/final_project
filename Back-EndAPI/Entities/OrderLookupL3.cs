using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Entities;

[Keyless]
public partial class OrderLookupL3
{
    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("order_date")]
    public DateOnly? OrderDate { get; set; }

    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("qty_ordered")]
    public string? QtyOrdered { get; set; }

    [Column("box_id")]
    public int? BoxId { get; set; }

    [Column("box_tracking_id")]
    public int? BoxTrackingId { get; set; }

    [Column("qty_in_box")]
    public string? QtyInBox { get; set; }

    [Column("date_shipped")]
    public DateOnly? DateShipped { get; set; }
}
