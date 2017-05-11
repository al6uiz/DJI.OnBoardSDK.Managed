using System;

namespace DJI.OnBoardSDK
{
    partial class CoreAPI
    {
        CallBack callBack;
        UserData data;
        Ptr protHeader;

        /// Activation Control
        bool nonBlockingCBThreadEnable;

        void setBroadcastCallback(CallBackHandler callback) { broadcastCallback = callback; }
        void setMisssionCallback(CallBackHandler callback) { missionCallback = callback; }
        void setHotPointCallback(CallBackHandler callback) { hotPointCallback = callback; }
        void setWayPointCallback(CallBackHandler callback) { wayPointCallback = callback; }
        void setFollowCallback(CallBackHandler callback) { followCallback = callback; }
        void setWayPointEventCallback(CallBackHandler callback) { wayPointEventCallback = callback; }

        public void setMisssionCallback(CallBack handler, UserData userData = null) { throw new NotImplementedException(); }
        public void setHotPointCallback(CallBack handler, UserData userData = null) { throw new NotImplementedException(); }
        public void setWayPointCallback(CallBack handler, UserData userData = null) { throw new NotImplementedException(); }
        public void setFollowCallback(CallBack handler, UserData userData = null) { throw new NotImplementedException(); }
        public void setWayPointEventCallback(CallBack handler, UserData userData = null) { throw new NotImplementedException(); }


        /** 
         * MOS Protocol parsing lirbary functions. 
         */

        /**
         * Default MOS Protocol Parser. Calls other callback functions based on data
         */
        /**
		 * Mobile Callback handler functions
		 */
        public void setObtainControlMobileCallback(CallBackHandler callback) { obtainControlMobileCallback = callback; }
        public void setReleaseControlMobileCallback(CallBackHandler callback) { releaseControlMobileCallback = callback; }
        public void setActivateMobileCallback(CallBackHandler callback) { activateMobileCallback = callback; }
        public void setArmMobileCallback(CallBackHandler callback) { armMobileCallback = callback; }
        public void setDisArmMobileCallback(CallBackHandler callback) { disArmMobileCallback = callback; }
        public void setTakeOffMobileCallback(CallBackHandler callback) { takeOffMobileCallback = callback; }
        public void setLandingMobileCallback(CallBackHandler callback) { landingMobileCallback = callback; }
        public void setGoHomeMobileCallback(CallBackHandler callback) { goHomeMobileCallback = callback; }
        public void setTakePhotoMobileCallback(CallBackHandler callback) { takePhotoMobileCallback = callback; }
        public void setStartVideoMobileCallback(CallBackHandler callback) { startVideoMobileCallback = callback; }
        public void setStopVideoMobileCallback(CallBackHandler callback) { stopVideoMobileCallback = callback; }

        /**
     * Flight mission decoder.
     */
        bool decodeMissionStatus(byte ack) { throw new NotImplementedException(); }

        /**
		 *@note  Thread data
		 */
        bool stopCond;

        /**
		 *@note  Thread data
		 */

        uint ack_data;
        HotPointReadACK hotpointReadACK;
        WayPointInitACK waypointInitACK;
        public MissionACKUnion missionACKUnion = new MissionACKUnion();

        /**
		 *@note Activation status to override BroadcastData activation flag
		 *
		 */
        ACK_ACTIVE_CODE ack_activation;
        /**
		 * Setters and getters for Mobile CMD variables - these are used 
		 * when interacting with a Data Transparent Transmission App 
		 */

        /** Core functions - getters */
        public bool getObtainControlMobileCMD() { return obtainControlMobileCMD; }
        public bool getReleaseControlMobileCMD() { return releaseControlMobileCMD; }
        public bool getActivateMobileCMD() { return activateMobileCMD; }
        public bool getArmMobileCMD() { return armMobileCMD; }
        public bool getDisArmMobileCMD() { return disArmMobileCMD; }
        public bool getTakeOffMobileCMD() { return takeOffMobileCMD; }
        public bool getLandingMobileCMD() { return landingMobileCMD; }
        public bool getGoHomeMobileCMD() { return goHomeMobileCMD; }
        public bool getTakePhotoMobileCMD() { return takePhotoMobileCMD; }
        public bool getStartVideoMobileCMD() { return startVideoMobileCMD; }
        public bool getStopVideoMobileCMD() { return stopVideoMobileCMD; }

