using System;

namespace Practice55
{
    public class Delen
    {
        public double Calculate(double a, double b)
        {
            if (b == 0)
                throw new DivideByZeroException();

            return a / b;
        }
    }
}