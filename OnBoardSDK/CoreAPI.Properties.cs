namespace DJI.OnBoardSDK
{
    partial class CoreAPI
    {

        public bool IsActivated
        {
            get
            {
                return broadcastData.activation == ACK_ACTIVE_CODE.ACK_ACTIVE_SUCCESS;
            }
        }
    }
}
