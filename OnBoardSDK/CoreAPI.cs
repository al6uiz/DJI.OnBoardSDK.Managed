using System;
using System.Text;

namespace DJI.OnBoardSDK
{
    //! CoreAPI implements core Open Protocol communication between M100/M600/A3 and your onboard embedded platform.
    /*!\remark
	 *  API is running on two poll threads:\n
	 *  - sendPoll(){}\n
	 *  - readPoll(){}\n
	 *  Please make sure both threads operate correctly.\n
	 *
	 * @note
	 * if you can read data in a interrupt, try to pass data through
	 * byteHandler() or byteStreamHandler()
	 *
	 */
    public partial class CoreAPI
    {
        public CoreAPI(IPlatformDriver sDevice = null, bool userCallbackThread = false,
                  CallBack userRecvCallback = null, UserData userData = null)
        {
            CallBackHandler handler;
            handler.callback = userRecvCallback;
            handler.userData = userData;
            init(sDevice, handler, userCallbackThread);
        }

        /// Serial Device Initialization
        void init(IPlatformDriver Driver, CallBackHandler userRecvCallback,
            bool userCallbackThread)
        {
            LastSequence = int.MaxValue;

            serialDevice = Driver;
            // serialDevice->init();

            seq_num = 0;
            ackFrameStatus = 11;
            broadcastFrameStatus = false;

            filter.recvIndex = 0;
            filter.reuseCount = 0;
            filter.reuseIndex = 0;
            filter.encode = 0;



            recvCallback.callback = userRecvCallback.callback;
            recvCallback.userData = userRecvCallback.userData;

            callbackThread = false;
            hotPointData = false;
            followData = false;
            wayPointData = false;
            callbackThread = userCallbackThread;

            nonBlockingCBThreadEnable = false;
            ack_data = 99;
            versionData.fwVersion = Version.Zero; //! Default init value
            ack_activation = ACK_ACTIVE_CODE.ACK_ACTIVE_DEFAULT;

            //! This handles hotfix for Movement Control issue with Z position Control
            homepointAltitude = 999999;

            setup();
        }


        public CoreAPI(IPlatformDriver sDevice, CallBackHandler userRecvCallback,
                  bool userCallbackThread = false)
        {
            init(sDevice, userRecvCallback, userCallbackThread);
            getFwVersion();
        }

        #region
        /**
		 * @remark
		 * void send() - core overloaded function which can be invoked in three different ways.
		 * void send(CallbackCommand *parameter) - main interface
		 * (other two overloaded functions are builded on the base of this function)
		 * Please be careful when passing in UserData, there might have memory leak problems.
		 *
		 */
        public void send(byte session, bool is_enc, CMD_SET cmdSet,
            byte cmdID, Ptr pdata, int len, CallBack ackCallback = null,
            int timeout = 0, int retry = 1)
        {
            Command param = new Command();
            var ptemp = (Ptr)encodeSendData;
            ptemp[0] = (byte)cmdSet;
            ptemp[1] = cmdID;

            Array.Copy(pdata.Buffer, pdata.Offset, encodeSendData, 2, len);

            param.handler = ackCallback;
            param.sessionMode = session;
            param.length = len + SET_CMD_SIZE;
            param.buf = encodeSendData;
            param.retry = retry;

            param.timeout = timeout;
            param.encrypt = is_enc;

            param.userData = null;

            sendInterface(ref param);
        }

        public void send(byte session_mode, bool is_enc, CMD_SET cmd_set,
            byte cmd_id, Ptr pdata, int len, int timeout,
            int retry_time, CallBack ack_handler = null, UserData userData = null)
        {
            Command param = new Command();
            var ptemp = (Ptr)encodeSendData;
            ptemp[0] = (byte)cmd_set;
            ptemp[1] = cmd_id;

            Array.Copy(pdata.Buffer, pdata.Offset, encodeSendData, 2, len);

            param.handler = ack_handler;
            param.sessionMode = session_mode;
            param.length = len + SET_CMD_SIZE;
            param.buf = encodeSendData;
            param.retry = retry_time;

            param.timeout = timeout;
            param.encrypt = is_enc;

            param.userData = userData;

            sendInterface(ref param);
        }

        /**@note Main interface*/
        public void send(ref Command parameter)
        {
            sendInterface(ref parameter);
        }
        #endregion


        public void ack(ref req_id_t req_id, Ptr ackdata, int len)
        {
            Ack param = new Ack();

            Array.Copy(ackdata.Buffer, ackdata.Offset, encodeACK, 0, len);

            param.sessionID = req_id.session_id;
            param.seqNum = req_id.sequence_number;
            param.encrypt = req_id.need_encrypt;
            param.buf = encodeACK;
            param.length = len;

            ackInterface(ref param);
        }


        #region
        /**
		 * Get aircraft version.
		 *
		 * @note
		 * You can query your flight controller prior to activation.
		 */
        public void getDroneVersion(CallBack callback = null, UserData userData = null)
        {
            versionData.version_ack = ACK_COMMON_CODE.ACK_COMMON_NO_RESPONSE;
            versionData.version_crc = 0x0;
            versionData.version_name = null;

            var cmd_timeout = 100; // unit is ms
            var retry_time = 3;
            byte cmd_data = 0;

            send(2, false, CMD_SET.SET_ACTIVATION, (byte)ACTIVATION_CODE.CODE_GETVERSION, new byte[] { cmd_data }, 1,
                cmd_timeout, retry_time,
                callback != null ? callback : getDroneVersionCallback, userData);
        }

