using System;
using System.Collections.Generic;

namespace SmartFinanceManager.Services
{
    public class PredictionResult
    {
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsConfident { get; set; }
    }

    public static class SmartCategorizationService
    {
        public static PredictionResult Predict(string description, bool isOffice)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return new PredictionResult { IsConfident = false };
            }

            string text = description.ToLowerInvariant();

            if (isOffice)
            {
                return PredictOffice(text);
            }
            else
            {
                return PredictPersonal(text);
            }
        }

        private static PredictionResult PredictPersonal(string text)
        {
            // Income matches
            if (ContainsAny(text, "salary", "paycheck", "wage", "direct deposit", "employer", "stipend"))
                return new PredictionResult { Type = "Income", Category = "Salary & Wages", IsConfident = true };
            if (ContainsAny(text, "bonus", "reward", "incentive", "gift received"))
                return new PredictionResult { Type = "Income", Category = "Bonus", IsConfident = true };
            if (ContainsAny(text, "freelance", "side gig", "upwork", "fiverr", "tutoring", "consulting"))
                return new PredictionResult { Type = "Income", Category = "Freelance & Consulting", IsConfident = true };
            if (ContainsAny(text, "dividend", "yield", "capital gain", "stock sale", "crypto gain"))
                return new PredictionResult { Type = "Income", Category = "Investment Returns", IsConfident = true };

            // Transfer matches
            if (ContainsAny(text, "transfer", "savings", "vanguard", "fidelity", "brokerage", "crypto", "roth", "401k", "deposit to savings"))
                return new PredictionResult { Type = "Transfer", Category = "Investments", IsConfident = true };

            // Expense matches
            if (ContainsAny(text, "starbucks", "coffee", "cafe", "lunch", "dinner", "breakfast", "burger", "pizza", "restaurant", "grocer", "supermarket", "walmart", "whole foods", "trader joe", "food", "dining", "snack", "bakery"))
                return new PredictionResult { Type = "Expense", Category = "Food & Dining", IsConfident = true };
            if (ContainsAny(text, "uber", "lyft", "taxi", "gas", "fuel", "shell", "chevron", "bp", "parking", "toll", "subway", "metro", "bus", "train", "flight", "airline"))
                return new PredictionResult { Type = "Expense", Category = "Transportation", IsConfident = true };
            if (ContainsAny(text, "rent", "lease", "apartment", "mortgage", "hoa", "landlord", "housing"))
                return new PredictionResult { Type = "Expense", Category = "Housing", IsConfident = true };
            if (ContainsAny(text, "amazon", "ebay", "clothes", "clothing", "shoes", "mall", "store", "target", "ikea", "zara", "h&m", "shopping"))
                return new PredictionResult { Type = "Expense", Category = "Shopping", IsConfident = true };
            if (ContainsAny(text, "netflix", "spotify", "hulu", "disney", "youtube", "movie", "cinema", "theatre", "game", "steam", "playstation", "xbox", "concert", "entertainment"))
                return new PredictionResult { Type = "Expense", Category = "Entertainment", IsConfident = true };
            if (ContainsAny(text, "phone", "mobile", "verizon", "at&t", "t-mobile", "vodafone", "internet", "wifi", "fiber", "telecom"))
                return new PredictionResult { Type = "Expense", Category = "Phone & Internet", IsConfident = true };
            if (ContainsAny(text, "electric", "power", "water", "gas bill", "utility", "trash", "sewer"))
                return new PredictionResult { Type = "Expense", Category = "Utilities", IsConfident = true };
            if (ContainsAny(text, "doctor", "dentist", "pharmacy", "medicine", "pill", "hospital", "clinic", "health", "dental", "vision", "therapy"))
                return new PredictionResult { Type = "Expense", Category = "Health & Medical", IsConfident = true };
            if (ContainsAny(text, "gym", "fitness", "workout", "crossfit", "yoga", "sport", "tennis"))
                return new PredictionResult { Type = "Expense", Category = "Sports & Fitness", IsConfident = true };
            if (ContainsAny(text, "tuition", "course", "udemy", "coursera", "book", "school", "university", "training"))
                return new PredictionResult { Type = "Expense", Category = "Education", IsConfident = true };
            if (ContainsAny(text, "hotel", "airbnb", "vacation", "trip", "resort", "flight ticket", "travel"))
                return new PredictionResult { Type = "Expense", Category = "Travel", IsConfident = true };

            return new PredictionResult { IsConfident = false };
        }

        private static PredictionResult PredictOffice(string text)
        {
            // Corporate Income
            if (ContainsAny(text, "sales", "license", "licensing", "product sale", "customer payment", "invoice paid", "saas subscription revenue"))
                return new PredictionResult { Type = "Income", Category = "Client Sales", IsConfident = true };
            if (ContainsAny(text, "retainer", "consulting", "advisory", "professional services", "implementation fee"))
                return new PredictionResult { Type = "Income", Category = "Consulting Fees", IsConfident = true };
            if (ContainsAny(text, "service revenue", "support contract", "sla", "maintenance fee"))
                return new PredictionResult { Type = "Income", Category = "Service Revenue", IsConfident = true };
            if (ContainsAny(text, "contract", "enterprise deal", "tender", "b2b"))
                return new PredictionResult { Type = "Income", Category = "Enterprise Contracts", IsConfident = true };

            // Corporate Transfers
            if (ContainsAny(text, "capital", "wire", "reserve", "treasury", "intercompany", "dividend paid", "sweep"))
                return new PredictionResult { Type = "Transfer", Category = "Treasury", IsConfident = true };

            // Corporate Expenses
            if (ContainsAny(text, "payroll", "salary", "salaries", "wages", "staff", "employee", "team compensation", "contractor payout"))
                return new PredictionResult { Type = "Expense", Category = "Staff Salaries & Payroll", IsConfident = true };
            if (ContainsAny(text, "office rent", "lease", "building rent", "wework", "co-working", "commercial space", "facility"))
                return new PredictionResult { Type = "Expense", Category = "Office Rent & Lease", IsConfident = true };
            if (ContainsAny(text, "aws", "azure", "google cloud", "cloud", "saas", "github", "jira", "slack", "zoom", "microsoft 365", "software", "hosting", "domain"))
                return new PredictionResult { Type = "Expense", Category = "Software & SaaS", IsConfident = true };
            if (ContainsAny(text, "marketing", "google ads", "meta ads", "facebook ads", "linkedin ads", "advertising", "seo", "branding", "pr", "campaign"))
                return new PredictionResult { Type = "Expense", Category = "Marketing & Ads", IsConfident = true };
            if (ContainsAny(text, "fiber", "internet", "power", "electricity", "water", "utilities", "telecom", "isp"))
                return new PredictionResult { Type = "Expense", Category = "Utilities & Fiber", IsConfident = true };
            if (ContainsAny(text, "server", "laptop", "macbook", "monitor", "hardware", "cisco", "workstation", "equipment"))
                return new PredictionResult { Type = "Expense", Category = "Hardware & Servers", IsConfident = true };
            if (ContainsAny(text, "tax", "vat", "irs", "legal", "lawyer", "audit", "compliance", "cpa", "attorney"))
                return new PredictionResult { Type = "Expense", Category = "Tax & Legal Fees", IsConfident = true };
            if (ContainsAny(text, "cleaning", "repair", "maintenance", "hvac", "renovation"))
                return new PredictionResult { Type = "Expense", Category = "Office Maintenance", IsConfident = true };
            if (ContainsAny(text, "coffee", "pantry", "snacks", "water cooler", "office supplies", "stationery", "paper"))
                return new PredictionResult { Type = "Expense", Category = "Pantry & Supplies", IsConfident = true };
            if (ContainsAny(text, "client dinner", "catering", "hospitality", "event", "summit", "conference ticket"))
                return new PredictionResult { Type = "Expense", Category = "Client Hospitality", IsConfident = true };

            return new PredictionResult { IsConfident = false };
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            foreach (var kw in keywords)
            {
                if (text.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
