using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Entities;

[Keyless]
public partial class InventoryOnHand
{
    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("item_name")]
    [StringLength(100)]
    public string? ItemName { get; set; }

    [Column("qty_in_warehouse")]
    public long? QtyInWarehouse { get; set; }

    [Column("qty_sold_not_yet_shipped")]
    public long? QtySoldNotYetShipped { get; set; }

    [Column("qty_available_for_sale")]
    public long? QtyAvailableForSale { get; set; }
}