        /**
		 * Blocking API Control
		 *
		 * @brief
		 * Get drone version from flight controller block until
		 * ACK arrives from flight controller
		 *
		 * @return VersionData containing ACK value, CRC of the
		 * protocol version and protocol version itself
		 *
		 * @todo
		 * Implement high resolution timer to catch ACK timeout
		 */
        public VersionData getDroneVersion(int timeout)
        {
            versionData.version_ack = ACK_COMMON_CODE.ACK_COMMON_NO_RESPONSE;
            versionData.version_crc = 0x0;
            versionData.fwVersion = Version.Zero;
            versionData.version_name = null;

            var cmd_timeout = timeout; // unit is ms
            var retry_time = 3;
            byte cmd_data = 0;

            send(2, false, CMD_SET.SET_ACTIVATION, (byte)ACTIVATION_CODE.CODE_GETVERSION, new byte[] { cmd_data }, 1,
                cmd_timeout, retry_time);

            //! Wait for end of ACK frame to arrive
            serialDevice.lockACK();
            serialDevice.wait(timeout);
            serialDevice.freeACK();

            //! Pointer to ACK
            //unsigned char* ptemp = &(missionACKUnion.droneVersion.ack[0]);

            //! Parse the HW & SW version, Serial no. and ACK. Discard return value, we don't process it right now.
            if (!parseDroneVersionInfo(missionACKUnion.raw_ack_array))
            {
                versionData.version_crc = 0x0;
                versionData.fwVersion = Version.Zero;
                versionData.version_name = null;
            }

            return versionData;
        }

