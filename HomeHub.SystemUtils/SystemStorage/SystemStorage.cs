using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HomeHub.SystemUtils.Configuration;
using HomeHub.SystemUtils.Models;
using HomeHub.SystemUtils.SystemTemperature;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeHub.SystemUtils.SystemStorage
{
    /// <summary>
    /// Linux only at the moment.
    /// </summary>
    public class StorageHelper : ISystemStore
    {
        private readonly ILogger<StorageHelper> logger;
        private readonly StorageOptions options;

        public StorageHelper(ILogger<StorageHelper> logger, IOptions<StorageOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        /// <summary>
        /// Returns the array of all the storage space objects/mounts on the linux system.
        /// </summary>
        public async Task<Collection<StorageResult>> GetAllStorageSpaceAsync()
        {
            string output;

            using (SystemProcess process = new())
            {
                output = await process.RunCommand(options.CommandInterface, options.CommandArgs);
            }

            return await GenerateModelsAsync(output);
        }

        /// <summary>
        /// Debug instance. To be used in the likely chance my understanding of the linux output is wrong, and I
        /// do not receive the text as expected.
        /// </summary>
        public async Task<Collection<StorageResult>> GetAllStorageSpaceRAWAsync()
        {
            string output;

            using (SystemProcess process = new())
            {
                output = await process.RunCommand(options.CommandInterface, options.CommandArgs);
            }

            return await GenerateModelsAsync(output);
        }

        public async Task<Collection<StorageResult>> GetStorageOfDrive(string drive)
        {
            string output;
            string args = $"{options.CommandArgs} {drive}";

            using (SystemProcess process = new())
            {
                output = await process.RunCommand(options.CommandInterface, args);
            }

            return await GenerateModelsAsync(output);
        }

        /// <summary>
        /// Converts the raw output from a 'df -B 1' command in a QTerminal (Ubuntu 20.04) into our model.
        /// Splits the raw output by newlines, then by tabs to get at the headers and put it into an easy to work
        /// with model.
        /// </summary>
        /// <param name="raw">The raw string output of the QTerminal</param>
        /// <returns>A collection of StorageResults</returns>
        private async Task<Collection<StorageResult>> GenerateModelsAsync(string raw)
        {
            Collection<StorageResult> results = new();
            string[] lines = raw.Split("\n");

            // Combing through our output.
            for (int i = 0; i < lines.Length; i++)
            {
                // Ignoring out header lines.
                if (i != 0)
                {
                    // TODO - If it's going to break, it'll break here. I have no guarantee the raw encoding is a \t.
                    // FileSystem, Blocks, Used, Available, Use%, Mounted On.
                    string[] rowResults = lines[i].Split('\t');
                    double bytes = double.Parse(rowResults[1]);
                    double space = ConvertToOptionsUnit(bytes, options.Unit);

                    results.Add(new StorageResult()
                    {
                        FileSystem = rowResults[0],
                        TotalSpace = space,
                        Unit = options.Unit,
                        UsedSpacePercent = double.Parse(rowResults[4])
                    });
                }
            }

            return results;
        }

        private double ConvertToOptionsUnit(double bytes, StorageUnit unit)
        {
            return unit switch
            {
                StorageUnit.Kilobyte => SystemConverter.BytesToKilobytes(bytes),
                StorageUnit.Megabyte => SystemConverter.BytesToMegabytes(bytes),
                StorageUnit.Gigabyte => SystemConverter.BytesToGigabytes(bytes),
                StorageUnit.Terabyte => SystemConverter.BytesToTerabytes(bytes),
                _ => bytes,
            };
        }
    }
}
