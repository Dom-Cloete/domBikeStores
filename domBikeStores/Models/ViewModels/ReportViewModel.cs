using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using domBikeStores.Models;

namespace domBikeStores.ViewModels
{
    public class ReportViewModel
    {
        public string ReportTitle { get; set; }
        public IEnumerable<ProductSalesReportItem> TopProducts { get; set; }
        public IEnumerable<StorePerformanceReportItem> TopStores { get; set; }
        public string ChartTypeProducts { get; set; }
        public string ChartTypeStores { get; set; }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Description { get; set; }
        public string EditedDescription { get; set; }
        public List<SavedReport> DocumentArchive { get; set; }

        public class SavedReport
        {
            public string FileName { get; set; }
            public string FileType { get; set; }
            public string Description { get; set; }
            public DateTime DateSaved { get; set; }
        }
    }

    public class ProductSalesReportItem
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
    }

    public class StorePerformanceReportItem
    {
        public string StoreName { get; set; }
        public int TotalProductsSold { get; set; }
    }
}