        /**
		* Parse SDK version returned from drone, and populate the API versionData member
		*/
        private bool parseDroneVersionInfo(Ptr ackPtrIncoming)
        {

            //! Local copy to prevent overwriting the ACK store
            var buf = new byte[64];
            Array.Copy(ackPtrIncoming.Buffer, ackPtrIncoming.Offset, buf, 0, 64);
            var ackPtr = (Ptr)buf;

            //! Note down our starting point as a sanity check
            var startPtr = ackPtr;
            //! 2b ACK.
            versionData.version_ack = (ACK_COMMON_CODE)(ackPtr[0] + (ackPtr[1] << 8));
            ackPtr += 2;

            //! Next, we might have CRC or ID; Put them into a variable that we will parse 
            //! later. Find next \0
            var crc = new byte[16];
            var crc_id = (Ptr)crc;

            int i = 0;
            while (ackPtr.Byte != 0)
            {
                crc_id[i] = ackPtr.Byte;
                i++;
                ackPtr++;
                if (ackPtr - startPtr > 18)
                {
                    API_LOG(serialDevice, ERROR_LOG, "Drone version was not obtained. Please " +
                    "restart the program or call " +
                    "getDroneVersion");
                    return false;
                }
            }
            //! Fill in the termination character
            crc_id[i] = ackPtr.Byte;
            ackPtr++;

            var vb = new StringBuilder();

            //! Now we're at the name. First, let's fill up the name field.
            for (i = 0; i < 32; i++)
            {
                var letter = (char)(ackPtr + i).Byte;
                if (letter != '\0')
                {
                    vb.Append(letter);
                }
            }
            versionData.version_name = vb.ToString();

            //! Now, we start parsing the name. Let's find the second space character.
            while (ackPtr.Byte != ' ')
            {
                ackPtr++;
                if (ackPtr - startPtr >= 64)
                {
                    API_LOG(serialDevice, ERROR_LOG, "Drone version was not obtained. Please " +
                    "restart the program or call " +
                    "getDroneVersion");
                    return false;
                }
            } //! Found first space ("SDK-v1.x")
            ackPtr++;

            while (ackPtr.Byte != ' ')
            {
                ackPtr++;
                if (ackPtr - startPtr > 64)
                {
                    API_LOG(serialDevice, ERROR_LOG, "Drone version was not obtained. Please " +
                    "restart the program or call " +
                    "getDroneVersion");
                    return false;
                }
            } //! Found second space ("BETA")
            ackPtr++;

            vb.Length = 0;

            //! Next is the HW version
            int j = 0;
            while (ackPtr.Byte != '-')
            {
                vb.Append((char)ackPtr.Byte);
                ackPtr++;
                j++;
                if (ackPtr - startPtr > 64)
                {
                    API_LOG(serialDevice, ERROR_LOG, "Drone version was not obtained. Please " +
                    "restart the program or call " +
                    "getDroneVersion");
                    return false;
                }
            }
            versionData.hwVersion = vb.ToString();
            //! Fill in the termination character
            //versionData.hwVersion[j] = '\0';
            ackPtr++;

            //! Finally, we come to the FW version. We don't know if each clause is 2 or 3 
            //! digits long.
            int ver1 = 0, ver2 = 0, ver3 = 0, ver4 = 0;

            while (ackPtr.Byte != '.')
            {
                ver1 = (ackPtr.Byte - 48) + 10 * ver1;
                ackPtr++;
                if (ackPtr - startPtr > 64)
                {
                    API_LOG(serialDevice, ERROR_LOG, "Drone version was not obtained. Please " +
                    "restart the program or call " +
                    "getDroneVersion");
                    return false;
                }
            }
            ackPtr++;
            while (ackPtr.Byte != '.')
            {
                ver2 = (ackPtr.Byte - 48) + 10 * ver2;
                ackPtr++;
                if (ackPtr - startPtr > 64)
                {
                    API_LOG(serialDevice, ERROR_LOG, "Drone version was not obtained. Please " +
                    "restart the program or call " +
                    "getDroneVersion");
                    return false;
                }
            }
            ackPtr++;
            while (ackPtr.Byte != '.')
            {
                ver3 = (ackPtr.Byte - 48) + 10 * ver3;
                ackPtr++;
                if (ackPtr - startPtr > 64)
                {
                    API_LOG(serialDevice, ERROR_LOG, "Drone version was not obtained. Please " +
                    "restart the program or call " +
                    "getDroneVersion");
                    return false;
                }
            }
            ackPtr++;
            while (ackPtr.Byte != '\0')
            {
                ver4 = (ackPtr.Byte - 48) + 10 * ver4;
                ackPtr++;
                if (ackPtr - startPtr > 64)
                {
                    API_LOG(serialDevice, ERROR_LOG, "Drone version was not obtained. Please " +
                    "restart the program or call " +
                    "getDroneVersion");
                    return false;
                }
            }

            versionData.fwVersion = new Version(ver1, ver2, ver3, ver4);

            //! Special cases
            //! M100:
            if (versionData.hwVersion == "M100")
            {
                //! Bug in M100 does not report the right FW.
                ver3 = 10 * ver3;
                versionData.fwVersion = new Version(ver1, ver2, ver3, ver4);
            }
            //! M600/A3 FW 3.2.10
            if (versionData.fwVersion == new Version(3, 2, 10, 0))
            {
                //! Bug in M600 does not report the right FW.
                ver3 = 10 * ver3;
                versionData.fwVersion = new Version(ver1, ver2, ver3, ver4);
            }

            //! Now, we can parse the CRC and ID based on FW version. If it's older than 
            //! 3.2 then it'll have a CRC, else not.
            if (versionData.fwVersion < new Version(3, 2, 0, 0))
            {
                versionData.version_crc =
                    crc_id[0] + (crc_id[1] << 8) + (crc_id[2] << 16) + (crc_id[3] << 24);
                var id_ptr = crc_id + 4;

                vb.Length = 0;
                i = 0;
                while (id_ptr.Byte != '\0')
                {
                    vb.Append((char)id_ptr.Byte);
                    i++;
                    id_ptr++;
                    if (id_ptr - ((Ptr)crc_id + 4) > 12)
                    {
                        API_LOG(serialDevice, ERROR_LOG, "Drone ID was not obtained.");
                        return false; //!Not catastrophic error
                    }
                }
                //! Fill in the termination character
                versionData.hw_serial_num = vb.ToString();
            }
            else
            {
                versionData.version_crc = 0;
                var id_ptr = crc_id;

                vb.Length = 0;
                i = 0;
                while (id_ptr.Byte != '\0')
                {
                    vb.Append((char)id_ptr.Byte);
                    i++;
                    id_ptr++;
                    if (id_ptr - crc_id > 16)
                    {
                        API_LOG(serialDevice, ERROR_LOG, "Drone ID was not obtained.");
                        return false; //!Not catastrophic error
                    }
                }
                //! Fill in the termination character
                versionData.hw_serial_num = vb.ToString();
            }

            //! Finally, we print stuff out.

            if (versionData.fwVersion > new Version(3, 1, 0, 0))
            {
                API_LOG(serialDevice, STATUS_LOG, "Device Serial No. = {0}",
                versionData.hw_serial_num);
            }
            API_LOG(serialDevice, STATUS_LOG, "Hardware = {0}",
                    versionData.hwVersion);
            API_LOG(serialDevice, STATUS_LOG, "Firmware = {0}.{1}.{2}.{3}", ver1,
                    ver2, ver3, ver4);
            if (versionData.fwVersion < new Version(3, 2, 0, 0))
            {
                API_LOG(serialDevice, STATUS_LOG, "Version CRC = 0x{0:X8}",
                versionData.version_crc);
            }
            return true;
        }


        /// Activation Control
        /**
		 *
		 * @drief
		 * Send activation request to your flight controller
		 * to check if:  a) your application registered in your developer
		 * account  b) API Control enabled in the Assistant software
		 * Proceed to programming if activation successful.
		 */
        public void activate(ActivateData data, CallBack callback = null, UserData userData = null)
        {

            //! First, we need to check if getDroneVersion has been called
            if (versionData.fwVersion == Version.Zero)
            {
                API_LOG(serialDevice, ERROR_LOG, "Please call getDroneVersion first.");
                return;
            }
            data.version = versionData.fwVersion;
            accountData = data;
            accountData.reserved = 2;

            for (int i = 0; i < 32; ++i)
                accountData.iosID[i] = (byte)'0'; //! @note for ios verification
            API_LOG(serialDevice, DEBUG_LOG, "version 0x{0:X}", versionData.fwVersion.RawVersion);
            API_LOG(serialDevice, DEBUG_LOG, "{0}", Utility.GetString(accountData.iosID));
            send(2, false, CMD_SET.SET_ACTIVATION, (byte)ACTIVATION_CODE.CODE_ACTIVATE, accountData.GetBytes(),
                ActivateData.Size, 1000, 3,
                callback ?? activateCallback, userData);
        }


