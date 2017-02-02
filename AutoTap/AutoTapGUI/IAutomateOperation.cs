using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTapGUI
{
    public abstract class IAutomateOperation
    {
        public virtual Task DoAutomateTaskAsync(CancellationToken token)
        {
            return Task.Run(() => DoAutomate(token), token);
        }
        public virtual void DoAutomate(CancellationToken token)
        {
        }
    }
}
