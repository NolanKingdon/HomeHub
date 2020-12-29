using System.Collections.ObjectModel;
using AutoFixture;
using HomeHub.SystemUtils.Models;
using HomeHub.SystemUtils.SystemStorage;
using HomeHub.SystemUtils.SystemTemperature;
using Moq;

namespace HomeHub.Tests.Customizations.System.Storage
{
    public class SystemStorageCustomization : ICustomization
    {
        private readonly StorageUnit unit;
        private readonly double input;

        public SystemStorageCustomization(StorageUnit unit = StorageUnit.Gigabyte, double input = 1)
        {
            this.unit = unit;
            this.input = input;
        }

        private double ConvertResultToDesiredUnits(StorageUnit unit, double input)
        {
            return unit switch
            {
                StorageUnit.Kilobyte => SystemConverter.BytesToKilobytes(input),
                StorageUnit.Megabyte => SystemConverter.BytesToMegabytes(input),
                StorageUnit.Gigabyte => SystemConverter.BytesToGigabytes(input),
                StorageUnit.Terabyte => SystemConverter.BytesToTerabytes(input),
                _ => input,
            };
        }

        private Collection<StorageResult> GenerateStorageResults(IFixture fixture,
                                                                 int resultNo,
                                                                 StorageUnit unit,
                                                                 double input)
        {
            Collection<StorageResult> results = new();
            double totalSpace = ConvertResultToDesiredUnits(unit, input);

            for (int i = 0; i<resultNo; i++)
            {
                StorageResult result = fixture.Create<StorageResult>();
                result.Unit = unit;
                result.TotalSpace = totalSpace;

                results.Add(result);
            }

            return results;
        }

        public void Customize(IFixture fixture)
        {
            const int resultNumber = 5;

            var storageMock = fixture.Freeze<Mock<ISystemStore>>();

            storageMock.Setup( ss => ss.GetAllStorageSpaceAsync())
                       .ReturnsAsync(GenerateStorageResults(fixture, resultNumber, unit, input));

            storageMock.Setup( ss => ss.GetStorageOfDrive(It.IsAny<string>()))
                       .ReturnsAsync(GenerateStorageResults(fixture, 1, unit, input));
        }
    }
}