using System;
using System.Collections;

namespace DJI.OnBoardSDK
{
    partial class CoreAPI
    {
        void sendData(Ptr buf)
        {
            int ans;
            Header pHeader = (Header)(buf);

#if API_TRACE_DATA
            printFrame(serialDevice, buf, true);
#endif

            ans = serialDevice.send(buf.Buffer, buf.Offset, pHeader.length);
            if (ans == 0)
            {
                API_LOG(serialDevice, STATUS_LOG, "Port not send");
            }
            if (ans == -1)
            {
                API_LOG(serialDevice, ERROR_LOG, "Port closed");
            }
        }


        private Hashtable _map = new Hashtable();

        private void appHandler(Ptr pBuf)
        {
#if API_TRACE_DATA
            printFrame(serialDevice, pBuf, false);
#endif

            var protocolHeader = (Header)(pBuf);

            int lastSequence = 0;
            var error = 0;
            if (_map.Contains(protocolHeader.sessionID))
            {
                lastSequence = (int)_map[protocolHeader.sessionID];
                error = (protocolHeader.sequenceNumber - lastSequence) - 1;
            }


            _map[protocolHeader.sessionID] = (int)protocolHeader.sequenceNumber;

            if (error > 0)
            {
                ErrorCount += (error);
            }

            LastSequence = protocolHeader.sequenceNumber;

            Header p2protocolHeader;


            if (protocolHeader.isAck)
            {
                if (protocolHeader.sessionID > 1 && protocolHeader.sessionID < 32)
                {
                    serialDevice.lockMemory();
                    var usageFlag = CMDSessionTab[protocolHeader.sessionID].usageFlag;
                    if (usageFlag == true)
                    {
                        p2protocolHeader =
                            (Header)(CMDSessionTab[protocolHeader.sessionID].mmu.pmem);
                        if (p2protocolHeader.sessionID == protocolHeader.sessionID &&
                            p2protocolHeader.sequenceNumber == protocolHeader.sequenceNumber)
                        {
                            API_LOG(serialDevice, DEBUG_LOG, "Recv Session {0} ACK",
                                p2protocolHeader.sessionID);

                            callBack = CMDSessionTab[protocolHeader.sessionID].handler;
                            data = CMDSessionTab[protocolHeader.sessionID].userData;
                            freeSession(CMDSessionTab[protocolHeader.sessionID]);
                            serialDevice.freeMemory();

                            if (callBack != null)
                            {
                                //! Non-blocking callback thread
                                if (nonBlockingCBThreadEnable == true)
                                {
                                    notifyNonBlockingCaller(pBuf);
                                }
                                else if (nonBlockingCBThreadEnable == false)
                                {
                                    callBack(pBuf, data);
                                }
                            }
                            else
                            {
                                // Notify caller end of ACK frame arrived
                                notifyCaller(pBuf);
                            }

                            /**
							 * Set end of ACK frame
							 * @todo Implement proper notification mechanism
							 */
                            setACKFrameStatus(
                                (CMDSessionTab[protocolHeader.sessionID]).usageFlag ? (byte)1 : (byte)0);
                        }
                        else
                        {
                            serialDevice.freeMemory();
                        }
                    }
                    else
                    {
                        serialDevice.freeMemory();
                        API_LOG(serialDevice, ERROR_LOG, "Session not used");
                    }
                }
            }
            else
            {
                switch (protocolHeader.sessionID)
                {
                    case 0:
                    {
                        recvReqData(pBuf);
                        break;
                    }

                    case 1:
                    //! @todo unnecessary ack in case 1. Maybe add code later
                    //! @todo check algorithm
                    default: //! @note session id is 2
                    {
                        API_LOG(serialDevice, STATUS_LOG, "ACK {0}", protocolHeader.sessionID);

                        if (ACKSessionTab[protocolHeader.sessionID - 1].sessionStatus ==
                            ACK_SESSION_PROCESS)
                        {
                            API_LOG(serialDevice, DEBUG_LOG, "This session is waiting for App ACK: session id={0},seq_num={1}",
                                protocolHeader.sessionID, protocolHeader.sequenceNumber);
                        }
                        else if (ACKSessionTab[protocolHeader.sessionID - 1].sessionStatus ==
                            ACK_SESSION_IDLE)
                        {
                            if (protocolHeader.sessionID > 1)
                                ACKSessionTab[protocolHeader.sessionID - 1].sessionStatus =
                                    ACK_SESSION_PROCESS;
                            recvReqData(pBuf);
                        }
                        else if (ACKSessionTab[protocolHeader.sessionID - 1].sessionStatus ==
                            ACK_SESSION_USING)
                        {
                            serialDevice.lockMemory();

                            p2protocolHeader =
                             (Header)(ACKSessionTab[protocolHeader.sessionID - 1].mmu.pmem);
                            if (p2protocolHeader.sequenceNumber ==
                                protocolHeader.sequenceNumber)
                            {
                                API_LOG(serialDevice, DEBUG_LOG, "Repeat ACK to remote,session " +
                                    "id={0},seq_num={1}",
                                    protocolHeader.sessionID, protocolHeader.sequenceNumber);
                                sendData(ACKSessionTab[protocolHeader.sessionID - 1].mmu.pmem);
                                serialDevice.freeMemory();
                            }
                            else
                            {
                                API_LOG(serialDevice, DEBUG_LOG,
                                    "Same session,but new seq_num pkg,session id={0}," +
                                    "pre seq_num={1},cur seq_num={2}",
                                    protocolHeader.sessionID, p2protocolHeader.sequenceNumber,
                                    protocolHeader.sequenceNumber);
                                ACKSessionTab[protocolHeader.sessionID - 1].sessionStatus =
                                    ACK_SESSION_PROCESS;
                                serialDevice.freeMemory();
                                recvReqData(pBuf);
                            }
                        }
                        break;
                    }
                }
            }
        }

