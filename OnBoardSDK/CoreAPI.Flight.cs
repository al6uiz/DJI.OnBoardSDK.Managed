using System;

namespace DJI.OnBoardSDK
{
    partial class Flight
    {
        private CoreAPI api;
        private TaskData taskData = new TaskData();

        public Flight(CoreAPI ControlAPI)
        {
            api = ControlAPI;
# if USE_SIMULATION  //! @note This functionality is not supported in this release.  
            simulating = 0;
#endif // USE_SIMULATION
        }

        public CoreAPI getApi() { return api; }

        public void setApi(CoreAPI value) { api = value; }

#if USE_SIMULATION   //! @note This functionality is not supported in this release.  
    bool isSimulating() {return simulating; }
void setSimulating(bool value) { simulating = value; }
#endif // USE_SIMULATION

        public void task(FlightTask taskname, CallBack TaskCallback, UserData userData)
        {
            taskData.cmdData = (byte)taskname;
            taskData.cmdSequence++;
            api.send(2, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_TASK, taskData.Buffer, TaskData.TypeSize,
                100, 3, TaskCallback ?? taskCallback, userData);
        }

        public ushort task(FlightTask taskname, int timeout)
        {
            taskData.cmdData = (byte)taskname;
            taskData.cmdSequence++;

            api.send(2, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_TASK, taskData.Buffer, TaskData.TypeSize,
                100, 3, null, null);
            api.serialDevice.lockACK();
            api.serialDevice.wait(timeout);
            api.serialDevice.freeACK();
            return api.missionACKUnion.simpleACK.ack;
        }

        public void setArm(bool enable, CallBack ArmCallback, UserData userData)
        {
            byte data = (byte)(enable ? 1 : 0);
            api.send(2, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_SETARM, new byte[] { data }, 1, 0, 1,
                ArmCallback ?? armCallback, userData);
        }

        public ushort setArm(bool enable, int timeout)
        {
            byte data = (byte)(enable ? 1 : 0);
            api.send(2, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_SETARM, new byte[data], 1, 10, 10, null, null);


            api.serialDevice.lockACK();
            api.serialDevice.wait(timeout);
            api.serialDevice.freeACK();

            return api.missionACKUnion.simpleACK.ack;
        }

        public void control(byte flag, float x, float y, float z, float yaw)
        {
            FlightData data = new FlightData();
            data.flag = flag;
            data.x = x;
            data.y = y;
            data.z = z;
            data.yaw = yaw;
            setFlight(data);
        }


        public void setMovementControl(byte flag, float x, float y, float z, float yaw)
        {
            FlightData data = new FlightData();
            data.flag = flag;
            data.x = x;
            data.y = y;
            data.yaw = yaw;
            if (api.getFwVersion() > new Version(3, 2, 0, 0) && api.getFwVersion() < new Version(3, 2, 15, 39))
            {
                if ((flag & (1 << 4)) != 0)
                {
                    if (api.getBroadcastData().pos.health > 3)
                    {
                        if (api.homepointAltitude != 999999)
                        {
                            data.z = z + api.homepointAltitude;
                            api.send(0, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_CONTROL, data.AllocPointer, data.TypeSize);
                        }
                    }
                    else
                    {
                        CoreAPI.API_LOG(api.getDriver(), CoreAPI.STATUS_LOG, "Not enough GPS locks, cannot run Movement Control \n");
                    }
                }
            }
            else if (api.getFwVersion() == new Version(3, 2, 100, 0))
            {
                if ((flag & (1 << 4)) != 0)
                {
                    if (api.getBroadcastData().pos.health > 3)
                    {
                        if (api.homepointAltitude != 999999)
                        {
                            data.z = z + api.homepointAltitude;
                            api.send(0, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_CONTROL, data.AllocPointer, data.TypeSize);
                        }
                    }
                    else
                    {
                        CoreAPI.API_LOG(api.getDriver(), CoreAPI.STATUS_LOG, "Not enough GPS locks, cannot run Movement Control \n");
                    }
                }
            }
            else
            {
                data.z = z;
                api.send(0, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_CONTROL, data.AllocPointer, data.TypeSize);
            }
        }


