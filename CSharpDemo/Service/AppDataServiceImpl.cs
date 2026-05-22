using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CSharpDemo.Utils;

namespace CSharpDemo.Service
{
    public class AppDataServiceImpl : IAppDataService
    {
        private readonly string[] _itemTitles =
        {
            "相关仪算法测试", "音频可视化", "串口通信"
        };

        public List<string> GetItemModels()
        {
            return _itemTitles.ToList();
        }

        public string GetCorrelatorWakeUpCmd()
        {
            const string preamble = "A3";
            var preambleByte = byte.Parse(preamble, NumberStyles.HexNumber);

            const string version = "20";
            var btVersion = byte.Parse(version, NumberStyles.HexNumber);

            byte[] btDevId = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; //广播模式，设备类型不解析

            const string routeFlag = "1";
            var btRouteFlag = byte.Parse(routeFlag, NumberStyles.HexNumber);

            byte[] btDstNode = { 0xFF, 0xFF };

            const short pduTypeByte = 2;
            var operateType = (short)(pduTypeByte & 0x7F);

            var pdu = (short)(2688 + operateType); //0X0A80
            var btPdu0 = BitConverter.GetBytes(pdu);
            byte[] btPdu = { btPdu0[1], btPdu0[0] };

            const string seq = "1";
            var btSeq = byte.Parse(seq, NumberStyles.HexNumber);

            byte[] oid = { 0x30, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00 };

            byte[] totalLen = { 0x00, 0x13 };

            //wrap the whole data
            var result = new byte[1 + 1 + 2 + 6 + 1 + 2 + 2 + 1 + oid.Length];

            result[0] = preambleByte;
            result[1] = btVersion;
            totalLen.CopyTo(result, 2);
            btDevId.CopyTo(result, 4);
            result[10] = btRouteFlag;
            btDstNode.CopyTo(result, 11);
            btPdu.CopyTo(result, 13);
            result[15] = btSeq;
            oid.CopyTo(result, 16);

            //增加CRC校验
            var strCrc = $"{(int)CrcCode.GenerateCrc16Code(result):X}".ConvertToHexString();
            byte[] crcByte =
            {
                strCrc.Substring(0, 2).ConvertToByte(),
                strCrc.Substring(2, 2).ConvertToByte()
            };

            var afCrc = new byte[result.Length + 2];
            result.CopyTo(afCrc, 0);
            crcByte.CopyTo(afCrc, result.Length);
            return BitConverter.ToString(afCrc);
        }

        public string GetStatusCollectCmd(byte devId)
        {
            const string preamble = "A3";
            var preambleByte = byte.Parse(preamble, NumberStyles.HexNumber);

            const string version = "20";
            var btVersion = byte.Parse(version, NumberStyles.HexNumber);

            byte[] btDevId = { 0x21, 0x17, 0x00, 0x08, 0x22, devId }; //广播模式，设备类型不解析

            const string routeFlag = "1";
            var btRouteFlag = byte.Parse(routeFlag, NumberStyles.HexNumber);

            byte[] btDstNode = { 0x22, devId };

            const short pduTypeByte = 2;
            const short operateType = pduTypeByte & 0x7F;

            const short pdu = 2688 + operateType; //0X0A80
            var btPdu0 = BitConverter.GetBytes(pdu);
            byte[] btPdu = { btPdu0[1], btPdu0[0] };

            const string seq = "1";
            var btSeq = byte.Parse(seq, NumberStyles.HexNumber);

            byte[] oid = { 0x60, 0x00, 0x02, 0x00, 0x00, 0x01, 0x00 };

            byte[] totalLen = { 0x00, 0x13 };

            var result = new byte[1 + 1 + 2 + 6 + 1 + 2 + 2 + 1 + oid.Length];

            result[0] = preambleByte;
            result[1] = btVersion;
            totalLen.CopyTo(result, 2);
            btDevId.CopyTo(result, 4);
            result[10] = btRouteFlag;
            btDstNode.CopyTo(result, 11);
            btPdu.CopyTo(result, 13);
            result[15] = btSeq;
            oid.CopyTo(result, 16);

            //增加CRC校验
            var strCrc = $"{(int)CrcCode.GenerateCrc16Code(result):X}".ConvertToHexString();
            byte[] crcByte =
            {
                strCrc.Substring(0, 2).ConvertToByte(),
                strCrc.Substring(2, 2).ConvertToByte()
            };

            var afCrc = new byte[result.Length + 2];
            result.CopyTo(afCrc, 0);
            crcByte.CopyTo(afCrc, result.Length);
            return BitConverter.ToString(afCrc);
        }
    }
}