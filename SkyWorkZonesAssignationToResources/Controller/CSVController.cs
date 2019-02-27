using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkZoneLoad.Controller
{
    public class CSVController
    {
        public string source { get; set; }

        public async Task<List<string>> LinesFile()
        {
            List<string> listResult = new List<string>();
            try
            {
                using (var reader = new StreamReader(this.source))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        listResult.Add(line);
                    }
                    return listResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error {0}, detalles: {1}", ex.Message, ex.InnerException.Message));
            }
            return null;
        }

        public string[] SplitBy(string text, char character)
        {
            string[] aResult;
            try
            {
                aResult = text.Split(character);
                return aResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error {0} {1}", ex.Message, ex.InnerException.Message));

            }
            return null;
        }
    }
}
