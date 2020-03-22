using PaymentGateway.Domain.Toolbox;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Toolbox
{
    internal class WorkItem<T, R> where T : IGetId
    {
        public readonly Task<R> Task;
        public readonly T item;

        public WorkItem(Task<R> task, T request)
        {
            Task = task;
            item = request;
        }
    }
}
