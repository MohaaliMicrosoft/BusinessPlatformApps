namespace Microsoft.Deployment.Common.Actions
{
    public interface IActionRequestInterceptor
    {
        InterceptorStatus CanIntercept(IAction actionToExecute, ActionRequest request);

        ActionResponse Intercept(IAction actionToExecute, ActionRequest request);
    }
}