        /** Custom missions - getters */
        public bool getDrawCirMobileCMD() { return drawCirMobileCMD; }
        public bool getDrawSqrMobileCMD() { return drawSqrMobileCMD; }
        public bool getAttiCtrlMobileCMD() { return attiCtrlMobileCMD; }
        public bool getGimbalCtrlMobileCMD() { return gimbalCtrlMobileCMD; }
        public bool getWayPointTestMobileCMD() { return wayPointTestMobileCMD; }
        public bool getLocalNavTestMobileCMD() { return localNavTestMobileCMD; }
        public bool getGlobalNavTestMobileCMD() { return globalNavTestMobileCMD; }
        public bool getVRCTestMobileCMD() { return VRCTestMobileCMD; }


        /** Advanced features: LiDAR Mapping, Collision Avoidance, Precision Missions */
        public bool getStartLASMapLoggingCMD() { return startLASMapLoggingCMD; }
        public bool getStopLASMapLoggingCMD() { return stopLASMapLoggingCMD; }
        public bool getPrecisionMissionsCMD() { return precisionMissionCMD; }
        public bool getPrecisionMissionsCollisionAvoidanceCMD() { return precisionMissionsCollisionAvoidanceCMD; }
        public bool getPrecisionMissionsLidarMappingCMD() { return precisionMissionsLidarMappingCMD; }
        public bool getPrecisionMissionsCollisionAvoidanceLidarMappingCMD() { return precisionMissionsCollisionAvoidanceLidarMappingCMD; }

        /** Core functions - setters */
        public void setObtainControlMobileCMD(bool userInput) { obtainControlMobileCMD = userInput; }
        public void setReleaseControlMobileCMD(bool userInput) { releaseControlMobileCMD = userInput; }
        public void setActivateMobileCMD(bool userInput) { activateMobileCMD = userInput; }
        public void setArmMobileCMD(bool userInput) { armMobileCMD = userInput; }
        public void setDisArmMobileCMD(bool userInput) { disArmMobileCMD = userInput; }
        public void setTakeOffMobileCMD(bool userInput) { takeOffMobileCMD = userInput; }
        public void setLandingMobileCMD(bool userInput) { landingMobileCMD = userInput; }
        public void setGoHomeMobileCMD(bool userInput) { goHomeMobileCMD = userInput; }
        public void setTakePhotoMobileCMD(bool userInput) { takePhotoMobileCMD = userInput; }
        public void setStartVideoMobileCMD(bool userInput) { startVideoMobileCMD = userInput; }
        public void setStopVideoMobileCMD(bool userInput) { stopVideoMobileCMD = userInput; }

        /** Advanced features: LiDAR Mapping, Collision Avoidance, Precision Missions */
        public void setStartLASMapLoggingCMD(bool userInput) { startLASMapLoggingCMD = userInput; }
        public void setStopLASMapLoggingCMD(bool userInput) { stopLASMapLoggingCMD = userInput; }
        public void setPrecisionMissionsCMD(bool userInput) { precisionMissionCMD = userInput; }
        public void setPrecisionMissionsCollisionAvoidanceCMD(bool userInput) { precisionMissionsCollisionAvoidanceCMD = userInput; }
        public void setPrecisionMissionsLidarMappingCMD(bool userInput) { precisionMissionsLidarMappingCMD = userInput; }
        public void setPrecisionMissionsCollisionAvoidanceLidarMappingCMD(bool userInput) { precisionMissionsCollisionAvoidanceLidarMappingCMD = userInput; }


