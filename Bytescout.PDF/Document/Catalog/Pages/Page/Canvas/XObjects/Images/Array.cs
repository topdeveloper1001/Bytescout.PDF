using System;

namespace Bytescout.PDF
{
    internal class Array<T>
    {
	    private readonly T[] _array;

	    public T this[int index]
	    {
		    get
		    {
			    return _array[index];
		    }

		    set
		    {
			    _array[index] = value;
		    }
	    }

	    public int Length
	    {
		    get { return _array.Length; }
	    }

	    public Array(long size)
        {
            if (size > -1)
                _array = new T[size];
        }

        public Array(T data)
        {
            _array = new T[] { data };
        }

	    public static Array<T> operator +(Array<T> ba1, Array<T> ba2)
        {
            Array<T> res = new Array<T>(ba1.Length + ba2.Length);

            Array.Copy(ba1._array, res._array, ba1.Length);
            Array.Copy(ba2._array, 0, res._array, ba1.Length, ba2.Length);

            return res;
        }

        public static Array<T> operator +(Array<T> ba1, T ba2)
        {
            Array<T> res = new Array<T>(ba1.Length + 1);

            Array.Copy(ba1._array, res._array, ba1.Length);
            res._array[ba1.Length] = ba2;

            return res;
        }

        public override bool Equals(Object obj)
        {
            return (this == (Array<T>)obj);
        }

        public override int GetHashCode()
        {
            return _array.GetHashCode();
        }

        public static bool operator ==(Array<T> ba1, Array<T> ba2)
        {
            if (ba1.Length != ba2.Length)
                return false;

            return Array.Equals(ba1._array, ba2._array);
        }

        public static bool operator !=(Array<T> ba1, Array<T> ba2)
        {
            return !(ba1 == ba2);
        }
    }
}
