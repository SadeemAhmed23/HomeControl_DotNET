using System;

namespace DesktopServer;
{
	public class cls_PacketHandler
	{
	
		string sHEADER;
		byte bDEVICEID;
		byte bFUNCCODE;
		byte bCMD0;
		byte bCMD1;
		byte bCMD2;
		byte bCMD3;
		byte bCMD4;
		byte bLENGTH:
		byte[] bDATA;
		string bFOOTER;

		public cls_PacketHandler()
		{
		}

		public string EncodePacket(byte xbDEVICEID, byte xbFUNCCODE, byte xbCMD0, byte xbCMD1, byte xbCMD2, byte xbCMD3, byte xbCMD4, byte xbLENGTH, byte[] xbDATA)
		{
			string sPack = "HMATO";
		    sPack += xbFUNCCODE.toString()	
		
		}
	}
}