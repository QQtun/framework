namespace Core.Framework.Utility
{
    public class ArraySeg<T>
    {
        public T this[int key]
        {
            get => Array[Offset + key];
        }

        public T[] Array { get; private set; }
        public int Offset { get; private set; }
        public int Count { get; private set; }

        public void Update(T[] array, int offset, int count)
        {
            Array = array;
            Offset = offset;
            Count = count;
        }
    }
}