        public void setFlight(FlightData data)
        {
            api.send(0, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CONTROL_CODE.CODE_CONTROL, data.AllocPointer, data.TypeSize);
        }

        internal QuaternionData getQuaternion()
        {
#if USE_SIMULATION
  if (simulating)
  {
    QuaternionData ans;
    //! @todo better physical model

    return ans;
  }
  else
#endif // USE_SIMULATION
            return api.getBroadcastData().q;
        }

        internal EulerAngle getEulerAngle() { return toEulerAngle(api.getBroadcastData().q); }

        internal PositionData getPosition() { return api.getBroadcastData().pos; }

        internal VelocityData getVelocity() { return api.getBroadcastData().v; }

        //! @warning The return type for getAcceleration will change to Vector3fData in a future release
        internal Vector3fData getAcceleration() { return api.getBroadcastData().a; }
        //! @warning The return type for getYawRate will change to Vector3fData in a future release
        internal Vector3fData getYawRate() { return api.getBroadcastData().w; }

        //! @warning old interface. Will be replaced by MagData getMagData() in the next release.   
        internal MagData getMagnet() { return api.getBroadcastData().mag; }

        internal FlightDevice getControlDevice()
        {
            return (FlightDevice)api.getBroadcastData().ctrlInfo.deviceStatus;
        }

        internal FlightStatus getStatus()
        {
            return (FlightStatus)api.getBroadcastData().status;
        }

        internal FlightMode getControlMode()
        {
            if (api.getFwVersion() != Version.M100_23)
                return (FlightMode)api.getBroadcastData().ctrlInfo.mode;
            return FlightMode.MODE_NOT_SUPPORTED;
        }

        internal double getYaw()
        {
#if USE_SIMULATION
  if (simulating)
    return AngularSim.yaw;
  else
#endif // USE_SIMULATION
            return toEulerAngle(api.getBroadcastData().q).yaw;
        }

        internal double getRoll()
        {
#if USE_SIMULATION
  if (simulating)
    return AngularSim.roll;
  else
#endif // USE_SIMULATION
            return toEulerAngle(api.getBroadcastData().q).roll;
        }

        internal double getPitch()
        {
#if USE_SIMULATION
  if (simulating)
    return AngularSim.pitch;
  else
#endif // USE_SIMULATION
            return toEulerAngle(api.getBroadcastData().q).pitch;
        }

        private void armCallback(Ptr pHeader, UserData userData = null)
        {
            Header protocolHeader = (Header)pHeader;

            ushort ack_data;
            if (protocolHeader.length - CoreAPI.EXC_DATA_SIZE <= 2)
            {
                ack_data = (pHeader + CoreAPI.HEADER_SIZE).UInt16;

                switch ((ACK_ARM_CODE)ack_data)
                {
                    case ACK_ARM_CODE.ACK_ARM_SUCCESS:
                    CoreAPI.API_LOG(api.getDriver(), CoreAPI.STATUS_LOG, "Success,0x000%x\n", ack_data);
                    break;
                    case ACK_ARM_CODE.ACK_ARM_NEED_CONTROL:
                    CoreAPI.API_LOG(api.getDriver(), CoreAPI.STATUS_LOG, "Need to obtain control first, 0x000%x\n", ack_data);
                    break;
                    case ACK_ARM_CODE.ACK_ARM_ALREADY_ARMED:
                    CoreAPI.API_LOG(api.getDriver(), CoreAPI.STATUS_LOG, "Already done, 0x000%x\n", ack_data);
                    break;
                    case ACK_ARM_CODE.ACK_ARM_IN_AIR:
                    CoreAPI.API_LOG(api.getDriver(), CoreAPI.STATUS_LOG, "Cannot execute while in air, 0x000%x\n", ack_data);
                    break;
                }
            }
            else
            {
                CoreAPI.API_LOG(api.getDriver(), CoreAPI.ERROR_LOG, "ACK is exception,session id %d,sequence %d\n",
                    protocolHeader.sessionID, protocolHeader.sequenceNumber);
            }
        }

