using System.Linq;

namespace System
{
    public static class ObjectExtentions
    {
        public static void CopyFrom(this object source, object destination, params string[] excludes)
        {
            var type1 = source.GetType();
            var type2 = destination.GetType();

            foreach (var pi in type1.GetProperties())
            {
                if (excludes.Contains(pi.Name)) continue;

                var pi2 = type2.GetProperty(pi.Name);

                if (pi2 == null) continue;

                pi.SetValue(source, pi2.GetValue(destination));
            }
        }
    }
}
