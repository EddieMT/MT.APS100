using System;

namespace MT.APS100.Model
{
    public class ProductRevision
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Revision { get; set; }
        public DateTime ProductDate { get; set; }
        public string ProductComment { get; set; }
    }
}