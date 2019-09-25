using System;

namespace Calcex.Windows
{
    public class Calculation
    {
        public int ID { get; set; }
        public string Expression { get; set; }
        public object Result { get; set; }

        public override string ToString() => $"{Expression} = {Result}";
    }
}
