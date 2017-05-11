/*! @file DJI_Type.h
 *  @version 3.1.9
 *  @date November 10, 2016
 *
 *  @brief
 *  Type definition for DJI onboardSDK library.
 *  Officially Maintained
 *  
 *  @copyright
 *  Copyright 2016 DJI. All rights reserved.
 * */

/*! @attention
 *  Do not modify any definition in this file
 *  if you are unsure about what are you doing.
 *  DJI will not provide any support for changes made to this file.
 * */

using System;
using System.Runtime.InteropServices;


namespace DJI.OnBoardSDK
{
    partial class CoreAPI
    {
        ////! Define the UNUSED macro to suppress compiler warnings about unused arguments 
        //#ifdef __GNUC__
        //#define __UNUSED __attribute__((__unused__))
        //#define __DELETE(x) delete (char *) x
        //#else
        //#define __UNUSED
        //#define __DELETE(x) delete x


        ////! @todo fix warning.
        //#ifndef STM32
        //#pragma warning(disable : 4100)
        //#pragma warning(disable : 4800)
        //#pragma warning(disable : 4996)
        //#pragma warning(disable : 4244)
        //#pragma warning(disable : 4267)
        //#pragma warning(disable : 4700)
        //#pragma warning(disable : 4101)
        //#endif // STM32
        //#endif //__GNUC__

        //#ifdef WIN32
        //#define __func__ __FUNCTION__
        //#endif // WIN32




        //! This is the default status printing mechanism
        //#define API_LOG(driver, title, fmt, ...)                                  \
        //  if ((title))                                                            \
        //  {                                                                       \
        //    int len = (sprintf(DJI::onboardSDK::buffer, "%s %s,line %d: " fmt,    \
        //        (title) ? (title) : "NONE", __func__, __LINE__, ##__VA_ARGS__));  \
        //    if ((len != -1) && (len < 1024))                                      \
        //      (driver)->displayLog();                                             \
        //    else                                                                  \
        //      (driver)->displayLog(Thread.CurrentThread.ManagedThreadId + " : ERROR: log printer inner fault\n");           \
#if !NETMF
        public static void API_LOG(IPlatformDriver serialDevice, string level, string format, params object[] args)
        {
            serialDevice.displayLog(string.Format(format, args));
        }
#else
        public static void API_LOG(IPlatformDriver serialDevice, string level, string format, object a0)
        {
            serialDevice.displayLog(format, a0);
        }

        public static void API_LOG(IPlatformDriver serialDevice, string level, string format, object a0, object a1)
        {
            serialDevice.displayLog(format, a0, a1);
        }

        public static void API_LOG(IPlatformDriver serialDevice, string level, string format, object a0, object a1, object a2)
        {
            serialDevice.displayLog(format, a0, a1, a2);
        }
        public static void API_LOG(IPlatformDriver serialDevice, string level, string format, object a0, object a1, object a2, object a3)
        {
            serialDevice.displayLog(format, a0, a1, a2, a3);
        }

        public static void API_LOG(IPlatformDriver serialDevice, string level, string format, params object[] args)
        {
            serialDevice.displayLog(format, args);
        }


        private static object[] _zeroParam = new object[0];
        public static void API_LOG(IPlatformDriver serialDevice, string level, string format)
        {

            serialDevice.displayLog(format, _zeroParam);
        }
#endif

        //#ifdef API_TRACE_DATA
        //#define TRACE_LOG "TRACE"
        //#else
        //#define TRACE_LOG 0
        //#endif

        //#ifdef API_DEBUG_DATA
        //#define DEBUG_LOG "DEBUG"
        //#else
        //#define DEBUG_LOG 0
        //#endif

        //#ifdef API_ERROR_DATA
        //#define ERROR_LOG "ERROR"
        //#else
        //#define ERROR_LOG 0
        //#endif

        //#ifdef API_BUFFER_DATA
        //#define BUFFER_LOG "BUFFER"
        //#else
        //#define BUFFER_LOG 0
        //#endif

        //#ifdef API_STATUS_DATA
        //#define STATUS_LOG "STATUS"
        //#else
        //#define STATUS_LOG 0
        //#endif

        //#ifdef API_MISSION_DATA
        //#define MISSION_LOG "MISSION"
        //#else
        //#define MISSION_LOG 0
        //#endif

