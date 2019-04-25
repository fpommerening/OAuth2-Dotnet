using System.Collections.Generic;

namespace FP.OAuth.ResourceServer.Business
{
    public interface IValueRepository
    {
        ValueItem Add(string userName, int value);
        List<ValueItem> GetValue();
    }
}