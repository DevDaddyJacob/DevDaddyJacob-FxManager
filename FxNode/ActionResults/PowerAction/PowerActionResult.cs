using DevDaddyJacob.FxManager.Shared.ActionResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDaddyJacob.FxManager.Node.ActionResults.PowerAction
{
    internal abstract class PowerActionResult : ActionResult
    {
        public PowerActionResult(int code, string message) : base(code, message) { }
    }
}
