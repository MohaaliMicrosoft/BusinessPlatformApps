namespace Microsoft.Deployment.Common.Actions
{
    public interface IAction
    {
        string OperationUniqueName { get; }

        ActionResponse ExecuteAction(ActionRequest request);
    }
}
