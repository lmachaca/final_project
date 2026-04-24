using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Entities;

[Keyless]
public partial class OrderLookupL2
{
    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("order_date")]
    public DateOnly? OrderDate { get; set; }

    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("qty_ordered")]
    public long? QtyOrdered { get; set; }

    [Column("qty_already_shipped")]
    public long? QtyAlreadyShipped { get; set; }

    [Column("qty_remaining_to_be_shipped")]
    public long? QtyRemainingToBeShipped { get; set; }

    [Column("qty_in_wharehouse")]
    public long? QtyInWharehouse { get; set; }
}