        //#ifdef API_RTK_DEBUG
        //#define RTK_LOG "MISSION"
        //#else
        //#define RTK_LOG 0
        //#endif


        public const string TRACE_LOG = "TRACE";
        public const string DEBUG_LOG = "DEBUG";
        public const string ERROR_LOG = "ERROR";
        public const string BUFFER_LOG = "BUFFER";
        public const string STATUS_LOG = "STATUS";
        public const string MISSION_LOG = "MISSION";
        public const string RTK_LOG = "MISSION";


        ////! @note for ARMCC-5.0 compiler
        //#ifdef ARMCC
        //#pragma anon_unions
        //#endif
    }

    partial class CoreAPI
    {

        //const size_t bufsize = 1024;
        //extern char buffer[];
        //extern uint8_t encrypt;

        public const int SESSION_TABLE_NUM = 32;
        public const int CALLBACK_LIST_NUM = 10;

        /**
		 * @note size is in Bytes
		 */
        public const int MAX_ACK_SIZE = 107;

    }

    ////! The CoreAPI class definition is detailed in DJI_API.h 
    //class CoreAPI;


    //! The Header struct is meant to handle the open protocol header.
    public struct Header
    {
        //unsigned int sof : 8;

        public byte sof
        {
            get { return Ptr.Byte; }
            set { Ptr.SetByte(value); }
        }

        //unsigned int length : 10;
        //unsigned int version : 6;
        public int length
        {
            get
            {
                return (byte)((Ptr + 1).Byte | (((Ptr + 2).Byte & 0x3) << 8));
            }
            set
            {
                var original = (byte)((Ptr + 2).Byte & 0xFC);

                (Ptr + 1).SetByte((byte)(value & 0xFF));
                (Ptr + 2).SetByte((byte)(((value >> 8) & 0x3) | original));
            }
        }
        public byte version
        {
            get
            {
                return (byte)((Ptr + 2).Byte >> 2);
            }
            set
            {
                var original = (byte)((Ptr + 2).Byte & 0x3);
                (Ptr + 2).SetByte((byte)((value & 0x3F << 2) | original));
            }
        }

        //unsigned int sessionID : 5;
        //unsigned int isAck : 1;
        ////! @warning this field will change reversed0 --> reserved0 in the next release 
        //unsigned int reversed0 : 2; // always 0
        public byte sessionID// : 5;
        {
            get { return (byte)((Ptr + 3).Byte & 0x1F); }
            set
            {
                var original = (byte)((Ptr + 3).Byte & 0xE0);

                (Ptr + 3).SetByte((byte)((value & 0x1F) | original & 0xE0));
            }
        }
        public bool isAck//: 1;
        {
            get
            {
                return (((Ptr + 3).Byte >> 5) & 0x1) == 1;
            }
            set
            {
                var original = (byte)((Ptr + 3).Byte & 0xDF);
                (Ptr + 3).SetByte((byte)((value ? 1 : 0 << 5) | original));
            }
        }

        public byte reversed0//: 2; // always 0
        {
            get
            {
                return (byte)(((Ptr + 3).Byte >> 6) & 0x3);
            }
        }


        //unsigned int padding : 5;
        //unsigned int enc : 3;
        public byte padding
        {
            get { return (byte)((Ptr + 4).Byte & 0x1F); }
            set
            {
                var original = (byte)((Ptr + 4).Byte & 0xE0);

                (Ptr + 4).SetByte((byte)((value & 0x1F) | original));
            }
        }

        public bool enc
        {
            get { return (byte)((Ptr + 4).Byte >> 5) != 0; }
            set
            {
                var original = (byte)((Ptr + 4).Byte & 0x1F);

                (Ptr + 4).SetByte((byte)((value ? (1 << 5) : 0) | original));
            }
        }


        ////! @warning this field will change reversed1 --> reserved1 in the next release 
        //unsigned int reversed1 : 24;
        public uint reversed1
        {
            get { return (Ptr + 4).UInt32 >> 8; }
        }


        //unsigned int sequenceNumber : 16;
        public ushort sequenceNumber
        {
            get { return (Ptr + 8).UInt16; }
            set
            {
                (Ptr + 8).SetByte((byte)(value & 0xFF));
                (Ptr + 9).SetByte((byte)((value >> 8) & 0xFF));
            }
        }