        //! Notify caller ACK frame arrived
        void allocateACK(Ptr pBuf)
        {
            var protocolHeader = (Header)(pBuf);

            if (protocolHeader.length <= MAX_ACK_SIZE)
            {
                var pSource = pBuf + HEADER_SIZE;
                Array.Copy(pSource.Buffer, pSource.Offset, missionACKUnion.raw_ack_array, 0,
                    protocolHeader.length - EXC_DATA_SIZE);
            }
            else
            {
#if !STM32
                throw new Exception("Unknown ACK");
#endif
            }
        }

        void notifyCaller(Ptr protocolHeader)
        {
            serialDevice.lockACK();

            allocateACK(protocolHeader);

            // Notify caller end of ACK frame arrived
            serialDevice.notify();
            serialDevice.freeACK();
        }

        void notifyNonBlockingCaller(Ptr protocolHeader)
        {

            serialDevice.lockNonBlockCBAck();
            //! This version of non-blocking can be limited in performance since the
            //! read thread waits for the callback thread to return before the read thread 
            //! continues.

            allocateACK(protocolHeader);

            //! Copying protocol header to a global variable - will be passed to the 
            //! Callback thread.
            //! protHeader is not thread safe and is passed to Callback for legacy 
            //! purposes.
            //! Ack is available in the callback via MissionACKUnion.
            protHeader = protocolHeader;
            serialDevice.freeNonBlockCBAck();

            serialDevice.lockProtocolHeader();
            serialDevice.notifyNonBlockCBAckRecv();
            serialDevice.freeProtocolHeader();
        }

        public void sendPoll()
        {
            byte i;
            long curTimestamp;
            for (i = 1; i < SESSION_TABLE_NUM; i++)
            {
                if (CMDSessionTab[i].usageFlag == true)
                {
                    curTimestamp = serialDevice.getTimeStamp();
                    if ((curTimestamp - CMDSessionTab[i].preTimestamp) >
                        CMDSessionTab[i].timeout)
                    {
                        serialDevice.lockMemory();
                        if (CMDSessionTab[i].retry > 0)
                        {
                            if (CMDSessionTab[i].sent >= CMDSessionTab[i].retry)
                            {
                                API_LOG(serialDevice, DEBUG_LOG, "Free session {0}",
                                    CMDSessionTab[i].sessionID);

                                freeSession(CMDSessionTab[i]);
                            }
                            else
                            {
                                API_LOG(serialDevice, DEBUG_LOG, "Retry session {0}",
                                    CMDSessionTab[i].sessionID);
                                sendData(CMDSessionTab[i].mmu.pmem);
                                CMDSessionTab[i].preTimestamp = curTimestamp;
                                CMDSessionTab[i].sent++;
                            }
                        }
                        else
                        {
                            API_LOG(serialDevice, DEBUG_LOG, "Send once {0}", i);
                            sendData(CMDSessionTab[i].mmu.pmem);
                            CMDSessionTab[i].preTimestamp = curTimestamp;
                        }
                        serialDevice.freeMemory();
                    }
                    //else
                    //{
                    //    API_LOG(serialDevice, DEBUG_LOG, "Timeout Session: {0} ({1} > {2})", i, curTimestamp - CMDSessionTab[i].preTimestamp, CMDSessionTab[i].timeout);
                    //}
                }
            }
            //! @note Add auto resendpoll
        }

