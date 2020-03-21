using System.Collections.Generic;

namespace PaymentGateway.Domain.Metrics
{
    public class MetricsTimeData
    {
        public static string TimePaymentsReceived = "time_payments_received";
        public static string TimePaymentsRetrieved = "time_payments_retrieved";

        public static Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            { TimePaymentsReceived, "Histogram for the duration in seconds" },
            { TimePaymentsRetrieved, "Histogram for the duration in seconds"}
        };
    }
}