        //unsigned int crc : 16;
        public ushort crc
        {
            get { return (Ptr + 10).UInt16; }
            set
            {
                (Ptr + 10).SetByte((byte)(value & 0xFF));
                (Ptr + 11).SetByte((byte)((value >> 8) & 0xFF));
            }
        }

        private Header(Ptr p)
        {
            _ptr = p;
        }

        public static explicit operator Header(Ptr p)
        {
            return new Header(p);
        }

        public static explicit operator Header(byte[] buffer)
        {
            return (Header)((Ptr)buffer);
        }


        private Ptr _ptr;
        public Ptr Ptr
        {
            get { return _ptr; }
        }
    }

    //! The CallBack function pointer is used as an argument in api->send calls
    public delegate void CallBack(Ptr h, UserData d);

    //! The CallBackHandler struct allows users to encapsulate callbacks and data in one struct 
    public struct CallBackHandler
    {
        public CallBack callback;
        public UserData userData;
    } // CallBackHandler;

    public class UserData
    {
        object Data;
    }




    public struct Command
    {
        public ushort sessionMode;//: 2;
        public bool encrypt;//: 1;
        public int retry;//: 13;

        public int timeout; // unit is ms

        public int length;
        public Ptr buf;
        public CallBack handler;
        public UserData userData;
    }

    //! @warning this struct will be renamed in a future release.
    public class SDKFilter
    {
        public int reuseIndex;
        public int reuseCount;
        public int recvIndex;
        public byte[] recvBuf = new byte[CoreAPI.BUFFER_SIZE];
        // for encrypt
        public byte[] sdkKey = new byte[32];
        public byte encode;
    }

    //! @warning this struct will be renamed in a future release.
    public class MMU_Tab
    {

        public int tabIndex;//: 8;
        public bool usageFlag;// : 8;
        public int memSize;//: 16;

        public Ptr pmem;
    }

    public class CMDSession
    {
        public byte sessionID;//: 5;
        public bool usageFlag;// : 1;
        public int sent;//: 5;
        public int retry;// : 5;

        public int timeout;//: 16;

        public MMU_Tab mmu;
        public CallBack handler;
        public UserData userData;
        public uint preSeqNum;
        public long preTimestamp;
    } // CMDSession;

    public class ACKSession
    {

        public byte sessionID;//: 5;
        public byte sessionStatus;//: 2;
                                  //uint res : 25;

        public MMU_Tab mmu;
    }

    public struct Ack
    {

        public byte sessionID;//: 8;
        public bool encrypt;// : 8;

        public ushort seqNum;
        public int length;
        public Ptr buf;
    }

    //#pragma pack(1)

    public class BatteryData : NativeObject
    {
        byte _data;
#if !NETMF
        public override string ToString()
        {
            return $"{ _data}";
        }
#endif

        public override int TypeSize
        {
            get { return 1; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            _data = p.Byte;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            p.SetByte(_data);
        }
    }

    /**
     * Gimbal Data
     */
    public class GimbalAngleData : NativeObject
    {
        public short yaw;
        public short roll;
        public short pitch;
        public byte mode;
        public byte duration; // Optional

#if !NETMF
        public override string ToString()
        {
            return base.ToString();
        }
#endif

        public override int TypeSize
        {
            get { return 8; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            yaw = (p + 0).Int16;
            roll = (p + 2).Int16;
            pitch = (p + 4).Int16;
            mode = (p + 6).Byte;

            if (size > 7)
                duration = (p + 7).Byte;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            throw new NotImplementedException();
        }
    } // GimbalAngleData;

    public class GimbalSpeedData : NativeObject
    {
        public short yaw;
        public short roll;
        public short pitch;
        public byte reserved; // always 0x80;

        public override int TypeSize
        {
            get
            {
                return 7;
            }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            yaw = (p + 0).Int16;
            roll = (p + 2).Int16;
            pitch = (p + 4).Int16;
            reserved = (p + 6).Byte;

        }

        public override unsafe void WriteTo(Ptr p)
        {
            throw new NotImplementedException();
        }
    } // GimbalSpeedData;


    // typedef float float32_t;
    // typedef double float64_t;