        /// Blocking API Control
        /**
		* @remark
		* Blocks until ACK frame arrives or timeout occurs
		*
		* @brief
		* Send activation control to your flight controller to check if:  a)
		* your application registered in your developer
		* account  b) API Control enabled in the Assistant software
		* Proceed to programming if activation successful.
		*
		* @return ACK from flight controller
		*
		* @todo
		* Implement high resolution timer to catch ACK timeout
		*/
        public ACK_ACTIVE_CODE activate(ActivateData data, int timeout)
        {
            //! First, we need to check if getDroneVersion has been called
            if (versionData.fwVersion == Version.Zero)
            {
                this.getDroneVersion(1);
            }
            //! Now, look into versionData and set for activation.
            data.version = versionData.fwVersion;
            accountData = data;
            accountData.reserved = 2;
            for (int i = 0; i < 32; ++i)
                accountData.iosID[i] = (byte)'0'; //! @note for ios verification
            API_LOG(serialDevice, DEBUG_LOG, "version 0x{0:X}", versionData.fwVersion.RawVersion);
            API_LOG(serialDevice, DEBUG_LOG, "{0}", Utility.GetString(accountData.iosID));
            send(2, false, CMD_SET.SET_ACTIVATION, (byte)ACTIVATION_CODE.CODE_ACTIVATE, accountData.GetBytes(),
                ActivateData.Size, 1000, 3, null, null);

            // Wait for end of ACK frame to arrive
            serialDevice.lockACK();
            serialDevice.wait(timeout);
            serialDevice.freeACK();
            ack_data = missionACKUnion.simpleACK.ack;
            if ((ACK_ACTIVE_CODE)ack_data == ACK_ACTIVE_CODE.ACK_ACTIVE_SUCCESS && accountData.encKey != null)
                setKey(accountData.encKey);

            return (ACK_ACTIVE_CODE)ack_data;
        }

        #endregion
        public void sendToMobile(Ptr data, byte len, CallBack callback = null,
                UserData userData = null)
        {
            if (len > 100)
            {
                API_LOG(serialDevice, ERROR_LOG, "Too much data to send");
                return;
            }
            send(0, false, CMD_SET.SET_ACTIVATION, (byte)ACTIVATION_CODE.CODE_TOMOBILE, data, len, 500, 1,
                 callback ?? sendToMobileCallback, userData);
        }



        /**
		 * @drief
		 * Set broadcast frequency.
		 *
		 * @remark
		 * We offer 12 frequency channels to customize:
		 * 0 - Timestamp
		 * 1 - Attitude Quaterniouns
		 * 2 - Acceleration
		 * 3 - Velocity (Ground Frame)
		 * 4 - Angular Velocity (Body Frame)
		 * 5 - Position
		 * 6 - Magnetometer
		 * 7 - RC Channels Data
		 * 8 - Gimbal Data
		 * 9 - Flight Status
		 * 10 - Battery Level
		 * 11 - Control Information
		 */
        void setBroadcastFreq(Ptr dataLenIs16, CallBack callback = null, UserData userData = null)
        {
            //! @note see also enum BROADCAST_FREQ in DJI_API.h
            for (int i = 0; i < 16; ++i)
            {
                if (versionData.hwVersion == "M100")
                    if (i < 12)
                    {
                        dataLenIs16[i] = (byte)(dataLenIs16[i] > 5 ? 5 : dataLenIs16[i]);
                    }
                    else
                        dataLenIs16[i] = 0;
                else
                {
                    if (i < 14)
                    {
                        dataLenIs16[i] = (byte)(dataLenIs16[i] > 5 ? 5 : dataLenIs16[i]);
                    }
                    else
                        dataLenIs16[i] = 0;
                }
            }
            send(2, false, CMD_SET.SET_ACTIVATION, (byte)ACTIVATION_CODE.CODE_FREQUENCY, dataLenIs16, 16, 100, 1,
                 callback ?? setFrequencyCallback, userData);
        }

        ushort setBroadcastFreq(Ptr dataLenIs16, int timeout)
        {
            //! @note see also enum BROADCAST_FREQ in DJI_API.h
            for (int i = 0; i < 16; ++i)
            {
                if (versionData.hwVersion == "M100")
                    if (i < 12)
                    {
                        dataLenIs16[i] = (byte)(dataLenIs16[i] > 5 ? 5 : dataLenIs16[i]);
                    }
                    else
                        dataLenIs16[i] = 0;
                else
                {
                    if (i < 14)
                    {
                        dataLenIs16[i] = (byte)(dataLenIs16[i] > 5 ? 5 : dataLenIs16[i]);
                    }
                    else
                        dataLenIs16[i] = 0;
                }
            }
            send(2, false, CMD_SET.SET_ACTIVATION, (byte)ACTIVATION_CODE.CODE_FREQUENCY, dataLenIs16, 16, 100, 1, null, null);

            // Wait for end of ACK frame to arrive
            serialDevice.lockACK();
            serialDevice.wait(timeout);
            serialDevice.freeACK();
            return missionACKUnion.simpleACK.ack;
        }




