using System;
using System.Runtime.InteropServices;

namespace DJI.OnBoardSDK
{


    public class FlightData : NativeObject
    {
        public byte flag;
        public float x;
        public float y;
        public float z;
        public float yaw;

        public override int TypeSize
        {
            get { return 17; }
        }


        public override unsafe void ReadFrom(Ptr p, int size)
        {
            flag = (p + 0).Byte;
            x = (p + 1).Single;
            y = (p + 5).Single;
            z = (p + 9).Single;
            yaw = (p + 13).Single;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetByte(flag);
            (p + 1).SetSingle(x);
            (p + 5).SetSingle(y);
            (p + 9).SetSingle(z);
            (p + 13).SetSingle(yaw);
        }
    }

    public partial class Flight
    {

    }

    public enum FlightTask
    {
        TASK_GOHOME = 1,
        TASK_TAKEOFF = 4,
        TASK_LANDING = 6
    };

    public enum FlightVerticalLogic
    {
        VERTICAL_VELOCITY = 0x00,
        VERTICAL_POSITION = 0x10,
        VERTICAL_THRUST = 0x20,
    };

    public enum FlightHorizontalLogic
    {
        HORIZONTAL_ANGLE = 0x00,
        HORIZONTAL_VELOCITY = 0x40,
        HORIZONTAL_POSITION = 0X80,
    };

    public enum FlightYawLogic
    {
        YAW_ANGLE = 0x00,
        YAW_RATE = 0x08
    };

    public enum HFlightorizontalCoordinate
    {
        HORIZONTAL_GROUND = 0x00,
        HORIZONTAL_BODY = 0x02
    };

    //! @version 2.3
    public enum FlightYawCoordinate
    {
        YAW_GROUND = 0x00,
        YAW_BODY = 0X01
    };
    //! @version 3.1
    public enum FlightSmoothMode
    {
        SMOOTH_DISABLE = 0x00,
        SMOOTH_ENABLE = 0x01
    };

    public enum FlightStatus
    {
        STATUS_MOTOR_OFF = 0,
        STATUS_GROUND_STANDBY = 1,
        STATUS_TAKE_OFF = 2,
        STATUS_SKY_STANDBY = 3,
        STATUS_LANDING = 4,
        STATUS_FINISHING_LANDING = 5,
    };


    public enum FlightDevice
    {
        DEVICE_RC = 0,
        DEVICE_APP = 1,
        DEVICE_SDK = 2,
    };

    //! @todo rename
    public enum FlightMode
    {
        ATTI_STOP = 0,
        HORIZ_ANG_VERT_VEL_YAW_ANG = 1,
        HORIZ_ANG_VERT_VEL_YAW_RATE = 2,
        HORIZ_VEL_VERT_VEL_YAW_ANG = 3,
        HORIZ_VEL_VERT_VEL_YAW_RATE = 4,
        HORIZ_POS_VERT_VEL_YAW_ANG = 5,
        HORIZ_POS_VERT_VEL_YAW_RATE = 6,
        HORIZ_ANG_VERT_POS_YAW_ANG = 7,
        HORIZ_ANG_VERT_POS_YAW_RATE = 8,
        HORIZ_VEL_VERT_POS_YAW_ANG = 9,
        HORIZ_VEL_VERT_POS_YAW_RATE = 10,
        HORIZ_POS_VERT_POS_YAW_ANG = 11,
        HORIZ_POS_VERT_POS_YAW_RATE = 12,
        HORIZ_ANG_VERT_THR_YAW_ANG = 13,
        HORIZ_ANG_VERT_THR_YAW_RATE = 14,
        HORIZ_VEL_VERT_THR_YAW_ANG = 15,
        HORIZ_VEL_VERT_THR_YAW_RATE = 16,
        HORIZ_POS_VERT_THR_YAW_ANG = 17,
        HORIZ_POS_VERT_THR_YAW_RATE = 18,
        GPS_ATII_CTRL_CL_YAW_RATE = 97,
        GPS_ATTI_CTRL_YAW_RATE = 98,
        ATTI_CTRL_YAW_RATE = 99,
        ATTI_CTRL_STOP = 100,
        MODE_NOT_SUPPORTED = 0xFF
    };
}