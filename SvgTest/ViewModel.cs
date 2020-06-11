using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace SvgTest
{
    public class ViewModel
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
        public SvgImageSource SvgSource
        {
            get { return _svgSource; }
            set
            {
                _svgSource = value;

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
            return await Task.Run(() =>
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
                        
                            _document.Save(memoryStream);

                            var ramStream = new InMemoryRandomAccessStream();
                            memoryStream.CopyTo(ramStream.AsStream());
                            ramStream.Seek(0);
                           
                            return ramStream;
                        }
                    }
                
            }, cancellationToken
            );
        }
    }
}
