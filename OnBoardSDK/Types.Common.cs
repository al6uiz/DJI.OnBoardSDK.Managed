namespace DJI.OnBoardSDK
{

    //    public uint64_t time_ms;
    //public uint64_t time_us; // about 0.3 million years

    ////! This is used as the datatype for all data arguments in callbacks.
    //public void* UserData;
    //    public uint32_t Flag;

    //public uint8_t size8_t;
    //public uint16_t size16_t;

    //! @warning This struct will be replaced by Measurement in a future release.
    public struct Measure
    {
        public double data;
        public float precision;
    }
    //! @note This struct will replace Measure in a future release.
    public struct Measurement
    {
        public double data;
        public float precision;
    }

    //! @warning This struct will be replaced by Vector3dData (similar to Vector3fData in DJI_Type.h) in a future release.
    public struct SpaceVector
    {
        public double x;
        public double y;
        public double z;
    }

    //! @note This struct will replace SpaceVector in a future release.
    //! Eigen-like naming convention
    public struct Vector3dData
    {
        public double x;
        public double y;
        public double z;
    }

    /*! @todo range mathematial class
    class Angle
    {
      public:
      Angle(double degree = 0);

      private:
      double degree;
    };
    */


    //! @warning This struct will be replaced by EulerAngle in a future release.
    //public struct EulerianAngle
    //{
    //    public double yaw;
    //    public double roll;
    //    public double pitch;
    //}

    public struct Quaternion
    {
        public float q0;
        public float q1;
        public float q2;
        public float q3;
    }

    //! @note This struct will replace EulerianAngle in a future release.
    public struct EulerAngle
    {
        public double yaw;
        public double roll;
        public double pitch;
    }

}
