using Aspose.Words.Loading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SISPK.Models
{
    public class HandleResourceLoading : IResourceLoadingCallback
    {
        public ResourceLoadingAction ResourceLoading(ResourceLoadingArgs args)
        {

            return ResourceLoadingAction.Skip;

        }
    }
}