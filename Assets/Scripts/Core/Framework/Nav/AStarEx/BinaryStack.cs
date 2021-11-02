using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Core.Framework.Nav.AStarEx
{
    public class BinaryStack
    {
        public NodeGrid _nodeGrid = null;
        public List<int> _data = null;
		private Dictionary<int, int> _dict = null;
		public  BinaryStack(string compareValue = "f")
		{
            _data = new List<int>(2000);
			_dict = new Dictionary<int, int>(2000);
		}
		public void push(int guid)
		{
            _data.Add(guid);
            _dict[guid] = _data.Count - 1;
			int len = _data.Count;
			if( len > 1 )
			{
				int index = len;
				int parentIndex = index / 2 - 1;
				int temp;
                while (compareTwoNodes(guid, _data[parentIndex]))
				{
					temp = _data[parentIndex];
                    _data[parentIndex] = guid;
                    _dict[guid] = parentIndex;
					_data[index - 1] = temp;
                    _dict[temp] = index - 1;
					index /= 2;
					parentIndex = index / 2 - 1;
                    if (parentIndex < 0)
                    {
                        break;
                    }
				}
			}			
		}
		public int shift()
		{
            int result = _data.ElementAt(0); 
            _data.RemoveAt(0);
            _dict.Remove(result);
			int len = _data.Count;
			if( len > 1 )
			{
                int lastNode = _data.ElementAt(_data.Count - 1);
                _data.RemoveAt(_data.Count - 1);
				_data.Insert(0, lastNode );
                _dict[lastNode] = 0;
				int index = 0;
				int childIndex = (index + 1) * 2 - 1;
				int comparedIndex;
                int temp;
				while( childIndex < len )
				{
					if( childIndex + 1 == len )
					{
						comparedIndex = childIndex;
					}
					else
					{
						comparedIndex = compareTwoNodes(_data[childIndex], _data[childIndex + 1]) ? childIndex : childIndex + 1;
					}
                    if (comparedIndex < 0)
                    {
                        break;
                    }
					if( compareTwoNodes(_data[comparedIndex], lastNode) )
					{
						temp = _data[comparedIndex];
						_data[comparedIndex] = lastNode;
                        _dict[lastNode] = comparedIndex;
						_data[index] = temp;
                        _dict[temp] = index;
						index = comparedIndex;
						childIndex = (index + 1) * 2 - 1;
					}
					else
					{
						break;
					}
				}
			}
			return result;
		}
        public void updateNode(int indexObj, int node)
		{
			int index = indexObj + 1;
			int parentIndex = index / 2 - 1;
            if (parentIndex < 0)
            {
                return;
            }
            int temp;
			while( compareTwoNodes(node, _data[parentIndex]) )
			{
				temp = _data[parentIndex];
				_data[parentIndex] = node;
                _dict[node] = parentIndex;
				_data[index - 1] = temp;
                _dict[temp] = index - 1;
				index /= 2;
				parentIndex = index / 2 - 1;
                if (parentIndex < 0)
                {
                    break;
                }
			}
		}
        public int indexOf(int node)
		{
            int findIndex = -1;
            if (_dict.TryGetValue(node, out findIndex))
            {
                return findIndex;
            }
            return -1;
		}
		public int getLength()
		{
			return _data.Count;
		}
        private bool compareTwoNodes(int node1, int node2)
		{
            float f1 = _nodeGrid.Nodes[ANode.GetGUID_X(node1), ANode.GetGUID_Y(node1)].f;
            float f2 = _nodeGrid.Nodes[ANode.GetGUID_X(node2), ANode.GetGUID_Y(node2)].f;
            return f1 < f2;
		}
		public string toString()
		{
			string result = "";
            int len = _data.Count;
            for (int i = 0; i < len; i++)
            {
                double f = _nodeGrid.Nodes[ANode.GetGUID_X(_data[i]), ANode.GetGUID_Y(_data[i])].f;
                result += f;
                if (i < len - 1) result += ",";
            }
			return result;
		}
        public void ClearAll()
        {
            _data.Clear();
            _dict.Clear();
        }
    }
}