        byte[] bufReadPoll = new byte[BUFFER_SIZE];

        public void readPoll()
        {
            int read_len;
            read_len = serialDevice.readall(bufReadPoll, bufReadPoll.Length);

#if API_BUFFER_DATA
			onceRead = read_len;
			totalRead += onceRead;
#endif // API_BUFFER_DATA
            for (int i = 0; i < read_len; i++)
            {
                byteHandler(bufReadPoll[i]);
            }
        }

        //! @todo Implement callback poll here
        void callbackPoll()
        {
            serialDevice.lockNonBlockCBAck();
            serialDevice.nonBlockWait();
            //! The protHeader is being passed to the Callback function for legacy 
            //! purposes and is not thread safe.
            //! Ack is already avaialble to you in the callback via the mission ACK Union.
            callBack(protHeader, data);
            serialDevice.freeNonBlockCBAck();
        }

        void setup()
        {
            setupMMU();
            setupSession();
        }

        void setKey(byte[] key)
        {
            Array.Copy(key, filter.sdkKey, 32);
            filter.encode = 1;
        }

        /// Activation Control
        /**
		 * @brief
		 * Is your aircraft already activated ?
		 */
        public void setActivation(bool isActivated)
        {
            serialDevice.lockMSG();
            if (isActivated)
            {
                broadcastData.activation = ACK_ACTIVE_CODE.ACK_ACTIVE_PARAMETER_ERROR;
            }
            else
            {
                broadcastData.activation = ACK_ACTIVE_CODE.ACK_ACTIVE_SUCCESS;
            }
            serialDevice.freeMSG();
        }


        /*
		 * Let user know when ACK and Broadcast messages processed
		 */
        void setACKFrameStatus(byte usageFlag)
        {
            ackFrameStatus = usageFlag;
        }

        byte getACKFrameStatus()
        {
            return ackFrameStatus;
        }

        void setSyncFreq(uint freqInHz)
        {
            send(0, true, CMD_SET.SET_SYNC, (byte)SYNC_CODE.CODE_SYNC_BROADCAST, BitConverter.GetBytes(freqInHz), 4);
        }

        int calculateLength(int size, bool encrypt_flag)
        {
            int len;
            if (encrypt_flag)
                len = size + HEADER_SIZE + 4 + (16 - size % 16);
            else
                len = size + HEADER_SIZE + 4;
            return len;
        }

        int ackInterface(ref Ack parameter)
        {
            int ret = 0;
            ACKSession ack_session = null;

            if (parameter.length > PRO_PURE_DATA_MAX_SIZE)
            {
                API_LOG(serialDevice, ERROR_LOG, "length={0} is over-sized",
                    parameter.length);
                return -1;
            }

            if (parameter.sessionID == 0)
            {
                //! @note Do nothing, session 0 is a NACK session.
                return 0;
            }
            else if (parameter.sessionID > 0 && parameter.sessionID < 32)
            {
                serialDevice.lockMemory();
                ack_session =
                    allocACK(parameter.sessionID,
                    calculateLength(parameter.length, parameter.encrypt));
                if (ack_session == null)
                {
                    serialDevice.freeMemory();
                    return -1;
                }

                ret = encrypt(ack_session.mmu.pmem, parameter.buf, parameter.length, true,
                        parameter.encrypt, parameter.sessionID, parameter.seqNum);
                if (ret == 0)
                {
                    API_LOG(serialDevice, ERROR_LOG, "encrypt ERROR");
                    serialDevice.freeMemory();
                    return -1;
                }

                API_LOG(serialDevice, DEBUG_LOG, "Sending data!");
                sendData(ack_session.mmu.pmem);
                serialDevice.freeMemory();
                ack_session.sessionStatus = ACK_SESSION_USING;
                return 0;
            }

            return -1;
        }

