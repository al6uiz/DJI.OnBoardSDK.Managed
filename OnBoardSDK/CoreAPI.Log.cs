using System;
using System.Text;

namespace DJI.OnBoardSDK
{
    partial class CoreAPI
    {

        void printFrame(IPlatformDriver serialDevice, Ptr pHeader, bool onboardToAircraft)
        {
            var header = (Header)(pHeader);

            var pData = (pHeader + HEADER_SIZE);
            if (pData.Byte == 0 && (pData + 1).Byte == 0xFE)
            {
                return;
            }

            serialDevice.lockLog();

            var crc32 = (pHeader + (header.length - 4)).UInt32;

            if (!header.isAck)
            {
                Ptr command = (pHeader + HEADER_SIZE);

                if (!onboardToAircraft && (CMD_SET)command.Byte == CMD_SET.SET_BROADCAST)
                {
                    serialDevice.freeLog();
                    return;
                }

                API_LOG(serialDevice, DEBUG_LOG, "");
                if (onboardToAircraft)
                {
                    API_LOG(serialDevice, DEBUG_LOG, "|---------------------Sending To Aircraft-------------------------------------------------------------|");
                }
                else
                {
                    API_LOG(serialDevice, DEBUG_LOG, "|---------------------Received From Aircraft-----------------------------------------------------------|");
                }

                API_LOG(serialDevice, DEBUG_LOG,
                    "|<---------------------Header-------------------------------.|<---CMD frame data--.|<--Tail-.|");
                API_LOG(serialDevice, DEBUG_LOG,
                    "|SOF |LEN |VER|SESSION|ACK|RES0|PADDING|ENC|RES1|SEQ   |CRC16 |CMD SET|CMD ID|CMD VAL|  CRC32   |");
                API_LOG(serialDevice, DEBUG_LOG,
                    "|0x{0:X2}|{1,4}|{2,3}|{3,7}|{4,3}|{5,4}|{6,7}|{7,3}|{8,4}|{9,6}|0x{10:X4}|  0x{11:X2} | 0x{12:X2} |       |0x{13:X8}|", header.sof,
                    header.length, header.version, header.sessionID, header.isAck ? 1 : 0,
                    header.reversed0, header.padding, header.enc ? 1 : 0, header.reversed1,
                    header.sequenceNumber, header.crc, command.Byte, (command + 1).Byte, crc32);

                if ((CMD_SET)(command.Byte) == CMD_SET.SET_ACTIVATION && (command + 1).Byte == 0x00)
                {
                    //            API_LOG(serialDevice, DEBUG_LOG,
                    //                    "command\tset: %d %scommand id:");
                    //        __ActivationGetProtocolVersionCommand aCommand = (__ActivationGetProtocolVersionCommand) &command;
                }
            }
            else
            {
                API_LOG(serialDevice, DEBUG_LOG, "");
                if (onboardToAircraft)
                {
                    API_LOG(serialDevice, DEBUG_LOG,
                        "|---------------------Sending To Aircraft-------------------------------------------------------------|");
                }
                else
                {
                    API_LOG(serialDevice, DEBUG_LOG,
                        "|---------------------Received From Aircraft-----------------------------------------------------------|");
                }

                API_LOG(serialDevice, DEBUG_LOG,
                    "|<---------------------Header---------------------------------|--ACK frame data--|---Tail---|");
                API_LOG(serialDevice, DEBUG_LOG,
                    "|SOF |LEN |VER|SESSION|ACK|RES0|PADDING|ENC|RES1|SEQ   |CRC16 |      ACK VAL     |  CRC32   |");
                API_LOG(serialDevice, DEBUG_LOG,
                    "|0x{0:X2}|{1,4}|{2,3}|{3,7}|{4,3}|{5,4}|{6,7}|{7,3}|{8,4}|{9,6}|0x{10:X4}|      ACK VAL     |0x{11:X8}|", header.sof,
                    header.length, header.version, header.sessionID, header.isAck ? 1 : 0,
                    header.reversed0, header.padding, header.enc ? 1 : 0, header.reversed1,
                    header.sequenceNumber, header.crc, crc32);
            }
            serialDevice.freeLog();

        }

        internal void send(int v1, bool isEncrypt, CMD_SET sET_CONTROL, byte cODE_CONTROL, object allocPointer, int v2)
        {
            throw new NotImplementedException();
        }

        private string GetCode(Ptr command)
        {
            var id = (command + 1).Byte;
            switch ((CMD_SET)command.Byte)
            {
                case CMD_SET.SET_ACTIVATION: return ((ACTIVATION_CODE)id).ToString();
                case CMD_SET.SET_CONTROL: return ((CONTROL_CODE)id).ToString();
                case CMD_SET.SET_BROADCAST: return ((BROADCAST_CODE)id).ToString();
                case CMD_SET.SET_MISSION: return null;
                case CMD_SET.SET_SYNC: return ((SYNC_CODE)id).ToString();
                case CMD_SET.SET_VIRTUALRC: return ((VIRTUALRC_CODE)id).ToString();
                default: return id.ToString();
            }
        }
    }
}
