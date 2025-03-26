using System.Data;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace yuelinLib
{
    public class TypeConvert
    {
        private readonly List<Tuple<string, string>> _customDateFormats;
        private readonly List<Tuple<string, string>> _customDateTimeFormats;
        private readonly List<Tuple<string, string>> _customTimeFormats;
        private readonly List<Tuple<string, string>> _validFormats;
        private readonly MemoryCache _cache;

        public TypeConvert()
        {
            _customDateFormats =
            [
                Tuple.Create("\\d{8}", "yyyyMMdd"),
                Tuple.Create("\\d{4}-\\d{2}-\\d{2}", "yyyy-MM-dd"),
                Tuple.Create("\\d{2}-\\d{2}-\\d{4}", "dd-MM-yyyy"),
                Tuple.Create("\\d{2}/\\d{2}/\\d{4}", "dd/MM/yyyy"),
                Tuple.Create("\\d{4}/\\d{2}/\\d{2}", "yyyy/MM/dd"),
                Tuple.Create("\\d{4} \\d{2} \\d{2}", "yyyy MM dd")//
            ];

            _customDateTimeFormats =
            [
                Tuple.Create("\\d{14}", "yyyyMMddHHmmss"),
                Tuple.Create("\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}", "yyyy-MM-dd HH:mm:ss"),
                Tuple.Create("\\d{8} \\d{6}", "yyyyMMdd HHmmss")
            ];

            _customTimeFormats =
            [
                Tuple.Create("\\d{6}", "HHmmss"),
                Tuple.Create("\\d{2}:\\d{2}:\\d{2}", "HH:mm:ss")
            ];

            _validFormats =
            [
                .. _customDateFormats,
                .. _customDateTimeFormats,
                .. _customTimeFormats,
            ];

            _cache = new MemoryCache("TypeConvertCache");
        }

        public T? ConvertTo<T>(string input) where T : struct
        {
            var cacheKey = $"{input}|{typeof(T).FullName}";
            if (_cache.Contains(cacheKey))
            {
                return (T?)_cache.Get(cacheKey);
            }

            object? result = null;
            if (typeof(T) == typeof(DateTime))
            {
                result = ConvertToDateTime(input);
            }
            else if (typeof(T) == typeof(DateOnly))
            {
                result = ConvertToDateOnly(input);
            }
            else if (typeof(T) == typeof(TimeOnly))
            {
                result = ConvertToTimeOnly(input);
            }

            if (result != null)
            {
                _cache.Add(cacheKey, result, DateTimeOffset.Now.AddHours(1));
                return (T)result;
            }

            return null;
        }

        private DateTime? ConvertToDateTime(string input)
        {
            List<Tuple<string, string>> possibleFormats = input.Length switch
            {
                8 => _customDateFormats.FindAll(f => f.Item2.Length == 8),
                14 => _customDateTimeFormats.FindAll(f => f.Item2.Length == 14),
                6 => _customTimeFormats.FindAll(f => f.Item2.Length == 6),
                _ => _validFormats,
            };
            foreach (var formatTuple in possibleFormats)
            {
                if (Regex.IsMatch(input, formatTuple.Item1))
                {
                    if (DateTime.TryParseExact(input, formatTuple.Item2, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        private DateOnly? ConvertToDateOnly(string input)
        {
            List<Tuple<string, string>> possibleFormats = _customDateFormats;
            if (input.Length == 8)
            {
                possibleFormats = _customDateFormats.FindAll(f => f.Item2.Length == 8);
            }

            foreach (var formatTuple in possibleFormats)
            {
                if (Regex.IsMatch(input, formatTuple.Item1))
                {
                    if (DateTime.TryParseExact(input, formatTuple.Item2, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    {
                        return DateOnly.FromDateTime(result);
                    }
                }
            }
            return null;
        }

        private TimeOnly? ConvertToTimeOnly(string input)
        {
            List<Tuple<string, string>> possibleFormats = _customTimeFormats;
            if (input.Length == 6)
            {
                possibleFormats = _customTimeFormats.FindAll(f => f.Item2.Length == 6);
            }

            foreach (var formatTuple in possibleFormats)
            {
                if (Regex.IsMatch(input, formatTuple.Item1))
                {
                    if (DateTime.TryParseExact(input, formatTuple.Item2, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    {
                        return TimeOnly.FromDateTime(result);
                    }
                }
            }
            return null;
        }

        // 并行处理 DataTable 转换的示例方法
        public void ConvertDataTableColumns(DataTable dataTable, string columnName, Type targetType)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.ForEach(dataTable.Rows.Cast<DataRow>(), parallelOptions, row =>
            {
                var input = row[columnName]?.ToString();
                if (input != null)
                {
                    if (targetType == typeof(DateTime))
                    {
                        var converted = ConvertTo<DateTime>(input);
                        if (converted.HasValue)
                        {
                            row[columnName] = converted.Value;
                        }
                    }
                    else if (targetType == typeof(DateOnly))
                    {
                        var converted = ConvertTo<DateOnly>(input);
                        if (converted.HasValue)
                        {
                            row[columnName] = converted.Value;
                        }
                    }
                    else if (targetType == typeof(TimeOnly))
                    {
                        var converted = ConvertTo<TimeOnly>(input);
                        if (converted.HasValue)
                        {
                            row[columnName] = converted.Value;
                        }
                    }
                }
            });
        }
    }
}