    /**
     * HotPoint Data
     */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HotPointData
    {
        public byte version;

        public double latitude;
        public double longitude;
        public double height;

        public double radius;
        public float yawRate; // degree

        public byte clockwise;
        public byte startPoint;
        public byte yawMode;
        //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 11)]
        byte[] reserved;
    }
    // HotPointData;

    /**
     * WayPoint Data
     */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WayPointInitData
    {
        public byte indexNumber;
        public float maxVelocity;
        public float idleVelocity;

        public byte finishAction;
        public byte executiveTimes;
        public byte yawMode;
        public byte traceMode;
        public byte RCLostAction;
        public byte gimbalPitch;
        public double latitude;  //! @note For Camera to recording
        public double longitude; //! not supported yet
        public float altitude;

        //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        public byte[] reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WayPointData
    {
        public byte index;

        public double latitude;
        public double longitude;
        public float altitude;
        public float damping;

        public short yaw;
        public short gimbalPitch;
        public byte turnMode;

        //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public byte[] reserved;
        public byte hasAction;
        public ushort actionTimeLimit;

        public byte _data;
        //byte actionNumber : 4;
        //byte actionRepeat : 4;

        //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        public byte[] commandList;//! @note issues here list number is 15
                                  //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I2, SizeConst = 16)]
        public short[] commandParameter;
    }

    /**
     * ACK Data
     */

    public class MissionACK
    {
        public byte ack;

        public unsafe MissionACK(Ptr p)
        {
            ack = p.Byte;
        }
    }

    public class SimpleACK
    {
        public unsafe SimpleACK(Ptr p)
        {
            ack = p.UInt16;
        }

        public ushort ack;
    }

    public class HotPointStartACK
    {
        public unsafe HotPointStartACK(Ptr p)
        {
            ack = p.Byte;
            maxRadius = (p + 1).Single;
        }

        public byte ack;
        public float maxRadius;
    }
    // HotpointStartACK;

    public class WayPointInitACK
    {
        public unsafe WayPointInitACK(Ptr p)
        {
            ack = p.Byte;
        }
        public byte ack;
        public WayPointInitData data;
    }
    // WayPointInitACK;

    public class WayPointDataACK
    {
        public unsafe WayPointDataACK(Ptr p)
        {
            ack = p.Byte;
            index = (p + 1).Byte;
        }
        public byte ack;
        public byte index;
        public WayPointData data;
    } // WayPointDataACK;

    public class WayPointVelocityACK
    {

        public unsafe WayPointVelocityACK(Ptr p)
        {
            ack = p.Byte;
            idleVelocity = (p + 1).Single;
        }

        public byte ack;
        public float idleVelocity;
    } // WayPointVelocityACK;

    // HotPoint data read from flight controller
    public class HotPointReadACK
    {
        public unsafe HotPointReadACK(Ptr p)
        {
            ack = new MissionACK(p);
        }
        public MissionACK ack;
        public HotPointData data;
    }  // HotpointReadACK;

    public class DroneVersionACK
    {
        public DroneVersionACK(Ptr p)
        {
            ack = p;
        }
        public Ptr ack;
    } // DroneVersionACK;

    public class MissionACKUnion
    {
        public byte[] raw_ack_array = new byte[CoreAPI.MAX_ACK_SIZE];

        public DroneVersionACK droneVersion
        {
            get { return new DroneVersionACK(raw_ack_array); }
        }

        public MissionACK missionACK
        {
            get { return new MissionACK(raw_ack_array); }
        }

        public SimpleACK simpleACK
        {
            get { return new SimpleACK(raw_ack_array); }
        }

        public HotPointStartACK hotpointStartACK
        {
            get { return new HotPointStartACK(raw_ack_array); }
        }

        // Contains 1-Byte ACK plus hotpoint mission
        // information read from flight controller
        public HotPointReadACK hotpointReadACK
        {
            get { return new HotPointReadACK(raw_ack_array); }
        }


        // Contains 1-Byte ACK plus waypoint mission
        // information read from flight controller
        public WayPointInitACK waypointInitACK
        {
            get { return new WayPointInitACK(raw_ack_array); }
        }

        // Contains 1-Byte ACK plus waypoint mission
        // information read from flight controller
        public WayPointDataACK waypointDataACK
        {
            get { return new WayPointDataACK(raw_ack_array); }
        }


        public WayPointVelocityACK waypointVelocityACK
        {
            get { return new WayPointVelocityACK(raw_ack_array); }
        }
    }

    public class QuaternionData : NativeObject
    {
        public float q0;
        public float q1;
        public float q2;
        public float q3;

#if !NETMF
        public override string ToString()
        {
            return $"X {q0:0.000} / Y {q1:0.000} / Z {q2:0.000} / W {q3:0.000}";
        }
#endif
        public override int TypeSize
        {
            get { return 16; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            q0 = (p + 0).Single;
            q1 = (p + 4).Single;
            q2 = (p + 8).Single;
            q3 = (p + 12).Single;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetSingle(q0);
            (p + 4).SetSingle(q1);
            (p + 8).SetSingle(q2);
            (p + 12).SetSingle(q3);
        }
    } // QuaternionData;


    //! @note this struct will replace CommonData in the next release.
    //! Eigen-like naming convention
    public class Vector3fData : NativeObject
    {
        public float x;
        public float y;
        public float z;

#if !NETMF
        public override string ToString()
        {
            return $"X {x:0.000} / Y {y:0.000} / Z {z:0.000}";

        }
#endif

        public override int TypeSize
        {
            get { return 12; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            x = (p + 0).Single;
            y = (p + 4).Single;
            z = (p + 8).Single;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetSingle(x);
            (p + 4).SetSingle(y);
            (p + 8).SetSingle(z);
        }
    } /// Vector3fData;

    public class VelocityData : NativeObject
    {
        public float x;
        public float y;
        public float z;

        byte _data;

#if !NETMF
        public override string ToString()
        {
            return $"X {x:0.000} / Y {y:0.000} / Z {z:0.000} / Data {_data:X2}";
        }
#endif

        //byte health : 1;
        //byte sensorID : 4;
        //byte reserve : 3;

        public override int TypeSize
        {
            get { return 13; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            x = (p + 0).Single;
            y = (p + 4).Single;
            z = (p + 8).Single;
            _data = (p + 12).Byte;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetSingle(x);
            (p + 4).SetSingle(y);
            (p + 8).SetSingle(z);
            (p + 12).SetByte(_data);
        }
    } // VelocityData;

    public class PositionData : NativeObject
    {
        public double latitude;
        public double longitude;
        //! @warning the 'altitude' field will be renamed in a future release.
        //! @note the altitude value can be configured to output GPS-only data
        //!       or a fusion of GPS and Baro in Assistant 2's SDK Tab, 'ALTI' 
        public float altitude;

        //! @warning the 'height' field will be renamed in a future release.
        //! @note the height value can be configured to output AGL height
        //!       or height relative to takeoff in Assistant 2's SDK Tab, 'HEIGHT'
        public float height;

        public byte health;

#if !NETMF
        public override string ToString()
        {
            return $"Lat {latitude:0.000} / Lon {longitude:0.000} / Alt {altitude:0.000}";
        }
#endif

        public override int TypeSize
        {
            get { return 25; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            latitude = (p + 0).Double;
            longitude = (p + 8).Double;

            altitude = (p + 16).Single;
            height = (p + 20).Single;
            health = (p + 24).Byte;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetDouble(latitude);
            (p + 8).SetDouble(longitude);

            (p + 16).SetSingle(altitude);
            (p + 20).SetSingle(height);
            (p + 24).SetByte(health);
        }
    } // PositionData;

    //! @warning the 'RadioData' struct will be deprecated in the next release and renamed to RCData. Use RCData instead.
    public class RCData : NativeObject
    {
        public short roll;
        public short pitch;
        public short yaw;
        public short throttle;
        public short mode;
        public short gear;

#if !NETMF
        public override string ToString()
        {
            return $"R {roll:0} / P {pitch} / Y {yaw} / T {throttle} / M {mode} / G {gear}";
        }
#endif

        public override int TypeSize
        {
            get
            {
                return 2 * 6;
            }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            roll = (p + 0).Int16;
            pitch = (p + 2).Int16;
            yaw = (p + 4).Int16;
            throttle = (p + 6).Int16;
            mode = (p + 8).Int16;
            gear = (p + 10).Int16;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetInt16(roll);
            (p + 2).SetInt16(pitch);
            (p + 4).SetInt16(yaw);
            (p + 6).SetInt16(throttle);
            (p + 8).SetInt16(mode);
            (p + 10).SetInt16(gear);
        }
    } // RadioData;


    ////! @note This struct will replace RadioData in the next release. 
    //typedef struct RCData
    //{
    //    int16_t roll;
    //    int16_t pitch;
    //    int16_t yaw;
    //    int16_t throttle;
    //    int16_t mode;
    //    int16_t gear;
    //} //RCData;


    ////! @warning the 'MagnetData' struct will be deprecated in the next release and renamed to MagData. Use MagData instead.
    internal class __MagnetData
    {
        short x;
        short y;
        short z;
    } // MagnetData;


    //! @note This struct will replace MagnetData in the next release.
    public class MagData : NativeObject
    {
        public short x;
        public short y;
        public short z;

#if !NETMF
        public override string ToString()
        {
            return $"X {x} / Y {y} / Z {z}";
        }
#endif

        public override int TypeSize
        {
            get { return 6; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            x = (p + 0).Int16;
            y = (p + 2).Int16;
            z = (p + 4).Int16;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetInt16(x);
            (p + 2).SetInt16(y);
            (p + 4).SetInt16(z);
        }
    } // MagData;

    //! @note This struct is provided as a means for users to provide sigle GPS points to the SDK.
    //!       It does not follow standard SDK GPS datatypes. This may change in a future release. 
    public class GPSPositionData : NativeObject
    {
        public double latitude;
        public double longitude;
        //! @warning please provide relative height in the altitude field. The name will change in a future release.
        public double altitude;

#if !NETMF
        public override string ToString()
        {
            return base.ToString();
        }
#endif

        public override int TypeSize
        {
            get { return 8 * 3; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            latitude = (p + 0).Double;
            longitude = (p + 8).Double;
            altitude = (p + 16).Double;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetDouble(latitude);
            (p + 8).SetDouble(longitude);
            (p + 16).SetDouble(altitude);
        }
    } // GPSPositionData;

    public class CtrlInfoData : NativeObject
    {
        //! @todo mode remote to enums
        byte _mode;
        byte _data;


#if !NETMF
        public override string ToString()
        {
            return $"M {mode} / D {deviceStatus} / {flightStatus} / VRC {vrcStatus}";
        }
#endif

        public FlightMode mode
        {
            get { return (FlightMode)_mode; }
        }


        public FlightDevice deviceStatus /*0->rc  1->app  2->serial*/
        {
            get { return (FlightDevice)(_data & 0x5); }
        }

        public bool flightStatus/*1->opensd  0->close*/
        {
            get { return ((_data >> 3) & 0x1) == 1; }
        }

        public bool vrcStatus
        {
            get { return ((_data >> 4) & 0x1) == 1; }
        }

        public override int TypeSize
        {
            get { return 2; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            _mode = p.Byte;
            if (size > 1)
            {
                _data = (p + 1).Byte;
            }

        }

        public override unsafe void WriteTo(Ptr p)
        {
            p.SetByte(_mode);
            (p + 1).SetByte(_data);
        }

        //public byte reserved : 3;
    } // CtrlInfoData;

    public class TimeData : NativeObject
    {
        int _data;

        public static implicit operator int(TimeData t)
        {
            return t._data;
        }

#if !NETMF
        public override string ToString()
        {
            return _data.ToString();
        }
#endif

        public override int TypeSize
        {
            get { return 4; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            _data = p.Int32;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            p.SetInt32(_data);
        }
    }

    public class TimeStampData : NativeObject
    {
        //! @todo type modify
        public TimeData time = new TimeData();
        public uint nanoTime;
        public byte syncFlag;

#if !NETMF
        public override string ToString()
        {
            return $"{time * 1E-3:0.000} / N {(nanoTime % 1E+6) * 1E-6:0.000000} / Sync {syncFlag}";
        }
#endif

        public override int TypeSize
        {
            get { return 9; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            time.ReadFrom(p, size);
            nanoTime = (p + 4).UInt32;
            syncFlag = (p + 8).Byte;
        }

        public override unsafe void WriteTo(Ptr p)
        {

        }
    } // TimeStampData;

    public class GimbalData : NativeObject
    {
        public float roll;
        public float pitch;
        public float yaw;


        public bool pitchLimit
        {
            get { return (_data & 0x1) == 1; }
            set { _data = (byte)((_data & 0xFE) | (value ? 1 << 0 : 0)); }
        }
        public bool rollLimit
        {
            get { return ((_data >> 1) & 0x1) == 1; }
            set { _data = (byte)((_data & 0xFD) | (value ? 1 << 1 : 0)); }
        }
        public bool yawLimit
        {
            get { return ((_data >> 2) & 0x1) == 1; }
            set { _data = (byte)((_data & 0xFB) | (value ? 1 << 2 : 0)); }
        }
        //byte reserved : 5;

        private byte _data;

#if !NETMF
        public override string ToString()
        {
            return $"R {roll} / P {pitch} / Y {yaw}";
        }
#endif

        public override int TypeSize
        {
            get { return 12 + 1; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            roll = (p + 0).Single;
            pitch = (p + 4).Single;
            yaw = (p + 8).Single;

            _data = (p + 12).Byte;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetSingle(roll);
            (p + 4).SetSingle(pitch);
            (p + 8).SetSingle(yaw);

            (p + 12).SetByte(_data);
        }
    } // GimbalData;

    public class StatusData : NativeObject
    {
        byte _data;

#if !NETMF
        public override string ToString()
        {
            return ((FlightStatus)_data).ToString();
        }
#endif

        public override int TypeSize
        {
            get { return 1; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            if (size >= 1)
            {
                _data = p.Byte;
            }
        }

        public override unsafe void WriteTo(Ptr p)
        {
            p.SetByte(_data);
        }

        public static implicit operator FlightStatus(StatusData value)
        {
            return (FlightStatus)value._data;
        }
    }

    public class TaskData
    {
        public TaskData()
        {
            Buffer = new byte[2];
        }

        public byte cmdSequence
        {
            get { return Buffer[0]; }
            set { Buffer[0] = value; }
        }
        public byte cmdData { get { return Buffer[1]; } set { Buffer[1] = value; } }

        public byte[] Buffer { get; private set; }
        public static int TypeSize { get { return 2; } }
    } // TaskData;

    //! @todo rename to a final version
    //! RTKData from the A3. This is not available on the M100.
    public class RTKData : NativeObject
    {
        uint date;
        uint time;
        double longitude;
        double latitude;
        //! @warning the 'Hmsl' field will be renamed in a future release.
        float Hmsl;

        float velocityNorth;
        float velocityEast;
        //! @warning the 'velocityGround' field will be renamed to velocityDown in the next release.
        float velocityGround;

        short yaw;
        byte posFlag;
        byte yawFlag;

        public override int TypeSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            throw new NotImplementedException();
        }

        public override unsafe void WriteTo(Ptr p)
        {
            throw new NotImplementedException();
        }
    } // RTKData;

    //! @todo rename to a final version
    //! Detailed GPSData from the A3. This is not available on the M100.
    public class GPSData : NativeObject
    {
        public uint date;
        public uint time;
        public int longitude;
        public int latitude;

        public int Hmsl;

        public float velocityNorth;
        public float velocityEast;
        //! @warning the 'velocityGround' field will be renamed to velocityDown in the next release.
        public float velocityGround;

        public override int TypeSize
        {
            get { return 4 * 8; }
        }

        public override unsafe void ReadFrom(Ptr p, int size)
        {
            date = (p + 0).UInt32;
            time = (p + 4).UInt32;
            longitude = (p + 8).Int32;
            latitude = (p + 12).Int32;
            Hmsl = (p + 16).Int32;
            velocityNorth = (p + 20).Single;
            velocityEast = (p + 24).Single;
            velocityGround = (p + 28).Single;
        }

        public override unsafe void WriteTo(Ptr p)
        {
            (p + 0).SetUInt32(date);
            (p + 4).SetUInt32(time);
            (p + 8).SetInt32(longitude);
            (p + 12).SetInt32(latitude);
            (p + 16).SetInt32(Hmsl);
            (p + 20).SetSingle(velocityNorth);
            (p + 24).SetSingle(velocityEast);
            (p + 28).SetSingle(velocityGround);
        }
    } // GPSData;

    //# ifndef SDK_DEV
    //! @todo
    public class BroadcastData
    {
        public ushort dataFlag;
        public TimeStampData timeStamp = new TimeStampData();
        public QuaternionData q = new QuaternionData();
        //! @warning the CommonData type will change to Vector3fData in a future release
        public Vector3fData a = new Vector3fData();
        public VelocityData v = new VelocityData();
        //! @warning the CommonData type will change to Vector3fData in a future release
        public Vector3fData w = new Vector3fData();
        public PositionData pos = new PositionData();
        //! @warning the MagnetData type will change to MagData in a future release
        public MagData mag = new MagData();
        public GPSData gps = new GPSData();
        public RTKData rtk = new RTKData();
        //! @warning the RadioData type will change to RCData in a future release
        public RCData rc = new RCData();

        public GimbalData gimbal = new GimbalData();
        public StatusData status = new StatusData(); //! @todo define enum
        public BatteryData battery = new BatteryData();
        public CtrlInfoData ctrlInfo = new CtrlInfoData();

        //! @note this variable is not set by the FC but populated by the API
        public ACK_ACTIVE_CODE activation;


        public BroadcastData Clone()
        {
            var c = new BroadcastData();
            c.dataFlag = dataFlag;
            c.timeStamp = timeStamp;
            c.q = q;
            c.a = a;
            c.v = v;
            c.w = w;
            c.pos = pos;
            c.mag = mag;
            c.gps = gps;
            c.rtk = rtk;
            c.rc = rc;
            c.gimbal = gimbal;
            c.status = status;
            c.battery = battery;
            c.ctrlInfo = ctrlInfo; ;

            return c;
        }
    } // BroadcastData;
      //#endif // SDK_DEV

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VirtualRCSetting
    {
        byte _data;

        //byte enable : 1;
        //byte cutoff : 1;
        //byte reserved : 6;
    } // VirtualRCSetting;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VirtualRCData
    {
        //! @note this is default mapping data structure for
        //! virtual remote controller.
        //! @todo channel mapping
        uint roll;
        uint pitch;
        uint throttle;
        uint yaw;
        uint gear;
        uint reserved;
        uint mode;
        uint Channel_07;
        uint Channel_08;
        uint Channel_09;
        uint Channel_10;
        uint Channel_11;
        uint Channel_12;
        uint Channel_13;
        uint Channel_14;
        uint Channel_15;
    }  // VirtualRCData;

    public class ActivateData
    {
        public int ID;
        public int reserved;
        public Version version;
        public byte[] iosID = new byte[32];
        public byte[] encKey;

        public static int Size { get { return 4 + 4 + 4 + 32; } }
        public unsafe byte[] GetBytes()
        {
            var raw = new byte[Size];

            var p = (Ptr)raw;

            (p + 0).SetInt32(ID);
            (p + 4).SetInt32(reserved);
            (p + 8).SetInt32(version.RawVersion);

            Array.Copy(iosID, 0, raw, 12, iosID.Length);

            return raw;

        }
    } // ActivateData;

    /**
     * Versioning. VersionData struct updated @ FW 3.2.15.73
     */

    public class VersionData
    {
        public ACK_COMMON_CODE version_ack;
        public int version_crc;
        public string hw_serial_num;
        public string hwVersion; //! Current longest product code: pm820v3pro
        public Version fwVersion;

        //! Legacy member
        public string version_name;
    } // VersionData;

    public struct req_id_t
    {
        private int _data;

        public ushort sequence_number
        {
            get { return (ushort)(_data & 0xFFFF); }
            set { _data = (int)(_data & 0xFFFF0000) | value; }
        }
        public byte session_id
        {
            get { return (byte)((_data >> 16) & 0xFF); }
            set { _data = (int)(_data & 0xFF00FFFF) | (value << 16); }
        }
        public bool need_encrypt
        {
            get { return (_data >> 24) != 0; }
            set
            {
                _data = (_data & 0x00FFFFFF) | ((value ? 0x01 : 0x00) << 24);
            }
        }
    }

    //#pragma pack()
    //# ifdef SDK_DEV
    //# include "devtype.h"
    //#endif // SDK_DEV

    partial class CoreAPI
    {
        public const int PRO_PURE_DATA_MAX_SIZE = 1007;// 2^10 - header size
        public const int MMU_TABLE_NUM = 32;
    }
}
