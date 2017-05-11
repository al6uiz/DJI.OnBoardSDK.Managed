namespace DJI.OnBoardSDK
{
    partial class Camera
    {
        private CoreAPI api;


        public Camera(CoreAPI ControlAPI) { api = ControlAPI; }

        public void setCamera(CAMERA_CODE camera_cmd)
        {
            //byte send_data = 0;
            api.send(0, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)camera_cmd, new byte[1], 1);
        }

        private byte[] _bufferAngle;
        private byte[] _bufferSpeed;

        public void setGimbalAngle(GimbalAngleData data)
        {
            if (_bufferAngle == null)
            {
                _bufferAngle = new byte[data.TypeSize];
            }

            data.WriteTo(_bufferAngle);

            api.send(0, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CAMERA_CODE.CODE_GIMBAL_ANGLE, _bufferAngle,
                _bufferAngle.Length);
        }

        public void setGimbalSpeed(GimbalSpeedData data)
        {
            if (_bufferSpeed == null)
            {
                _bufferSpeed = new byte[data.TypeSize];
            }

            data.WriteTo(_bufferSpeed);

            data.reserved = 0x80;
            api.send(0, api.isEncrypt, CMD_SET.SET_CONTROL, (byte)CAMERA_CODE.CODE_GIMBAL_SPEED, _bufferSpeed,
                _bufferSpeed.Length);
        }

        public GimbalData getGimbal() { return api.getBroadcastData().gimbal; }

        public float getYaw() { return api.getBroadcastData().gimbal.yaw; }

        public float getRoll() { return api.getBroadcastData().gimbal.roll; }

        public float getPitch() { return api.getBroadcastData().gimbal.pitch; }

        public bool isYawLimit()
        {
            if (api.getFwVersion() != Version.M100_23)
                return api.getBroadcastData().gimbal.yawLimit ? true : false;
            return false;
        }

        public bool isRollLimit()
        {
            if (api.getFwVersion() != Version.M100_23)
                return api.getBroadcastData().gimbal.rollLimit ? true : false;
            return false;
        }
        public bool isPitchLimit()
        {
            if (api.getFwVersion() != Version.M100_23)
                return api.getBroadcastData().gimbal.pitchLimit ? true : false;
            return false;
        }

        public CoreAPI getApi() { return api; }

        public void setApi(CoreAPI value) { api = value; }

    }

    public enum CAMERA_CODE
    {
        CODE_GIMBAL_SPEED = 0x1A,
        CODE_GIMBAL_ANGLE = 0x1B,
        CODE_CAMERA_SHOT = 0x20,
        CODE_CAMERA_VIDEO_START = 0x21,
        CODE_CAMERA_VIDEO_STOP = 0x22
    }
}
