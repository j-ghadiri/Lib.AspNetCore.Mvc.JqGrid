using System.Threading.Tasks;

namespace Lib.AspNetCore.Mvc.JqGrid.Core.Helpers
{
    internal static class CompatibilityHelper
    {
        internal static Task CompletedTask
        {
            get
            {
                return Task.CompletedTask;
            }
        }
    }
}