        int sendInterface(ref Command parameter)
        {
            int ret = 0;
            CMDSession cmdSession = null;
            if (parameter.length > PRO_PURE_DATA_MAX_SIZE)
            {
                API_LOG(serialDevice, ERROR_LOG, "ERROR,length={0} is over-sized",
                    parameter.length);
                return -1;
            }

            switch (parameter.sessionMode)
            {
                case 0:
                {
                    serialDevice.lockMemory();
                    cmdSession = allocSession(
                        CMD_SESSION_0, calculateLength(parameter.length, parameter.encrypt));

                    if (cmdSession == null)
                    {
                        serialDevice.freeMemory();
                        API_LOG(serialDevice, ERROR_LOG, "ERROR,there is not enough memory");
                        return -1;
                    }
                    ret = encrypt(cmdSession.mmu.pmem, parameter.buf, parameter.length, false,
                        parameter.encrypt, cmdSession.sessionID, seq_num);
                    if (ret == 0)
                    {
                        API_LOG(serialDevice, ERROR_LOG, "encrypt ERROR");
                        freeSession(cmdSession);
                        serialDevice.freeMemory();
                        return -1;
                    }

                    API_LOG(serialDevice, DEBUG_LOG, "send data in session mode 0");

                    sendData(cmdSession.mmu.pmem);
                    seq_num++;
                    freeSession(cmdSession);
                    serialDevice.freeMemory();
                    break;
                }
                case 1:
                {
                    serialDevice.lockMemory();
                    cmdSession = allocSession(
                        CMD_SESSION_1, calculateLength(parameter.length, parameter.encrypt));
                    if (cmdSession == null)
                    {
                        serialDevice.freeMemory();
                        API_LOG(serialDevice, ERROR_LOG, "ERROR,there is not enough memory");
                        return -1;
                    }
                    if (seq_num == cmdSession.preSeqNum)
                    {
                        seq_num++;
                    }
                    ret = encrypt(cmdSession.mmu.pmem, parameter.buf, parameter.length, false,
                        parameter.encrypt, cmdSession.sessionID, seq_num);
                    if (ret == 0)
                    {
                        API_LOG(serialDevice, ERROR_LOG, "encrypt ERROR");
                        freeSession(cmdSession);
                        serialDevice.freeMemory();
                        return -1;
                    }
                    cmdSession.preSeqNum = seq_num++;

                    cmdSession.handler = parameter.handler;
                    cmdSession.userData = parameter.userData;
                    cmdSession.timeout =
                        (parameter.timeout > POLL_TICK) ? parameter.timeout : POLL_TICK;
                    cmdSession.preTimestamp = serialDevice.getTimeStamp();
                    cmdSession.sent = 1;
                    cmdSession.retry = 1;
                    API_LOG(serialDevice, DEBUG_LOG, "sending session {0}",
                        cmdSession.sessionID);
                    sendData(cmdSession.mmu.pmem);
                    serialDevice.freeMemory();
                    break;
                }
                // Case 2 is almost the same as case 1, except CMD_SESSION_AUTO and retry 
                // settings.
                case 2:
                {
                    serialDevice.lockMemory();
                    cmdSession =
                        allocSession(CMD_SESSION_AUTO,
                            calculateLength(parameter.length, parameter.encrypt));
                    if (cmdSession == null)
                    {
                        serialDevice.freeMemory();
                        API_LOG(serialDevice, ERROR_LOG, "ERROR,there is not enough memory");
                        return -1;
                    }
                    if (seq_num == cmdSession.preSeqNum)
                    {
                        seq_num++;
                    }
                    ret = encrypt(cmdSession.mmu.pmem, parameter.buf, parameter.length, false,
                        parameter.encrypt, cmdSession.sessionID, seq_num);

                    if (ret == 0)
                    {
                        API_LOG(serialDevice, ERROR_LOG, "encrypt ERROR");
                        freeSession(cmdSession);
                        serialDevice.freeMemory();
                        return -1;
                    }
                    cmdSession.preSeqNum = seq_num++;
                    cmdSession.handler = parameter.handler;
                    cmdSession.userData = parameter.userData;
                    cmdSession.timeout =
                        (parameter.timeout > POLL_TICK) ? parameter.timeout : POLL_TICK;
                    cmdSession.preTimestamp = serialDevice.getTimeStamp();
                    cmdSession.sent = 1;
                    cmdSession.retry = parameter.retry;
                    API_LOG(serialDevice, DEBUG_LOG, "Sending session {0}",
                        cmdSession.sessionID);
                    sendData(cmdSession.mmu.pmem);
                    serialDevice.freeMemory();
                    break;
                }
                default:
                {
                    API_LOG(serialDevice, ERROR_LOG, "Unknown mode:{0}",
                        parameter.sessionMode);
                    break;
                }
            }
            return 0;

        }
    }
}
