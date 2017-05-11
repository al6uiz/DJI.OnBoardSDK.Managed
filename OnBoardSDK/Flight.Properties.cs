namespace DJI.OnBoardSDK
{
    partial class Flight
    {
        public double Pitch { get { return getPitch(); } }

        public double Roll { get { return getRoll(); } }

        public double Yaw { get { return getYaw(); } }

        public FlightStatus Status
        {
            get { return getStatus(); }
        }

        public FlightMode ControlMode
        {
            get { return getControlMode(); }
        }



        public QuaternionData Quaternion{get{return getQuaternion();}}

        public EulerAngle EulerAngle{get{return getEulerAngle();}}

        public PositionData Position{get{return getPosition();}}

        public VelocityData Velocity{get{return getVelocity();}}

        public Vector3fData Acceleration{get{return getAcceleration();}}

        public Vector3fData YawRate{get{return getYawRate();}}

        public MagData Magnet{get{return getMagnet();}}

    }
}
