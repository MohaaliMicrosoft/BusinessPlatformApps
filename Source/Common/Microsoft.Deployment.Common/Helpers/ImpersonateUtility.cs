using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;

namespace Microsoft.Deployment.Common.Helpers
{
    public class ImpersonateUtility
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LoadUserProfile(IntPtr hToken, ref PROFILEINFO lpProfileInfo);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool UnloadUserProfile(IntPtr hToken, IntPtr hProfile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROFILEINFO
        {
            public int dwSize;
            public int dwFlags;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpUserName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpProfilePath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDefaultPath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpServerName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpPolicyPath;
            public IntPtr hProfile;
        }

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;

        private const int LOGON32_PROVIDER_DEFAULT = 0;

        public delegate ActionResponse ActionDelegate(ActionRequest request);

        public static ActionResponse Execute(ActionDelegate action, ActionRequest request)
        {
            var domain = NTHelper.CleanDomain(request.Message["ImpersonationDomain"][0].ToString());
            var userName = NTHelper.CleanUsername(request.Message["ImpersonationUsername"][0].ToString());
            var password = request.Message["ImpersonationPassword"][0].ToString();

            string[] userDomain = WindowsIdentity.GetCurrent().Name.Split('\\');
            if (userDomain.Length != 2)
                throw new InvalidOperationException("Current user identity doesn't contain a domain name");

            // Just call the method if it's for the same user
            if (userDomain[0].EqualsIgnoreCase(domain) && userDomain[1].EqualsIgnoreCase(userName))
                return action(request);

            IntPtr logonToken = IntPtr.Zero;
            PROFILEINFO profileInfo = new PROFILEINFO();
            profileInfo.dwSize = Marshal.SizeOf(profileInfo);
            profileInfo.dwFlags = 1; //PI_NOUI
            profileInfo.lpUserName = userName;
            bool loadProfileResult = false;

            try
            {
                bool logonResult = LogonUser(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out logonToken);

                if (!logonResult)
                    return new ActionResponse(ActionStatus.Failure, null, new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()), DefaultErrorCodes.ImpersonationFailed);           

                loadProfileResult = LoadUserProfile(logonToken, ref profileInfo);
                if (!loadProfileResult)
                    return new ActionResponse(ActionStatus.Failure, null, new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()), DefaultErrorCodes.ImpersonationFailed);

                // Use the token handle returned by LogonUser.
                using (WindowsIdentity newWindowsIdentity = new WindowsIdentity(logonToken))
                {
                    using (WindowsImpersonationContext impersonatedUser = newWindowsIdentity.Impersonate())
                    {
                        return  action(request);
                    }
                }
            }
            finally
            {
                if (loadProfileResult)
                    UnloadUserProfile(logonToken, profileInfo.hProfile);

                if (logonToken != IntPtr.Zero)
                    CloseHandle(logonToken);
            }
        }
        
    }
}
