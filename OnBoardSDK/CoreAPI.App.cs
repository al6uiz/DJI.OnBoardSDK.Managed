namespace DJI.OnBoardSDK
{
    partial class CoreAPI
    {

        void passData(ushort flag, ref ushort enable, NativeObject data, Ptr pBuf,
            int sizeDelta, ref int offset)
        {
            //! @todo new algorithm
            if ((flag & enable) != 0)
            {
                data.ReadFrom((pBuf + offset), data.TypeSize + sizeDelta);

                offset += (data.TypeSize + sizeDelta);
            }
            enable <<= 1;
        }

        CMD_SET getCmdSet(Ptr protocolHeader)
        {
            var ptemp = protocolHeader + HEADER_SIZE;
            return (CMD_SET)ptemp.Byte;
        }

        byte getCmdCode(Ptr protocolHeader)
        {
            var ptemp = protocolHeader + HEADER_SIZE;
            ptemp++;
            return ptemp.Byte;
        }

        /**Get broadcasted data values from flight controller.*/
        public BroadcastData getBroadcastData()
        {
            serialDevice.lockMSG();
            var data = broadcastData.Clone();
            serialDevice.freeMSG();
            return data;
        }

        /**
		 * Get battery capacity.
		 *
		 * @note
		 * Flight missions will not perform if battery capacity is under %50. If battery capacity
		 * drops below %50 during a flight mission, aircraft will automatically "go home".
		 *
		 */
        public BatteryData getBatteryCapacity() { return getBroadcastData().battery; }


        public CtrlInfoData getCtrlInfo() { return getBroadcastData().ctrlInfo; }

        public void setBroadcastFrameStatus(bool isFrame)
        {
            broadcastFrameStatus = isFrame;
        }

        public bool getBroadcastFrameStatus()
        {
            return broadcastFrameStatus;
        }

        static FlightStatus currentState = 0;
        static FlightStatus prevState = 0;
        static int counter = 0;
        void broadcast(Ptr protocolHeader)
        {
            if (versionData.hwVersion == null)
            {
                return;
            }

            Ptr pdata = protocolHeader + HEADER_SIZE;

            serialDevice.lockMSG();
            pdata += 2;
            var enableFlag = pdata.UInt16;
            broadcastData.dataFlag = enableFlag;
            int len = MSG_ENABLE_FLAG_LEN;

            //! @warning Change to const (+change interface for passData) in next release
            ushort DATA_FLAG = 0x0001;
            //! @todo better algorithm

            /** 
			 *@note Write activation status
			 *
			 * Default value: 0xFF
			 * Activation successful: 0x00
			 * Activation error codes: 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
			 * (see DJI_API.h for detailed description of the error codes)
			 */
            broadcastData.activation = ack_activation;

            if (versionData.fwVersion > new Version(3, 1, 0, 0))
                passData(enableFlag, ref DATA_FLAG, broadcastData.timeStamp, pdata, 0,
                    ref len);
            else
                passData(enableFlag, ref DATA_FLAG, broadcastData.timeStamp.time, pdata, 0,
                    ref len);

            passData(enableFlag, ref DATA_FLAG, broadcastData.q, pdata, 0, ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.a, pdata, 0, ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.v, pdata, 0, ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.w, pdata, 0, ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.pos, pdata, 0, ref len);

            if (versionData.hwVersion != "M100") //! N3/A3/M600
            {
                passData(enableFlag, ref DATA_FLAG, broadcastData.gps, pdata, 0, ref len);
                passData(enableFlag, ref DATA_FLAG, broadcastData.rtk, pdata, 0, ref len);
                if (((enableFlag) & 0x0040) != 0)
                    API_LOG(serialDevice, RTK_LOG, "receive GPS data {0}", serialDevice.getTimeStamp());
                if (((enableFlag) & 0x0080) != 0)
                    API_LOG(serialDevice, RTK_LOG, "receive RTK data {0}", serialDevice.getTimeStamp());
            }
            passData(enableFlag, ref DATA_FLAG, broadcastData.mag, pdata, 0, ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.rc, pdata, 0, ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.gimbal, pdata,
                -((versionData.fwVersion < new Version(3, 1, 0, 0)) ? 1 : 0), ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.status, pdata, 0, ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.battery, pdata, 0, ref len);
            passData(enableFlag, ref DATA_FLAG, broadcastData.ctrlInfo, pdata,
                -((versionData.fwVersion < new Version(3, 1, 0, 0)) ? 1 : 0), ref len);
            serialDevice.freeMSG();
            /**
			 * Set broadcast frame status
			 * @todo Implement proper notification mechanism
			 */
            setBroadcastFrameStatus(true);

            //! State Machine for MSL Altitude bug in A3 and M600
            //! Handles the case if users start OSDK after arming aircraft (STATUS_ON_GROUND)/after takeoff (STATUS_IN_AIR)
            //! Transition from STATUS_MOTOR_STOPPED to STATUS_ON_GROUND can be seen with Takeoff command with 1hz flight status data
            //! Transition from STATUS_ON_GROUND to STATUS_MOTOR_STOPPED can be seen with Landing command only for frequencies >= 50Hz
            if (getHwVersion() != "M100")
            {
                //! Only runs if Flight status is available
                if ((enableFlag & (1 << 11)) != 0)
                {
                    if (getBroadcastData().pos.health > 3)
                    {
                        if (getFlightStatus() != currentState)
                        {
                            prevState = currentState;
                            currentState = getFlightStatus();
                            if (prevState == FlightStatus.STATUS_MOTOR_OFF && currentState == FlightStatus.STATUS_GROUND_STANDBY)
                            {
                                homepointAltitude = getBroadcastData().pos.altitude;
                            }
                            if (prevState == FlightStatus.STATUS_SKY_STANDBY && currentState == FlightStatus.STATUS_GROUND_STANDBY)
                            {
                                homepointAltitude = getBroadcastData().pos.altitude;
                            }
                            //! This case would exist if the user starts OSDK after take off.
                            else if (prevState == FlightStatus.STATUS_MOTOR_OFF && currentState == FlightStatus.STATUS_SKY_STANDBY)
                            {
                                homepointAltitude = 999999;
                            }
                        }
                    }
                    else
                    {
                        homepointAltitude = 999999;
                    }
                }
            }
            if (broadcastCallback.callback != null)
                broadcastCallback.callback(protocolHeader, broadcastCallback.userData);
        }

        private byte[] bufRecvReqData = new byte[100];

        public int LastSequence { get; private set; }
        public int ErrorCount { get; private set; }

        void recvReqData(Ptr pHeader)
        {
            var protocolHeader = (Header)(pHeader);
            var buf = (Ptr)bufRecvReqData;

            var ack = (pHeader + HEADER_SIZE + 2).Byte;
            if (getCmdSet(pHeader) == CMD_SET.SET_BROADCAST)
            {
                switch ((BROADCAST_CODE)getCmdCode(pHeader))
                {
                    case BROADCAST_CODE.CODE_BROADCAST:
                    {
                        broadcast(pHeader);
                        break;
                    }
                    case BROADCAST_CODE.CODE_FROMMOBILE:
                    {
                        API_LOG(serialDevice, STATUS_LOG, "Receive data from mobile");
                        if (fromMobileCallback.callback != null)
                        {
                            fromMobileCallback.callback(pHeader, fromMobileCallback.userData);
                        }
                        else
                        {
                            parseFromMobileCallback(pHeader);
                        }
                        break;
                    }
                    case BROADCAST_CODE.CODE_LOSTCTRL:
                    {
                        API_LOG(serialDevice, STATUS_LOG, "onboardSDK lost control");
                        Ack param = new Ack();
                        if (protocolHeader.sessionID > 0)
                        {
                            buf[0] = buf[1] = 0;
                            param.sessionID = protocolHeader.sessionID;
                            param.seqNum = protocolHeader.sequenceNumber;
                            param.encrypt = protocolHeader.enc;
                            param.buf = buf;
                            param.length = 2;
                            ackInterface(ref param);
                        }
                        break;
                    }
                    case BROADCAST_CODE.CODE_MISSION:
                    {
                        //! @todo add mission session decode
                        if (missionCallback.callback != null)
                        {
                            missionCallback.callback(pHeader, missionCallback.userData);
                        }
                        else
                        {
                            switch ((MISSION_TYPE)ack)
                            {
                                case MISSION_TYPE.MISSION_MODE_A:
                                break;
                                case MISSION_TYPE.MISSION_WAYPOINT:
                                if (wayPointData)
                                {
                                    if (wayPointCallback.callback != null)
                                        wayPointCallback.callback(pHeader,
                                            wayPointCallback.userData);
                                    else
                                        API_LOG(serialDevice, STATUS_LOG, "Mode waypoint ");
                                }
                                break;
                                case MISSION_TYPE.MISSION_HOTPOINT:
                                if (hotPointData)
                                {
                                    if (hotPointCallback.callback != null)
                                        hotPointCallback.callback(pHeader,
                                            hotPointCallback.userData);
                                    else
                                        API_LOG(serialDevice, STATUS_LOG, "Mode HP ");
                                }
                                break;
                                case MISSION_TYPE.MISSION_FOLLOW:
                                if (followData)
                                {
                                    if (followCallback.callback != null)
                                        followCallback.callback(pHeader,
                                            followCallback.userData);
                                    else
                                        API_LOG(serialDevice, STATUS_LOG, "Mode Follow ");
                                }
                                break;
                                case MISSION_TYPE.MISSION_IOC:
                                //! @todo compare IOC with other mission modes comprehensively
                                API_LOG(serialDevice, STATUS_LOG, "Mode IOC ");
                                break;
                                default:
                                API_LOG(serialDevice, ERROR_LOG, "Unknown mission code 0x{0:X} ", ack);
                                break;
                            }
                        }
                        break;
                    }
                    case BROADCAST_CODE.CODE_WAYPOINT:
                    {
                        //! @todo add waypoint session decode
                        if (wayPointEventCallback.callback != null)
                            wayPointEventCallback.callback(pHeader,
                                wayPointEventCallback.userData);
                        else
                            API_LOG(serialDevice, STATUS_LOG, "WAYPOINT DATA");
                        break;
                    }
                    default:
                    API_LOG(serialDevice, STATUS_LOG, "Unknown BROADCAST command code");
                    break;
                }
            }
            else
                API_LOG(serialDevice, DEBUG_LOG, "Received unknown command");
            if (recvCallback.callback != null)
                recvCallback.callback(pHeader, recvCallback.userData);
        }

        public void setBroadcastCallback(CallBack userCallback, UserData userData)
        {
            broadcastCallback.callback = userCallback;
            broadcastCallback.userData = userData;
        }

        public void setFromMobileCallback(CallBack userCallback, UserData userData)
        {
            fromMobileCallback.callback = userCallback;
            fromMobileCallback.userData = userData;
        }
    }
}