        /** Custom missions - setters */
        public void setDrawCirMobileCMD(bool userInput) { drawCirMobileCMD = userInput; }
        public void setDrawSqrMobileCMD(bool userInput) { drawSqrMobileCMD = userInput; }
        public void setAttiCtrlMobileCMD(bool userInput) { attiCtrlMobileCMD = userInput; }
        public void setGimbalCtrlMobileCMD(bool userInput) { gimbalCtrlMobileCMD = userInput; }
        public void setWayPointTestMobileCMD(bool userInput) { wayPointTestMobileCMD = userInput; }
        public void setLocalNavTestMobileCMD(bool userInput) { localNavTestMobileCMD = userInput; }
        public void setGlobalNavTestMobileCMD(bool userInput) { globalNavTestMobileCMD = userInput; }
        public void setVRCTestMobileCMD(bool userInput) { VRCTestMobileCMD = userInput; }

        public float homepointAltitude;

        public IPlatformDriver serialDevice;
        private BroadcastData broadcastData = new BroadcastData();
        private byte ackFrameStatus;
        private bool broadcastFrameStatus;
        private byte[] encodeSendData = new byte[BUFFER_SIZE];
        private byte[] encodeACK = new byte[ACK_SIZE];

        //! Mobile Data Transparent Transmission - callbacks
        CallBackHandler fromMobileCallback;
        CallBackHandler broadcastCallback;
        CallBackHandler hotPointCallback;
        CallBackHandler wayPointCallback;
        CallBackHandler wayPointEventCallback;
        CallBackHandler followCallback;
        CallBackHandler missionCallback;
        CallBackHandler recvCallback;

        CallBackHandler obtainControlMobileCallback;
        CallBackHandler releaseControlMobileCallback;
        CallBackHandler activateMobileCallback;
        CallBackHandler armMobileCallback;
        CallBackHandler disArmMobileCallback;
        CallBackHandler takeOffMobileCallback;
        CallBackHandler landingMobileCallback;
        CallBackHandler goHomeMobileCallback;
        CallBackHandler takePhotoMobileCallback;
        CallBackHandler startVideoMobileCallback;
        CallBackHandler stopVideoMobileCallback;

        //! Mobile Data Transparent Transmission - flags

        //! Core functions
        bool obtainControlMobileCMD;
        bool releaseControlMobileCMD;
        bool activateMobileCMD;
        bool armMobileCMD;
        bool disArmMobileCMD;
        bool takeOffMobileCMD;
        bool landingMobileCMD;
        bool goHomeMobileCMD;
        bool takePhotoMobileCMD;
        bool startVideoMobileCMD;
        bool stopVideoMobileCMD;

        //! Custom Mission examples
        bool drawCirMobileCMD;
        bool drawSqrMobileCMD;
        bool attiCtrlMobileCMD;
        bool gimbalCtrlMobileCMD;
        bool wayPointTestMobileCMD;
        bool localNavTestMobileCMD;
        bool globalNavTestMobileCMD;
        bool VRCTestMobileCMD;


        //! Advanced features: LiDAR Mapping, Collision Avoidance, Precision Missions
        bool startLASMapLoggingCMD;
        bool stopLASMapLoggingCMD;
        //! Various flavors of precision missions
        bool precisionMissionCMD;
        bool precisionMissionsCollisionAvoidanceCMD;
        bool precisionMissionsLidarMappingCMD;
        bool precisionMissionsCollisionAvoidanceLidarMappingCMD;

        //! Versioning and activation
        VersionData versionData = new VersionData();
        ActivateData accountData;

        ushort seq_num;

        private SDKFilter filter = new SDKFilter();







        private MMU_Tab[] MMU = new MMU_Tab[MMU_TABLE_NUM];
        private CMDSession[] CMDSessionTab = new CMDSession[SESSION_TABLE_NUM];
        private ACKSession[] ACKSessionTab = new ACKSession[SESSION_TABLE_NUM - 1];
        private byte[] memory = new byte[MEMORY_SIZE];

        public bool isEncrypt;




        private bool callbackThread;
        private bool hotPointData;
        private bool wayPointData;
        private bool followData;

#if API_BUFFER_DATA
 
 public  void setTotalRead(const size_t &value) { totalRead = value; }
 public    void setOnceRead(const size_t &value) { onceRead = value; }

	public int getTotalRead() { return totalRead; }
public int getOnceRead() { return onceRead; }

  private:
 private  int onceRead;
int totalRead;
#endif
    }
}