        /**
		 * Reset all broadcast frequencies to their default values
		 */
        void setBroadcastFreqDefaults()
        {
            var freq = new byte[16];

            /* Channels definition:
			 * M100:
			 * 0 - Timestamp
			 * 1 - Attitude Quaterniouns
			 * 2 - Acceleration
			 * 3 - Velocity (Ground Frame)
			 * 4 - Angular Velocity (Body Frame)
			 * 5 - Position
			 * 6 - Magnetometer
			 * 7 - RC Channels Data
			 * 8 - Gimbal Data
			 * 9 - Flight Status
			 * 10 - Battery Level
			 * 11 - Control Information
			 *
			 * A3:
			 * 0 - Timestamp
			 * 1 - Attitude Quaterniouns
			 * 2 - Acceleration
			 * 3 - Velocity (Ground Frame)
			 * 4 - Angular Velocity (Body Frame)
			 * 5 - Position
			 * 6 - GPS Detailed Information
			 * 7 - RTK Detailed Information
			 * 8 - Magnetometer
			 * 9 - RC Channels Data
			 * 10 - Gimbal Data
			 * 11 - Flight Statusack
			 * 12 - Battery Level
			 * 13 - Control Information
			 *
			 */

            if (versionData.hwVersion == "M100")
            {
                freq[0] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_1HZ;
                freq[1] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[2] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[3] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_100HZ;
                freq[4] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[5] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[6] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_1HZ;
                freq[7] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[8] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[9] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_100HZ;
                freq[10] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[11] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
            }
            else
            { //! A3/N3/M600
                freq[0] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_1HZ;
                freq[1] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[2] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[3] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_100HZ;
                freq[4] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[5] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[6] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
                freq[7] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
                freq[8] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_1HZ;
                freq[9] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[10] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[11] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_100HZ;
                freq[12] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[13] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
            }
            setBroadcastFreq(freq);
        }


        /**
		 * Set all broadcast frequencies to zero. Only ACK data will stay on the line.
		 */
        void setBroadcastFreqToZero()
        {
            var freq = new byte[16];

            /* Channels definition:
			 * M100:
			 * 0 - Timestamp
			 * 1 - Attitude Quaterniouns
			 * 2 - Acceleration
			 * 3 - Velocity (Ground Frame)
			 * 4 - Angular Velocity (Body Frame)
			 * 5 - Position
			 * 6 - Magnetometer
			 * 7 - RC Channels Data
			 * 8 - Gimbal Data
			 * 9 - Flight Status
			 * 10 - Battery Level
			 * 11 - Control Information
			 *
			 * A3:
			 * 0 - Timestamp
			 * 1 - Attitude Quaterniouns
			 * 2 - Acceleration
			 * 3 - Velocity (Ground Frame)
			 * 4 - Angular Velocity (Body Frame)
			 * 5 - Position
			 * 6 - GPS Detailed Information
			 * 7 - RTK Detailed Information
			 * 8 - Magnetometer
			 * 9 - RC Channels Data
			 * 10 - Gimbal Data
			 * 11 - Flight Status
			 * 12 - Battery Level
			 * 13 - Control Information
			 *
			 */

            freq[0] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[1] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[2] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[3] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[4] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[5] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[6] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[7] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[8] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[9] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[10] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[11] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[12] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            freq[13] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
            setBroadcastFreq(freq);
        }


        /**
		 * Blocking API Control
		 *
		 * @brief
		 * Set broadcast frequencies to their default values and block until
		 * ACK arrives from flight controller
		 *
		 * @return ACK from flight controller
		 *
		 * @todo
		 * Implement high resolution timer to catch ACK timeout
		 */
        ushort setBroadcastFreqDefaults(int timeout)
        {
            var freq = new byte[16];

            /* Channels definition:
			 * M100:
			 * 0 - Timestamp
			 * 1 - Attitude Quaterniouns
			 * 2 - Acceleration
			 * 3 - Velocity (Ground Frame)
			 * 4 - Angular Velocity (Body Frame)
			 * 5 - Position
			 * 6 - Magnetometer
			 * 7 - RC Channels Data
			 * 8 - Gimbal Data
			 * 9 - Flight Status
			 * 10 - Battery Level
			 * 11 - Control Information
			 *
			 * A3:
			 * 0 - Timestamp
			 * 1 - Attitude Quaterniouns
			 * 2 - Acceleration
			 * 3 - Velocity (Ground Frame)
			 * 4 - Angular Velocity (Body Frame)
			 * 5 - Position
			 * 6 - GPS Detailed Information
			 * 7 - RTK Detailed Information
			 * 8 - Magnetometer
			 * 9 - RC Channels Data
			 * 10 - Gimbal Data
			 * 11 - Flight Statusack
			 * 12 - Battery Level
			 * 13 - Control Information
			 *
			 */

            if (versionData.hwVersion == "M100")
            {
                freq[0] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_1HZ;
                freq[1] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[2] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[3] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_100HZ;
                freq[4] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[5] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[6] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_1HZ;
                freq[7] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[8] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[9] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_100HZ;
                freq[10] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[11] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
            }
            else
            { //! A3/N3/M600
                freq[0] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_1HZ;
                freq[1] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[2] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[3] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_100HZ;
                freq[4] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[5] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[6] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
                freq[7] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_0HZ;
                freq[8] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_1HZ;
                freq[9] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
                freq[10] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[11] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_100HZ;
                freq[12] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_50HZ;
                freq[13] = (byte)BROADCAST_FREQ.BROADCAST_FREQ_10HZ;
            }
            return setBroadcastFreq(freq, timeout);
        }


