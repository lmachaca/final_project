using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.DTOs
{

    public class InventoryResponseDto
    {
        public List<InventoryItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalQuantityAvailable { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class InventoryItemDto
    {
        public int SkuNumber { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalDeposited { get; set; }
        public int TotalWithdrawn { get; set; }
        public int QuantityAvailable { get; set; }
        public List<StorageLocationDto> StorageLocations { get; set; } = new();
    }
    public class StorageLocationDto
    {
        public int BinId { get; set; }
        public string BinLocation { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
