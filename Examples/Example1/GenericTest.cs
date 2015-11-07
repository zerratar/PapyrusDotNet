using PapyrusDotNet.Core;

namespace Example1
{
    public class GenericTest
    {
        [Property, Auto] private GenericClass<int> genericInteger;

        public void OnInit()
        {
            genericInteger.Set(9999);
            var value = genericInteger.Get();
            Debug.Trace("The value is: " + value, 0);
        }
    }

    public class GenericClass<T> : Form
    {
        public T GenericVariable;

        public void Set(T value)
        {
            GenericVariable = value;
        }

        public T Get()
        {
            return GenericVariable;
        }
    }
}