        /**
	   * Get timestamp from flight controller.
	   *
	   * @note
	   * Make sure you are using appropriate timestamp broadcast frequency. See setBroadcastFreq
	   * function for more details.
	   */
        TimeStampData getTime()
        {
            return getBroadcastData().timeStamp;
        }

        /**
		 * Get flight status at any time during a flight mission.
		 */
        FlightStatus getFlightStatus()
        {
            return getBroadcastData().status;
        }

        void setFromMobileCallback(CallBackHandler FromMobileEntrance)
        {
            fromMobileCallback = FromMobileEntrance;
        }

        /**
		 * Get Activation information
		 */
        ActivateData getAccountData()
        {
            return accountData;
        }

        /// Activation Control
        void setAccountData(ActivateData value)
        {
            accountData = value;
        }



        /// HotPoint Mission Control
        void setHotPointData(bool value)
        {
            hotPointData = value;
        }


        /// WayPoint Mission Control
        void setWayPointData(bool value)
        {
            wayPointData = value;
        }

        /// Follow Me Mission Control
        void setFollowData(bool value)
        {
            followData = value;
        }


        /// HotPoint Mission Control
        bool getHotPointData()
        {
            return hotPointData;
        }

        /// WayPoint Mission Control
        bool getWayPointData()
        {
            return wayPointData;
        }

        // FollowMe mission Control
        bool getFollowData()
        {
            return followData;
        }

        public void setControl(bool enable, CallBack callback = null, UserData userData = null)
        {
            var data = new byte[] { enable ? (byte)1 : (byte)0 };
            send(2, isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_SETCONTROL, data, 1, 500,
                2, callback ?? setControlCallback, userData);
        }

        /// Blocking API Control
        /**
		* @remark
		* Blocks until ACK frame arrives or timeout occurs
		*
		* @brief
		* Obtain control
		*
		* @return ACK from flight controller
		*
		* @todo
		* Implement high resolution timer to catch ACK timeout
		*/
        public ACK_SETCONTROL_CODE setControl(bool enable, int timeout)
        {
            var data = new byte[] { enable ? (byte)1 : (byte)0 };
            send(2, isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_SETCONTROL, data, 1, 500,
                2, null, null);

            // Wait for end of ACK frame to arrive
            serialDevice.lockACK();
            serialDevice.wait(timeout);
            serialDevice.freeACK();

            if ((ACK_SETCONTROL_CODE)missionACKUnion.simpleACK.ack == ACK_SETCONTROL_CODE.ACK_SETCONTROL_ERROR_MODE)
            {
                if (versionData.fwVersion < new Version(3, 2, 0, 0))
                    missionACKUnion.simpleACK.ack = (ushort)ACK_SETCONTROL_CODE.ACK_SETCONTROL_NEED_MODE_F;
                else
                    missionACKUnion.simpleACK.ack = (ushort)ACK_SETCONTROL_CODE.ACK_SETCONTROL_NEED_MODE_P;
            }

            return (ACK_SETCONTROL_CODE)missionACKUnion.simpleACK.ack;
        }




        /**
		 * Get serial device handler.
		 */
        public IPlatformDriver getDriver()
        {
            return serialDevice;
        }

        SimpleACK getSimpleACK()
        {
            return missionACKUnion.simpleACK;
        }

        /**
		 * Initialize serial device
		 */
        void setDriver(IPlatformDriver sDevice)
        {
            serialDevice = sDevice;
        }

        private void getDroneVersionCallback(Ptr protocolHeader,
            UserData userData = null)
        {
            var ptemp = protocolHeader + HEADER_SIZE;
            if (!parseDroneVersionInfo(ptemp))
            {
                versionData.version_crc = 0x0;
                versionData.fwVersion = Version.Zero;
                versionData.version_name = null;
            }
        }


        private void activateCallback(Ptr pHeader,
            UserData userData = null)
        {
            var protocolHeader = (Header)(pHeader);

            ushort ack_data;
            if (protocolHeader.length - EXC_DATA_SIZE <= 2)
            {
                ack_data = (pHeader + HEADER_SIZE).UInt16;

                // Write activation status to the broadcast data
                setBroadcastActivation(ack_data);

                switch ((ACK_ACTIVE_CODE)ack_data)
                {
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_SUCCESS:
                        API_LOG(serialDevice, STATUS_LOG, "Activated successfully");

                        if (accountData.encKey != null)
                            setKey(accountData.encKey);
                        return;
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_NEW_DEVICE:
                        API_LOG(serialDevice, STATUS_LOG, "New device, please link DJIGO to your " +
                            "remote controller and try again");
                        break;
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_PARAMETER_ERROR:
                        API_LOG(serialDevice, ERROR_LOG, "Wrong parameter");
                        break;
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_ENCODE_ERROR:
                        API_LOG(serialDevice, ERROR_LOG, "Encode error");
                        break;
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_APP_NOT_CONNECTED:
                        API_LOG(serialDevice, ERROR_LOG, "DJIGO not connected");
                        break;
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_NO_INTERNET:
                        API_LOG(serialDevice, ERROR_LOG, "DJIGO not " +
                            "connected to the internet");
                        break;
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_SERVER_REFUSED:
                        API_LOG(serialDevice, ERROR_LOG, "DJI server rejected " +
                            "your request, please use your SDK ID");
                        break;
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_ACCESS_LEVEL_ERROR:
                        API_LOG(serialDevice, ERROR_LOG, "Wrong SDK permission");
                        break;
                    case ACK_ACTIVE_CODE.ACK_ACTIVE_VERSION_ERROR:
                        API_LOG(serialDevice, ERROR_LOG, "SDK version did not match");
                        break;
                    default:
                        if (!decodeACKStatus(ack_data))
                        {
                            API_LOG(serialDevice, ERROR_LOG, "While calling this function");
                        }
                        break;
                }
            }
            else
            {
                API_LOG(serialDevice, ERROR_LOG,
                    "ACK is exception, session id {0},sequence {1}",
                    protocolHeader.sessionID, protocolHeader.sequenceNumber);
            }
        }


