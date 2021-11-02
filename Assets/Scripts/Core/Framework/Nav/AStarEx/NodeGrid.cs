using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
namespace Core.Framework.Nav.AStarEx
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NodeFast
    {
        public float f;
        public float g;
        public float h;
        public int parentX;
        public int parentY;
		public byte ClosedStatus;
    }
    public class NodeGrid
    {		
		private int _startNodeX;
        private int _startNodeY;
		private int _endNodeX;
        private int _endNodeY;
        private static NodeFast[,] _nodes;
        private byte[,] _fixedObstruction;
        private static int _numCols;
        private static int _numRows;
		public NodeGrid(int numCols, int numRows)
		{
			setSize( numCols, numRows );
		}
		public void setSize( int numCols, int numRows)
		{
            if (_nodes == null || _numCols < numCols || _numRows < numRows)
            {
                _numCols = Math.Max(numCols, _numCols);
                _numRows = Math.Max(numRows, _numRows);
                _nodes = new NodeFast[_numCols, _numRows];
            }
            _fixedObstruction = new byte[numCols, numRows];
            for (int i = 0; i < numCols; i++)
            {
                for (int j = 0; j < numRows; j++)
                {
                    _fixedObstruction[i, j] = 1;
                }
            }
		}
        public byte[,] FixedObstruction
        {
            get { return _fixedObstruction;  }
        }
        public void Clear()
        {
            Array.Clear(_nodes, 0, _nodes.Length);
        }
        public NodeFast[,] Nodes
        {
            get
            {
                return _nodes;
            }
        }
        public bool isDiagonalWalkable(int node1, int node2)
        {
            int node1x = ANode.GetGUID_X(node1);
            int node1y = ANode.GetGUID_Y(node1);
            int node2x = ANode.GetGUID_X(node2);
            int node2y = ANode.GetGUID_Y(node2);
            if (1 == _fixedObstruction[node1x, node2y] && 1 == _fixedObstruction[node2x, node1y])
            {
                return true;
            }
			return false;
        }
		public void setEndNode(int x, int y)
		{
            _endNodeX = x;
            _endNodeY = y;
		}
		public void setStartNode(int x, int y)
		{
            _startNodeX = x;
            _startNodeY = y;
		}
		public void setWalkable(int x, int y, bool value)
		{
            if (value)
            {
                _fixedObstruction[x, y] = 1;
            }
            else
            {
                _fixedObstruction[x, y] = 0;
            }
		}
        public bool isWalkable(int x, int y)
        {
            return 1 == _fixedObstruction[x, y];
        }
		public int endNodeX
		{
            get { return  _endNodeX; }
		}
        public int endNodeY
        {
            get { return _endNodeY; }
        }
		public int numCols
		{
            get { return _numCols; }
		}
		public int numRows
		{
            get { return _numRows; }
		}
		public int startNodeX
		{
            get {  return _startNodeX; }
		}
        public int startNodeY
        {
            get { return _startNodeY; }
        }
    }
}
