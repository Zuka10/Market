namespace Market.Infrastructure.Constants;

public static class DatabaseConstants
{
    public static class Schemas
    {
        public const string Auth = "auth";
        public const string Market = "market";
    }

    public static class Tables
    {
        // Auth schema tables
        public static class Auth
        {
            public const string Role = "Role";
            public const string User = "User";
            public const string RefreshToken = "RefreshToken";
        }

        // Market schema tables
        public static class Market
        {
            public const string Location = "Location";
            public const string Vendor = "Vendor";
            public const string Category = "Category";
            public const string Product = "Product";
            public const string VendorLocation = "VendorLocation";
            public const string Discount = "Discount";
            public const string Order = "Order";
            public const string OrderDetail = "OrderDetail";
            public const string Procurement = "Procurement";
            public const string ProcurementDetail = "ProcurementDetail";
            public const string Payment = "Payment";
        }
    }
}