using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using System.Threading;
using HtmlAgilityPack;
using Windows.System;
using Windows.UI.Xaml.Media;

using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Interfaces.Canvas;
using ClipboardCanvas.Helpers;

namespace ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay
{
    public class UrlSimpleCanvasViewModel : BaseReadOnlyCanvasViewModel, ICanOpenFile
    {
        private bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => SetProperty(ref _IsLoading, value);
        }

        private int _Item1RowSpan;
        public int Item1RowSpan
        {
            get => _Item1RowSpan;
            set => SetProperty(ref _Item1RowSpan, value);
        }

        private int _Item2RowSpan;
        public int Item2RowSpan
        {
            get => _Item1RowSpan;
            set => SetProperty(ref _Item1RowSpan, value);
        }

        private string _Url;
        public string Url
        {
            get => _Url;
            set => SetProperty(ref _Url, value);
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string _SiteName;
        public string SiteName
        {
            get => _SiteName;
            set => SetProperty(ref _SiteName, value);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        private bool _DescriptionExpanderLoad;
        public bool DescriptionExpanderLoad
        {
            get => _DescriptionExpanderLoad;
            set => SetProperty(ref _DescriptionExpanderLoad, value);
        }

        private ImageSource _SiteIcon;
        public ImageSource SiteIcon
        {
            get => _SiteIcon;
            set => SetProperty(ref _SiteIcon, value);
        }

        private bool _ContentImageLoad;
        public bool ContentImageLoad
        {
            get => _ContentImageLoad;
            set => SetProperty(ref _ContentImageLoad, value);
        }

        private ImageSource _ContentImage1;
        public ImageSource ContentImage1
        {
            get => _ContentImage1;
            set => SetProperty(ref _ContentImage1, value);
        }

        private ImageSource _ContentImage2;
        public ImageSource ContentImage2
        {
            get => _ContentImage2;
            set => SetProperty(ref _ContentImage2, value);
        }

        private ImageSource _ContentImage3;
        public ImageSource ContentImage3
        {
            get => _ContentImage3;
            set => SetProperty(ref _ContentImage3, value);
        }

        private ImageSource _ContentImage4;
        public ImageSource ContentImage4
        {
            get => _ContentImage4;
            set => SetProperty(ref _ContentImage4, value);
        }

        public static List<string> Extensions => new List<string>() {
            Constants.FileSystem.URL_CANVAS_FILE_EXTENSION
        };

        public UrlSimpleCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        #region Override

        public override async Task<SafeWrapperResult> TryLoadExistingData(CanvasItem canvasItem, BaseContentTypeModel contentType, CancellationToken cancellationToken)
        {
            IsLoading = true;
            SafeWrapperResult result = await base.TryLoadExistingData(canvasItem, contentType, cancellationToken);
            IsLoading = false;

            return result;
        }

        protected override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> url = await FilesystemOperations.ReadFileText(file);
            if (!AssertNoError(url))
            {
                return url;
            }
            this.Url = url;

            // Set the document
            var webGet = new HtmlWeb();
            webGet.UserAgent = @"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            SafeWrapper<HtmlDocument> document = await SafeWrapperRoutines.SafeWrapAsync(() => webGet.LoadFromWebAsync(Url, cancellationToken));
            if (!AssertNoError(document))
            {
                return document;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            HtmlNodeCollection metaTags = document.Result.DocumentNode.SelectNodes("//meta");

            // Get metadata
            SiteMetadata metadata = WebHelpers.GetMetadata(metaTags);

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            this._Title = metadata.Title;
            this._Description = metadata.Description;
            this._SiteName = metadata.SiteName;

            if (string.IsNullOrEmpty(metadata.IconUrl))
            {
                HtmlNodeCollection linkNodes = document.Result.DocumentNode.SelectNodes("//link");
                metadata.IconUrl = WebHelpers.AlternativeGetIcon(linkNodes);
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            SafeWrapper<List<string>> imageUrls = await SafeWrapperRoutines.SafeWrapAsync(() => WebHelpers.FormatImageUrls(metadata.ImageUrls, Url));
            SafeWrapper<string> imageLogo = await SafeWrapperRoutines.SafeWrapAsync(() => WebHelpers.FormatImageUrl(metadata.IconUrl, url));

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            if (!string.IsNullOrEmpty(imageLogo))
            {
                _SiteIcon = await ImagingHelpers.ToImageAsync(new Uri(imageLogo));
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            if ((Description?.Length ?? 0) > 85)
            {
                DescriptionExpanderLoad = true;
            }
            else
            {
                DescriptionExpanderLoad = false;
            }

            // Set images
            if (!imageUrls.Result.IsEmpty())
            {
                ContentImageLoad = true;

                foreach (var imageLink in imageUrls.Result)
                {
                    if (this._ContentImage1 == null)
                    {
                        _ContentImage1 = await ImagingHelpers.ToImageAsync(new Uri(imageLink));
                    }
                    else if (this._ContentImage2 == null)
                    {
                        _ContentImage2 = await ImagingHelpers.ToImageAsync(new Uri(imageLink));
                    }
                    else if (this._ContentImage3 == null)
                    {
                        _ContentImage3 = await ImagingHelpers.ToImageAsync(new Uri(imageLink));
                    }
                    else if (this._ContentImage4 == null)
                    {
                        _ContentImage4 = await ImagingHelpers.ToImageAsync(new Uri(imageLink));
                    }
                    else
                    {
                        break;
                    }

                    if (cancellationToken.IsCancellationRequested) // Check if it's canceled
                    {
                        DiscardData();
                        return SafeWrapperResult.CANCEL;
                    }
                }
            }

            return SafeWrapperResult.SUCCESS;
        }

        protected override Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(SiteName));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(SiteIcon));

            OnPropertyChanged(nameof(ContentImage1));
            OnPropertyChanged(nameof(ContentImage2));
            OnPropertyChanged(nameof(ContentImage3));
            OnPropertyChanged(nameof(ContentImage4));

            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override bool AssertNoError(SafeWrapperResult result)
        {
            bool success = base.AssertNoError(result);
            if (!success)
            {
                Description = null;
                ContentImageLoad = false;
                IsLoading = false;
            }

            return success;
        }

        protected override void RefreshContextMenuItems()
        {
            base.RefreshContextMenuItems();
        }

        #endregion

        #region Helpers

        public async Task OpenFile()
        {
            if (!string.IsNullOrEmpty(Url))
            {
                await Launcher.LaunchUriAsync(new Uri(Url));
            }
            else
            {
                await StorageHelpers.OpenFile(await sourceItem);
            }
        }

        #endregion
    }
}
