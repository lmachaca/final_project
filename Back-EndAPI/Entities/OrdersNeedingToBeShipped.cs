using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Entities;

[Keyless]
public partial class OrdersNeedingToBeShipped
{
    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("order_date")]
    public DateOnly? OrderDate { get; set; }

    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("qty_ordered")]
    public string? QtyOrdered { get; set; }

    [Column("qty_already_shipped")]
    public string? QtyAlreadyShipped { get; set; }

    [Column("qty_remaining_to_be_shipped")]
    public string? QtyRemainingToBeShipped { get; set; }
}