        private void sendToMobileCallback(Ptr pHeader,
            UserData userData = null)
        {
            var protocolHeader = (Header)(pHeader);

            ushort ack_data = (ushort)ACK_COMMON_CODE.ACK_COMMON_NO_RESPONSE;
            if (protocolHeader.length - EXC_DATA_SIZE <= 2)
            {
                ack_data =
                    ((pHeader + HEADER_SIZE) +
                    (protocolHeader.length - EXC_DATA_SIZE)).UInt16;
                if (!decodeACKStatus(ack_data))
                {
                    API_LOG(serialDevice, ERROR_LOG, "While calling this function");
                }
            }
            else
            {
                API_LOG(serialDevice, ERROR_LOG,
                        "ACK is exception, session id {0},sequence {1}",
                        protocolHeader.sessionID, protocolHeader.sequenceNumber);
            }
        }

        /** 
		 * MOS Protocol parsing lirbary functions. 
		 */

        /**
		 * Default MOS Protocol Parser. Calls other callback functions based on data
		 */
        //! Mobile Data Transparent Transmission Input Servicing 
        private void parseFromMobileCallback(Ptr protocolHeader,
            UserData userData = null)
        {
            var pHeader = (Header)(protocolHeader);

            ushort mobile_data_id;

            if (pHeader.length - EXC_DATA_SIZE <= 4)
            {
                mobile_data_id = protocolHeader[HEADER_SIZE + 2];

                switch (mobile_data_id)
                {
                    case 2:
                        if (obtainControlMobileCallback.callback != null)
                        {
                            obtainControlMobileCallback.callback(
                                protocolHeader, obtainControlMobileCallback.userData);
                        }
                        else
                        {
                            obtainControlMobileCMD = true;
                        }
                        break;

                    case 3:
                        if (releaseControlMobileCallback.callback != null)
                        {
                            releaseControlMobileCallback.callback(
                                protocolHeader, releaseControlMobileCallback.userData);
                        }
                        else
                        {
                            releaseControlMobileCMD = true;
                        }
                        break;

                    case 4:
                        if (activateMobileCallback.callback != null)
                        {
                            activateMobileCallback.callback(protocolHeader,
                                activateMobileCallback.userData);
                        }
                        else
                        {
                            activateMobileCMD = true;
                        }
                        break;

                    case 5:
                        if (armMobileCallback.callback != null)
                        {
                            armMobileCallback.callback(protocolHeader,
                                armMobileCallback.userData);
                        }
                        else
                        {
                            armMobileCMD = true;
                        }
                        break;

                    case 6:
                        if (disArmMobileCallback.callback != null)
                        {
                            disArmMobileCallback.callback(protocolHeader,
                                disArmMobileCallback.userData);
                        }
                        else
                        {
                            disArmMobileCMD = true;
                        }
                        break;

                    case 7:
                        if (takeOffMobileCallback.callback != null)
                        {
                            takeOffMobileCallback.callback(protocolHeader,
                                takeOffMobileCallback.userData);
                        }
                        else
                        {
                            takeOffMobileCMD = true;
                        }
                        break;

                    case 8:
                        if (landingMobileCallback.callback != null)
                        {
                            landingMobileCallback.callback(protocolHeader,
                                landingMobileCallback.userData);
                        }
                        else
                        {
                            landingMobileCMD = true;
                        }
                        break;

                    case 9:
                        if (goHomeMobileCallback.callback != null)
                        {
                            goHomeMobileCallback.callback(protocolHeader,
                                goHomeMobileCallback.userData);
                        }
                        else
                        {
                            goHomeMobileCMD = true;
                        }
                        break;

                    case 10:
                        if (takePhotoMobileCallback.callback != null)
                        {
                            takePhotoMobileCallback.callback(protocolHeader,
                                takePhotoMobileCallback.userData);
                        }
                        else
                        {
                            takePhotoMobileCMD = true;
                        }
                        break;

                    case 11:
                        if (startVideoMobileCallback.callback != null)
                        {
                            startVideoMobileCallback.callback(protocolHeader,
                                startVideoMobileCallback.userData);
                        }
                        else
                        {
                            startVideoMobileCMD = true;
                        }
                        break;

                    case 13:
                        if (stopVideoMobileCallback.callback != null)
                        {
                            stopVideoMobileCallback.callback(protocolHeader,
                                stopVideoMobileCallback.userData);
                        }
                        else
                        {
                            stopVideoMobileCMD = true;
                        }
                        break;
                    //! Advanced features: LiDAR Mapping, Collision Avoidance, Precision Missions
                    case 20:
                        startLASMapLoggingCMD = true;
                        break;
                    case 21:
                        stopLASMapLoggingCMD = true;
                        break;
                    case 24:
                        precisionMissionCMD = true;
                        break;
                    case 25:
                        precisionMissionsCollisionAvoidanceCMD = true;
                        break;
                    case 26:
                        precisionMissionsLidarMappingCMD = true;
                        break;
                    case 27:
                        precisionMissionsCollisionAvoidanceLidarMappingCMD = true;
                        break;

                    //! The next few are only polling based and do not use callbacks. See
                    //! usage in Linux Sample.
                    case 61:
                        drawCirMobileCMD = true;
                        break;
                    case 62:
                        drawSqrMobileCMD = true;
                        break;
                    case 63:
                        attiCtrlMobileCMD = true;
                        break;
                    case 64:
                        gimbalCtrlMobileCMD = true;
                        break;
                    case 65:
                        wayPointTestMobileCMD = true;
                        break;
                    case 66:
                        localNavTestMobileCMD = true;
                        break;
                    case 67:
                        globalNavTestMobileCMD = true;
                        break;
                    case 68:
                        VRCTestMobileCMD = true;
                        break;
                    case 69:
                        precisionMissionCMD = true;
                        break;
                }
            }
        }


