using ClipboardCanvas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class DisplayFrameNavigationParameterDataModel
    {
        public readonly ICollectionsContainerModel collectionContainer;

        public DisplayFrameNavigationParameterDataModel(ICollectionsContainerModel collectionContainer)
        {
            this.collectionContainer = collectionContainer;
        }
    }
}
