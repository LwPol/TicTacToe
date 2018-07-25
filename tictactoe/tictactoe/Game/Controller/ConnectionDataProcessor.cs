using System;
using System.Collections.Generic;
using System.IO;

namespace TicTacToe.Game.Controller
{
    /// <summary>
    /// Provides processing received data and assembling data to be sent via network connection.
    /// </summary>
    public class ConnectionDataProcessor
    {
        private Dictionary<string, INetworkDataProcessor> processingUnits =
            new Dictionary<string, INetworkDataProcessor>();

        /// <summary>
        /// Assembles information to text form.
        /// </summary>
        /// <param name="infoCode">Code of operation.</param>
        /// <param name="infoData">Operation data.</param>
        /// <returns>Text form of information about specified operation.</returns>
        public string Assemble(string infoCode, object infoData)
        {
            try
            {
                return processingUnits[infoCode].Assemble(infoCode, infoData);
            }
            catch (KeyNotFoundException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Processes received data.
        /// </summary>
        /// <param name="data">Data to process.</param>
        public void Process(string data)
        {
            using (Stream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(data);
                writer.Flush();
                stream.Position = 0;

                StreamReader reader = new StreamReader(stream);
                string code = reader.ReadLine();
                string body = reader.ReadToEnd();

                try
                {
                    processingUnits[code].Process(code, body);
                }
                catch (KeyNotFoundException)
                {
                }
            }
        }

        /// <summary>
        /// Registers <see cref="INetworkDataProcessor"/> instance for processing and assembing data about event
        /// coresponding to specified code.
        /// </summary>
        /// <param name="code">Code to register <see cref="INetworkDataProcessor"/> instance to.</param>
        /// <param name="unit"><see cref="INetworkDataProcessor"/> instance to register.</param>
        /// <exception cref="ArgumentNullException">The unit parameter is null.</exception>
        public void RegisterProcessingUnit(string code, INetworkDataProcessor unit)
        {
            if (unit == null)
                throw new ArgumentNullException("unit");
            processingUnits.Add(code, unit);
        }
    }
}
