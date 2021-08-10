using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Utils
{
    public static class ObservableCollectionExtensions
    {

        public static void CopyFromStandardCollection<T>(this ObservableCollection<T> targetCollection, ICollection<T>? srcCollection)
        {
            if (srcCollection == null)
            {
                return;
            }
            var targetIdx = 0;
            foreach (var el in srcCollection)
            {
                if (targetIdx < targetCollection.Count)
                {
                    targetCollection[targetIdx] = el;
                }
                else
                {
                    targetCollection.Add(el);
                }
                targetIdx += 1;
            }
            while (srcCollection.Count < targetCollection.Count)
            {
                targetCollection.RemoveAt(targetCollection.Count - 1);
            }
        }
    }
}
