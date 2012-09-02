using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NETPack.Core.Engine.Utils
{
    public static class NativeHelper
    {
        #region Structs & APIs declarations

        [DllImport("Shell32", CharSet = CharSet.Auto)]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phIconLarge,
                                                IntPtr[] phIconSmall, int nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, string lpName, ushort wLanguage,
                                                  IntPtr lpData, uint cbData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        [StructLayout(LayoutKind.Sequential)]
        private struct GroupIcon
        {
            public short Reserved1;
            public short ResourceType;
            public short ImageCount;
            public byte Width;
            public byte Height;
            public byte Colors;
            public byte Reserved2;
            public short Planes;
            public short BitsPerPixel;
            public int ImageSize;
            public short ResourceID;
        }

        #endregion

        public static Icon ExtractIcon(string fileName, out int size)
        {
            var ico = Icon.ExtractAssociatedIcon(fileName);
            var ms = new MemoryStream();

            ico.Save(ms);
            size = (int) ms.Length;

            return ico;
        }

        public unsafe static bool UpdateIcon(string targetFile, Icon hIcon, int size)
        {
            var hRes = BeginUpdateResource(targetFile, false);

            var currentCulture = CultureInfo.CurrentCulture;
            var pid = ((ushort) currentCulture.LCID) & 0x3ff;
            var sid = ((ushort) currentCulture.LCID) >> 10;
            var languageID = (ushort) ((((ushort) pid) << 10) | ((ushort) sid));

            if (hRes == IntPtr.Zero)
                return false;

            var gc = GCHandle.Alloc(GetIconData(hIcon), GCHandleType.Pinned);

            if (!UpdateResource(hRes, (IntPtr) 3, "0", languageID, (IntPtr)(gc.AddrOfPinnedObject().ToInt32() + 22), (uint) size -22))
            {
                EndUpdateResource(hRes, true);
                return false;
            }

            var grpIcon = new GroupIcon
                              {
                                  Reserved1 = 0,
                                  ResourceType = 1,
                                  ImageCount = 1,
                                  Width = 32,
                                  Height = 32,
                                  Colors = 0,
                                  Reserved2 = 0,
                                  Planes = 2,
                                  BitsPerPixel = 32,
                                  ImageSize = size - 22,
                                  ResourceID = 1
                              };

            if (!UpdateResource(hRes, (IntPtr) 14, "MAINICON", languageID, (IntPtr)( &grpIcon ), (uint)Marshal.SizeOf(grpIcon)))
            {
                EndUpdateResource(hRes, true);
                return false;
            }

            EndUpdateResource(hRes, false);
            return true;
        }

        private static byte[] GetIconData(Icon ico)
        {
            var ms = new MemoryStream();

            ico.Save(ms);
            return ms.ToArray();
        }
    }
}