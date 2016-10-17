using System;
using Microsoft.Deployment.Common.ActionModel;

namespace Microsoft.Deployment.Common.Actions
{
    public interface IActionExceptionHandler
    {
        Type ExceptionExpected { get; }

        ActionResponse HandleException(ActionRequest request, Exception exception);
    }
}
