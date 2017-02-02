using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class CancellationTokenEx
    {
        public static void CancellableSleep(this System.Threading.CancellationToken self, int millisecond)
        {
            if (self == null)
                throw new ArgumentNullException();
            if (self.WaitHandle.WaitOne(millisecond))
                throw new OperationCanceledException();
        }
    }
}
