using System;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.DatabaseActions
{
    [Export(typeof(IActionExceptionHandler))]
    public class SqlExceptionHandler : IActionExceptionHandler
    {
        public Type ExceptionExpected { get; } = typeof(SqlException);

        public ActionResponse HandleException(ActionRequest request,  Exception exception)
        {
            SqlException sqlException = exception as SqlException;
            if (sqlException != null)
                switch (sqlException.Number)
                {
                    case 18456:
                        return new ActionResponse(ActionStatus.FailureExpected, JsonUtility.GetEmptyJObject(), exception, SqlErrorCodes.LoginFailed);
                }

            return new ActionResponse(ActionStatus.UnhandledException);
        }
    }
}
