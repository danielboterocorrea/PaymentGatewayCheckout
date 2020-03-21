using System.Collections.Generic;

namespace PaymentGateway.Domain.Metrics
{
    public class MetricsCountsData
    {
        public static string PaymentsReceived = "count_payments_received";
        public static string PaymentsRetrieved = "count_payments_retrieved";
        public static string PaymentsReceivedErrors = "count_payments_received_errors";

        public static Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            { PaymentsReceived, "The total number of payments received" },
            { PaymentsRetrieved, "The total number of payments retrieved"},
            { PaymentsReceivedErrors, "The total number of payments received with bad format"}
        };
    }
}
