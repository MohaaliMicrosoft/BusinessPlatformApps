using System;

namespace Microsoft.Deployment.Common.Actions
{
    public interface IActionExceptionHandler
    {
        Type ExceptionExpected { get; }

        ActionResponse HandleException(ActionRequest request, Exception exception);
    }
}
