using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace SvgTest
{
    public class ViewModel:INotifyPropertyChanged
    {
        public ViewModel()
        {
            LoadSvg();
        }
        private SVGGenerator generator;
        private async void LoadSvg()
        {
            generator = new SVGGenerator();
            await generator.LoadSvg();
            var generationCancellationTokenSource = new CancellationTokenSource();
            var souce = await generator.GetSourceAsync(generationCancellationTokenSource.Token);
            SvgSource = new SvgImageSource();
            await SvgSource.SetSourceAsync(souce);
        }
        private SvgImageSource _svgSource;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertName= null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertName));
        }

        public SvgImageSource SvgSource
        {
            get { return _svgSource; }
            set
            {
                _svgSource = value;
                OnPropertyChanged();
            }
        }

    }

    public class SVGGenerator
    {

        private XmlDocument _document;
        public SVGGenerator()
        {

        }
        public async Task LoadSvg()
        {
            _document = new XmlDocument();
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///svg.xml"));
            if (file != null)
            {
                var stream = await file.OpenAsync(FileAccessMode.Read);
                _document.Load(stream.AsStream());
            }
        }
        public async Task<IRandomAccessStream> GetSourceAsync(CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                using (var stringWriter = new StringWriter())
                {
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        Encoding = Encoding.UTF8
                    };
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var xmlTextWriter = XmlWriter.Create(memoryStream, settings))
                        {
                            _document.Save(xmlTextWriter);
                            xmlTextWriter.Flush();
                            var ibuffer = memoryStream.GetWindowsRuntimeBuffer();
                            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                            await randomAccessStream.WriteAsync(ibuffer);
                            randomAccessStream.Seek(0);
                            return randomAccessStream;
                        }
                    }
                }
            }, cancellationToken
            );
        }
    }
}
