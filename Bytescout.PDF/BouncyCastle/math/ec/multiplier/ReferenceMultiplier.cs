namespace Org.BouncyCastle.Math.EC.Multiplier
{
    internal class ReferenceMultiplier
        : AbstractECMultiplier
    {
        protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
        {
            return ECAlgorithms.ReferenceMultiply(p, k);
        }
    }
}
