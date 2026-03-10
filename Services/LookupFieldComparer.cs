using System.Collections.Generic;
using Autodesk.Revit.DB;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.Services
{
    public class LookupFieldComparer : IEqualityComparer<LookupField>
    {
        public bool Equals(LookupField x, LookupField y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            long xParamId = x.ParameterElement?.Id?.Value ?? -1;
            long yParamId = y.ParameterElement?.Id?.Value ?? -1;

            return xParamId == yParamId
                   && x.FieldName == y.FieldName
                   && x.ParameterType == y.ParameterType;
        }

        public int GetHashCode(LookupField obj)
        {
            unchecked
            {
                long hash = 17;

                long paramId = obj.ParameterElement?.Id?.Value ?? -1;

                hash = hash * 23 + paramId.GetHashCode();
                hash = hash * 23 + (obj.FieldName?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.ParameterType?.GetHashCode() ?? 0);

                return (int)hash;
            }
        }
    }
}