using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class ClearCollectionCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            var collection = parameter as IList;
            if (collection != null)
            {
                collection.Clear();
            }
        }
    }
}