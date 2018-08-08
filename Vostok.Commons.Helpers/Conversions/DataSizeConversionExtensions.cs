using Vostok.Commons.Primitives;

namespace Vostok.Commons.Helpers.Conversions
{
    internal static class DataSizeConversionExtensions
    {
        public static DataSize Bytes(this ushort value) =>
            DataSize.FromBytes(value);

        public static DataSize Bytes(this int value) =>
            DataSize.FromBytes(value);

        public static DataSize Bytes(this long value) =>
            DataSize.FromBytes(value);

        public static DataSize Kilobytes(this ushort value) =>
            DataSize.FromKilobytes(value);

        public static DataSize Kilobytes(this int value) =>
            DataSize.FromKilobytes(value);

        public static DataSize Kilobytes(this long value) =>
            DataSize.FromKilobytes(value);

        public static DataSize Kilobytes(this double value) =>
            DataSize.FromKilobytes(value);

        public static DataSize Megabytes(this ushort value) =>
            DataSize.FromMegabytes(value);

        public static DataSize Megabytes(this int value) =>
            DataSize.FromMegabytes(value);

        public static DataSize Megabytes(this long value) =>
            DataSize.FromMegabytes(value);

        public static DataSize Megabytes(this double value) =>
            DataSize.FromMegabytes(value);

        public static DataSize Gigabytes(this ushort value) =>
            DataSize.FromGigabytes(value);

        public static DataSize Gigabytes(this int value) =>
            DataSize.FromGigabytes(value);

        public static DataSize Gigabytes(this long value) =>
            DataSize.FromGigabytes(value);

        public static DataSize Gigabytes(this double value) =>
            DataSize.FromGigabytes(value);

        public static DataSize Terabytes(this ushort value) =>
            DataSize.FromTerabytes(value);

        public static DataSize Terabytes(this int value) =>
            DataSize.FromTerabytes(value);

        public static DataSize Terabytes(this long value) =>
            DataSize.FromTerabytes(value);

        public static DataSize Terabytes(this double value) =>
            DataSize.FromTerabytes(value);

        public static DataSize Petabytes(this ushort value) =>
            DataSize.FromPetabytes(value);

        public static DataSize Petabytes(this int value) =>
            DataSize.FromPetabytes(value);

        public static DataSize Petabytes(this long value) =>
            DataSize.FromPetabytes(value);

        public static DataSize Petabytes(this double value) =>
            DataSize.FromPetabytes(value);
    }
}