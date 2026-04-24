using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Back_EndAPI.Entities;

[Keyless]
public partial class OrderLookupL1
{
    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("order_date")]
    public DateOnly? OrderDate { get; set; }

    [Column("shipping_adress")]
    public string? ShippingAdress { get; set; }

    [Column("everything_shipped")]
    public bool? EverythingShipped { get; set; }
}