        private void setFrequencyCallback(Ptr pHeader, UserData userData = null)
        {
            var protocolHeader = (Header)(pHeader);

            ushort ack_data = (ushort)ACK_COMMON_CODE.ACK_COMMON_NO_RESPONSE;

            if (protocolHeader.length - EXC_DATA_SIZE <= 2)
            {
                ack_data =
                    ((pHeader + HEADER_SIZE) +
                    (protocolHeader.length - EXC_DATA_SIZE)).Byte;
            }
            switch (ack_data)
            {
                case 0x0000:
                    API_LOG(serialDevice, STATUS_LOG, "Frequency set successfully");
                    break;
                case 0x0001:
                    API_LOG(serialDevice, ERROR_LOG, "Frequency parameter error");
                    break;
                default:
                    if (!decodeACKStatus(ack_data))
                    {
                        API_LOG(serialDevice, ERROR_LOG, "While calling this function");
                    }
                    break;
            }
        }

        /**
		 * Get SDK version
		 */
        public Version getFwVersion()
        {
            return versionData.fwVersion;
        }
        string getHwVersion()
        {
            return versionData.hwVersion;
        }
        string getHwSerialNum()
        {
            return versionData.hw_serial_num;
        }

        /// Open Protocol Control
        /**
		 * Get Open Protocol packet information.
		 */
        SDKFilter getFilter()
        {
            return filter;
        }

        private void setControlCallback(Ptr pHeader,
            UserData userData = null)
        {
            var protocolHeader = (Header)(pHeader);

            ushort ack_data = (ushort)ACK_COMMON_CODE.ACK_COMMON_NO_RESPONSE;
            var data = new byte[] { 0x1 };

            if (protocolHeader.length - EXC_DATA_SIZE <= 2)
            {
                ack_data =
                    ((pHeader + HEADER_SIZE) +
                    (protocolHeader.length - EXC_DATA_SIZE)).UInt16;
            }
            else
            {
                API_LOG(serialDevice, ERROR_LOG,
                    "ACK is exception, session id {0},sequence {1}",
                    protocolHeader.sessionID, protocolHeader.sequenceNumber);
            }

            switch ((ACK_SETCONTROL_CODE)ack_data)
            {
                case ACK_SETCONTROL_CODE.ACK_SETCONTROL_ERROR_MODE:
                    if (versionData.fwVersion < new Version(3, 2, 0, 0))
                    {
                        API_LOG(serialDevice, STATUS_LOG,
                            "Obtain control failed: switch to F mode");
                    }
                    else
                    {
                        API_LOG(serialDevice, STATUS_LOG,
                            "Obtain control failed: switch to P mode");
                    }
                    break;
                case ACK_SETCONTROL_CODE.ACK_SETCONTROL_RELEASE_SUCCESS:
                    API_LOG(serialDevice, STATUS_LOG, "Released control successfully");
                    break;
                case ACK_SETCONTROL_CODE.ACK_SETCONTROL_OBTAIN_SUCCESS:
                    API_LOG(serialDevice, STATUS_LOG, "Obtained control successfully");
                    break;
                case ACK_SETCONTROL_CODE.ACK_SETCONTROL_OBTAIN_RUNNING:
                    API_LOG(serialDevice, STATUS_LOG, "Obtain control running");
                    send(2, isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_SETCONTROL,
                        data, 1, 500, 2, setControlCallback);
                    break;
                case ACK_SETCONTROL_CODE.ACK_SETCONTROL_RELEASE_RUNNING:
                    API_LOG(serialDevice, STATUS_LOG, "Release control running");
                    data[0] = 0;
                    send(2, isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_SETCONTROL,
                        data, 1, 500, 2, setControlCallback);
                    break;
                case ACK_SETCONTROL_CODE.ACK_SETCONTROL_IOC:
                    API_LOG(serialDevice, STATUS_LOG,
                    "IOC mode opening can not obtain control");
                    break;
                default:
                    if (!decodeACKStatus(ack_data))
                    {
                        API_LOG(serialDevice, ERROR_LOG, "While calling this function");
                    }
                    break;
            }
        }
    }
}