using System;
using System.Drawing;
using System.Runtime.InteropServices;
namespace DC4Ever
{
	/// <summary>
	/// Summary description for 
	/// </summary>
    public partial class emu
    {
		#region WinApi
		public struct BITMAPINFOHEADER
		{
			public int biSize;
			public int biWidth;
			public int biHeight;
			public short biPlanes;
			public short biBitCount;
			public int biCompression;
			public int biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public int biClrUsed;
			public int biClrImportant;
		}
		[DllImport("gdi32.dll")]
		static unsafe extern int StretchDIBits ( 
			System.IntPtr hdc,
			int x,
			int y,
			int dx,
			int dy,
			int SrcX,
			int SrcY,
			int wSrcWidth,
			int wSrcHeight,
			byte* lpBits,
			BITMAPINFOHEADER* lpBitsInfo,
			int wUsage,
			int dwRop);
		public static BITMAPINFOHEADER bitinfo;
		#endregion
		public static uint f;
		public static uint fps;
		public static byte[] vram= new byte[8*dc.mb];
		public static unsafe void writemem(uint adr,uint data,int len)
		{
			#region Address translation
			if (adr<0x800000)//(adr>>24)&0x1//using 64 bit interface
			{
				//Translate address to offset
				//if bit 2(0x4) is set then read from rambank2(4mb+>)
				//get rid of bit 2(0x4) and >> by 1 to fix the pos 
				//01111111111111111111100->0x3FFFFC
				//00000000000000000000011->0x3
				adr=((adr>>1)&0x3FFFFC)+(adr&0x3)+0x3FFFFF*((adr>>2)&0x1);
			}
			else if ((adr > 0xFFFFFF)&& (adr<0x1800000))//using 32 bit interface
			{
				adr=adr-0x1000000;//translate to vram offset
			}
			else 
			{
				//dc.dcon.WriteLine("Address read out of Vram on write (pc="+pc+")");
				return;
			}
			#endregion
		switch (len)
			{
				case 0x1://1 byte write
					vram[adr]=(byte)data;
					return;
				case 0x2://2 byte write
					fixed(byte *p=&vram[adr])
						*(ushort*)p=(ushort)data;
					return;
				case 0x4://4 byte write
					fixed(byte *p=&vram[adr])
						*(uint*)p=data;
					return; 
			}
			dc.dcon.WriteLine("Wrong write size in write (" + len+") at pc "+pc);
		}
		
		public static unsafe uint readmem(uint adr,int len)
		{
			#region Address translation
			if ((adr > 0xFFFFFF)&& (adr<0x1800000))//using 32 bit interface
			{
				adr=adr-0x1000000;//translate to vram offset
			}
			else if (adr<0x800000)//(adr>>24)&0x1//using 64 bit interface
			{
				//Translate address to offset
				//if bit 2(0x4) is set then read from rambank2(4mb+>)
				//get rid of bit 2(0x4) and >> by 1 to fix the pos 
				//01111111111111111111100->0x3FFFFC
				//00000000000000000000011->0x3
					adr=((adr>>1)&0x3FFFFC)+(adr&0x3)+0x3FFFFF*((adr>>2)&0x1);
			}
			else 
			{
				dc.dcon.WriteLine("Address read out of Vram on read (pc="+pc+")");
				return 0;
			}
			#endregion
			switch (len)
			{
				case 0x1://1 byte read
						return vram[adr];
				case 0x2://2 byte read
					fixed(byte *p=&vram[adr])
						return *(ushort*)p;
				case 0x4://4 byte read
					fixed(byte *p=&vram[adr])
						return *(uint*)p;
			}
			dc.dcon.WriteLine("Wrong read size in read (" + len+") at pc "+pc);
			return 0;
		}
		
		public static unsafe  void present()// draw the framebuffer(640*480*16 bit)
		{
                System.IntPtr hdc = dx.bb.GetDc();
                fixed (byte* i = &vram[0])
                {
                    fixed (BITMAPINFOHEADER* bi = &bitinfo)
                        StretchDIBits(hdc, 0, 0, 640, 480, 0, 0, 640, 480, i, bi, 0, 13369376);
                }
                dx.bb.ReleaseDc(hdc);
                try
                {
                    dx.fb.Draw(new Rectangle(dc.frmMain.PointToScreen(new Point(dc.frmMain.ClientRectangle.X + 8, dc.frmMain.ClientRectangle.Y + 8))
                        , new Size(dc.frmMain.screen.Width, dc.frmMain.screen.Height)), dx.bb, Microsoft.DirectX.DirectDraw.DrawFlags.DoNotWait | Microsoft.DirectX.DirectDraw.DrawFlags.Async);
                }
                catch { }
            fps+=1;
		}
	}
}