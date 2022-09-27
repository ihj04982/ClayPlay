using System;
using System.Runtime.InteropServices;

namespace Oculus.Avatar2.Experimental
{
    using ovrAvatar2Result = Avatar2.CAPI.ovrAvatar2Result;
    using ovrAvatar2EntityId = Avatar2.CAPI.ovrAvatar2EntityId;
    using ovrAvatar2Transform = Avatar2.CAPI.ovrAvatar2Transform;

#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable IDE1006 // Naming Styles
    public partial class CAPI
    {
        public enum ovrAvatar2EndEffector : Int32
        {
            LeftArm = 0,
            RightArm = 1,
            LeftLeg = 2,
            RightLeg = 3,
        }

        [DllImport(Avatar2.CAPI.LibFile, CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern ovrAvatar2Result ovrAvatar2Entity_SetEndEffectorTargetTransform(
            ovrAvatar2EntityId entityId,
            ovrAvatar2EndEffector endEffector,
            ovrAvatar2Transform* targetTransform); // Declare as pointer to make the parameter nullable.
    }

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1401 // P/Invokes should not be visible
}
