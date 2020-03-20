using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Toolbox
{
    internal class WorkItem
    {
        public readonly TaskCompletionSource<object> TaskSource;
        public readonly Action Action;
        public readonly CancellationToken? CancelToken;
        private int NumberOfFails;

        public WorkItem(
          TaskCompletionSource<object> taskSource,
          Action action,
          CancellationToken? cancelToken)
        {
            TaskSource = taskSource;
            Action = action;
            CancelToken = cancelToken;
            NumberOfFails = 0;
        }

        public void IncreaseNumberOfFails()
        {
            NumberOfFails++;
        }

        public bool CanBeEnqueuedAgain()
        {
            return NumberOfFails < 3;
        }
    }
}
