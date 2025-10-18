using System;
using System.Linq.Expressions;

namespace Assets.Interfaces
{
    public interface IPseudoCompositeKey<TSelf> where TSelf : IPseudoCompositeKey<TSelf>
    {
        public Expression<Func<TSelf, bool>> GetCompositeKeyPredicate();
    }
}
