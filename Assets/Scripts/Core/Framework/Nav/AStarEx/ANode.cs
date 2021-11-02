using System;
using System.Net;
using System.Runtime.InteropServices;
namespace Core.Framework.Nav.AStarEx
{
    public class ANode
    {
		public int x;
		public int y;
		public ANode(int x, int y)
		{
			this.x = x;
			this.y = y;
		}		
        public static int GetGUID(int key1, int key2)
        {
            int lKey1 = key1;
            int lKey2 = key2;
            return (lKey1 << 16) | lKey2;
        }
        public static int GetGUID_X(int val)
        {
            return (int)((val >> 16) & 0x0000FFFFL);
        }
        public static int GetGUID_Y(int val)
        {
            return (int)(val & 0x0000FFFFL);
        }
    }
}
