using System;
using System.Collections.Generic;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Printing;
using BlueYonder.Companion.Client.DataModel;
using BlueYonder.Companion.Client.Views;

namespace BlueYonder.Companion.Client.Helpers
{
    public class PrinterJob : IDisposable
    {
        private const double LeftMargin = 0.075;
        private const double TopMargin = 0.03;

        private readonly Page _page;
        private readonly PrintJobType _type;
        private readonly Reservation _reservation;

        private readonly PrintDocument _printDocument;
        private readonly IPrintDocumentSource _printDocumentSource;

        private readonly List<UIElement> _printPages; 

        private readonly FrameworkElement _firstPage;

        public Canvas PrintingRoot
        {
            get { return _page.FindName("printingRoot") as Canvas; }
        }

        public PrinterJob(Page page, PrintJobType type, Reservation reservation)
        {
            _page = page;
            _reservation = reservation;
            _type = type;

            _printDocument = new PrintDocument();
            _printDocumentSource = _printDocument.DocumentSource;

            PrintManager printManager = PrintManager.GetForCurrentView();
            printManager.PrintTaskRequested += PrintTaskRequested;

            _printPages = new List<UIElement>();

            _firstPage = CreateFirstPage();
            PrintingRoot.Children.Add(_firstPage);
            PrintingRoot.InvalidateMeasure();
            PrintingRoot.UpdateLayout();

            _printDocument.Paginate += PrintDocument_Paginate;

            _printDocument.GetPreviewPage += PrintDocument_GetPreviewPage;

            _printDocument.AddPages += PrintDocument_AddPages;
        }

        private Page CreateFirstPage()
        {
            if (_type == PrintJobType.BoardingPass)
            {
                return new BoardingPassPreviewPage();
            }
            if (_type == PrintJobType.Receipt)
            {
                return new ReceiptPreviewPage();
            }
            return null;
        }

        private static void SetContentSizeAndMargins(PrintPageDescription printPageDescription, FrameworkElement page, FrameworkElement rootElement)
        {
            var pageSize = printPageDescription.PageSize;
            var pageWidth = pageSize.Width;
            var pageHeight = pageSize.Height;

            rootElement.Width = pageWidth;
            rootElement.Height = pageHeight;

            // Calculate the page margins
            var printableArea = printPageDescription.ImageableRect;
            var marginWidth = Math.Max(pageWidth - printableArea.Width, pageWidth * LeftMargin * 2);
            var marginHeight = Math.Max(pageHeight - printableArea.Height, pageHeight * TopMargin * 2);

            // Set content size based on the calculated margins
            var contentContainer = (FrameworkElement)page.FindName("ContentContainer");
            contentContainer.Width = pageWidth - marginWidth;
            contentContainer.Height = pageHeight - marginHeight;

            // Set content margins
            contentContainer.SetValue(Canvas.LeftProperty, marginWidth / 2);
            contentContainer.SetValue(Canvas.TopProperty, marginHeight / 2);
        }

        private void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
        {
            PrintTask printTask = e.Request.CreatePrintTask("BlueYonder", sourceRequested => sourceRequested.SetSource(_printDocumentSource));

            printTask.Options.DisplayedOptions.Clear();
            printTask.Options.DisplayedOptions.Add(StandardPrintTaskOptions.Copies);
            printTask.Options.DisplayedOptions.Add(StandardPrintTaskOptions.Orientation);
            printTask.Options.DisplayedOptions.Add(StandardPrintTaskOptions.ColorMode);
        }

        public void Dispose()
        {
            PrintManager printManager = PrintManager.GetForCurrentView();
            printManager.PrintTaskRequested -= PrintTaskRequested;
        }

        private void PrintDocument_Paginate(object sender, PaginateEventArgs e)
        {
            _printPages.Clear();

            PrintingRoot.Children.Clear();

            PrintPageDescription pageDescription = e.PrintTaskOptions.GetPageDescription(0);
            GeneratePrintPages(pageDescription);

            _printDocument.SetPreviewPageCount(_printPages.Count, PreviewPageCountType.Intermediate);
        }

        private void GeneratePrintPages(PrintPageDescription printPageDescription)
        {
            if (_type == PrintJobType.BoardingPass)
            {
                GenerateBoardingPassPage(printPageDescription);
            }
            else if (_type == PrintJobType.Receipt)
            {
                GenerateReceiptPages(printPageDescription);
            }
        }

        private void PreparePrintPage(PrintPageDescription printPageDescription, FrameworkElement page, FrameworkElement rootElement)
        {
            page.DataContext = _reservation;

            SetContentSizeAndMargins(printPageDescription, page, rootElement);

            _printPages.Add(page);
            PrintingRoot.Children.Add(page);

            PrintingRoot.InvalidateMeasure();
            PrintingRoot.UpdateLayout();
        }

        private void GenerateBoardingPassPage(PrintPageDescription printPageDescription)
        {
            var page = _firstPage;
            var rootElement = (FrameworkElement)page.FindName("PrintRoot");

            PreparePrintPage(printPageDescription, page, rootElement);
        }

        private void GenerateReceiptPages(PrintPageDescription printPageDescription, RichTextBlockOverflow previousOverflowContainer = null)
        {
            FrameworkElement page;
            if (previousOverflowContainer == null)
            {
                page = _firstPage;
            }
            else
            {
                page = new ReceiptContinuationPreviewPage(previousOverflowContainer);
            }

            PreparePrintPage(printPageDescription, page, page);

            var overflowContainer = (RichTextBlockOverflow)page.FindName("continuationPageLinkedContainer");
            if (overflowContainer.HasOverflowContent)
            {
                GenerateReceiptPages(printPageDescription, overflowContainer);
            }
        }

        private void PrintDocument_GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            _printDocument.SetPreviewPage(e.PageNumber, _printPages[e.PageNumber - 1]);
        }

        private void PrintDocument_AddPages(object sender, AddPagesEventArgs e)
        {
            foreach (UIElement page in _printPages)
            {
                _printDocument.AddPage(page);
            }

            // Indicate that all of the print pages have been provided
            _printDocument.AddPagesComplete();
        }
    }
}
