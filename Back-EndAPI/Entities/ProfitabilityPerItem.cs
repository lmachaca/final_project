using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Entities;

[Keyless]
public partial class ProfitabilityPerItem
{
    [Column("item_id")]
    public int? ItemId { get; set; }

    [Column("item_name")]
    [StringLength(100)]
    public string? ItemName { get; set; }

    [Column("qty_sold")]
    public string? QtySold { get; set; }

    [Column("avg_cost_to_purchase")]
    public string? AvgCostToPurchase { get; set; }

    [Column("avg_revenue_per_sale")]
    public string? AvgRevenuePerSale { get; set; }

    [Column("profit_per_unit")]
    public string? ProfitPerUnit { get; set; }
}
