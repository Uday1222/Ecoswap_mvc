using System;

namespace EcoSwap.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public bool IsGiveaway { get; set; }
        public decimal? Price { get; set; }
        public DateTime DatePosted { get; set; }
        public int? UserId { get; set; } // Link to user who posted the item
    }
}