        private void taskCallback(Ptr pHeader, UserData userData)
        {
            Header protocolHeader = (Header)pHeader;

            ushort ack_data;
            if (protocolHeader.length - CoreAPI.EXC_DATA_SIZE <= 2)
            {
                ack_data = (pHeader + CoreAPI.HEADER_SIZE).UInt16;
                CoreAPI.API_LOG(api.getDriver(), CoreAPI.STATUS_LOG, "Task running successfully,%d\n", ack_data);
            }
            else
            {
                CoreAPI.API_LOG(api.getDriver(), CoreAPI.ERROR_LOG, "ACK is exception,session id %d,sequence %d\n",
                    protocolHeader.sessionID, protocolHeader.sequenceNumber);
            }
        }

        //! @ deprecated Use toEulerAngle instead. 
        public static EulerAngle toEulerianAngle(QuaternionData data)
        {
            EulerAngle ans;

            double q2sqr = data.q2 * data.q2;
            double t0 = -2.0 * (q2sqr + data.q3 * data.q3) + 1.0;
            double t1 = +2.0 * (data.q1 * data.q2 + data.q0 * data.q3);
            double t2 = -2.0 * (data.q1 * data.q3 - data.q0 * data.q2);
            double t3 = +2.0 * (data.q2 * data.q3 + data.q0 * data.q1);
            double t4 = -2.0 * (data.q1 * data.q1 + q2sqr) + 1.0;

            t2 = t2 > 1.0 ? 1.0 : t2;
            t2 = t2 < -1.0 ? -1.0 : t2;

            ans.pitch = Math.Asin(t2);
            ans.roll = Math.Atan2(t3, t4);
            ans.yaw = Math.Atan2(t1, t0);

            return ans;
        }


        public static EulerAngle toEulerAngle(QuaternionData quaternionData)
        {
            EulerAngle ans;

            double q2sqr = quaternionData.q2 * quaternionData.q2;
            double t0 = -2.0 * (q2sqr + quaternionData.q3 * quaternionData.q3) + 1.0;
            double t1 = +2.0 * (quaternionData.q1 * quaternionData.q2 + quaternionData.q0 * quaternionData.q3);
            double t2 = -2.0 * (quaternionData.q1 * quaternionData.q3 - quaternionData.q0 * quaternionData.q2);
            double t3 = +2.0 * (quaternionData.q2 * quaternionData.q3 + quaternionData.q0 * quaternionData.q1);
            double t4 = -2.0 * (quaternionData.q1 * quaternionData.q1 + q2sqr) + 1.0;

            t2 = t2 > 1.0 ? 1.0 : t2;
            t2 = t2 < -1.0 ? -1.0 : t2;

            ans.pitch = Math.Asin(t2);
            ans.roll = Math.Atan2(t3, t4);
            ans.yaw = Math.Atan2(t1, t0);

            return ans;
        }

        public static Quaternion toQuaternion(EulerAngle eulerAngleData)
        {
            Quaternion ans;
            double t0 = Math.Cos(eulerAngleData.yaw * 0.5);
            double t1 = Math.Sin(eulerAngleData.yaw * 0.5);
            double t2 = Math.Cos(eulerAngleData.roll * 0.5);
            double t3 = Math.Sin(eulerAngleData.roll * 0.5);
            double t4 = Math.Cos(eulerAngleData.pitch * 0.5);
            double t5 = Math.Sin(eulerAngleData.pitch * 0.5);

            ans.q0 = (float)(t2 * t4 * t0 + t3 * t5 * t1);
            ans.q1 = (float)(t3 * t4 * t0 - t2 * t5 * t1);
            ans.q2 = (float)(t2 * t5 * t0 + t3 * t4 * t1);
            ans.q3 = (float)(t2 * t4 * t1 - t3 * t5 * t0);
            return ans;
        }
    }
}
