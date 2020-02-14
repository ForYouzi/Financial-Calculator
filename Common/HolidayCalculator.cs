namespace Financial_Calculator.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class HolidayCalculator
    {
        private static readonly IList<Holiday> Holidays = new List<Holiday>
        {
            new Holiday
            {
                Name = "春节",
                StartTime = new Date { Month = 1, Day = 1 },
                EndTime = new Date { Month = 1, Day = 6 },
                IsChineseFestival = true
            },
            new Holiday
            {
                Name = "除夕",
                StartTime = new Date { Month = 12, Day = 31 },
                EndTime = new Date { Month = 12, Day = 31 },
                IsChineseFestival = true
            },
            new Holiday
            {
                Name = "清明",
                StartTime = new Date { Month = 4, Day = 4 },
                EndTime = new Date { Month = 4, Day = 6 },
                IsChineseFestival = false
            },
            new Holiday
            {
                Name = "端午",
                StartTime = new Date { Month = 5, Day = 5 },
                EndTime = new Date { Month = 5, Day = 7 },
                IsChineseFestival = true
            },
            new Holiday
            {
                Name = "中秋",
                StartTime = new Date { Month = 8, Day = 15 },
                EndTime = new Date { Month = 8, Day = 17 },
                IsChineseFestival = true
            },
            new Holiday
            {
                Name = "元旦",
                StartTime = new Date { Month = 1, Day = 1 },
                EndTime = new Date { Month = 1, Day = 1 },
                IsChineseFestival = false
            },
            new Holiday
            {
                Name = "劳动节",
                StartTime = new Date { Month = 5, Day = 1 },
                EndTime = new Date { Month = 5, Day = 3 },
                IsChineseFestival = false
            },
            new Holiday
            {
                Name = "国庆节",
                StartTime = new Date { Month = 10, Day = 1 },
                EndTime = new Date { Month = 10, Day = 7 },
                IsChineseFestival = false
            }
        };

        private static bool IsInHolidayRange(Holiday holiday, Date date)
        {
            return holiday.StartTime.Month == date.Month
                   && holiday.StartTime.Day <= date.Day
                   && date.Day <= holiday.EndTime.Day;
        }

        private static DateTime FinishHoliday(DateTime dateTime, Holiday holiday)
        {
            var remain = holiday.EndTime.Day - DateTimeToDate(dateTime, holiday.IsChineseFestival).Day + 1;

            return dateTime.AddDays(remain);
        }

        private static Date DateTimeToDate(DateTime dateTime, bool isTraditional)
        {
            Date date;
            if (isTraditional)
            {
                var chineseCalendar = new ChineseLunisolarCalendar();
                var year = chineseCalendar.GetYear(dateTime);
                var month = chineseCalendar.GetMonth(dateTime);
                var leapMonth = chineseCalendar.GetLeapMonth(year);

                date = new Date
                {
                    Month = leapMonth > 0 && leapMonth <= month ? month - 1 : month,
                    Day = chineseCalendar.GetDayOfMonth(dateTime)
                };
            }
            else
            {
                date = new Date
                {
                    Month = dateTime.Month,
                    Day = dateTime.Day
                };
            }

            return date;
        }

        private static IList<Holiday> IsDateInHoliday(DateTime dateTime)
        {
            var traditionalDate = DateTimeToDate(dateTime, true);
            var standardDate = DateTimeToDate(dateTime, false);

            var holidays = Holidays
                .Where(x => IsInHolidayRange(x, x.IsChineseFestival ? traditionalDate : standardDate))
                .ToList();

            return holidays;
        }

        private static Holiday BuildWeekEnd(DateTime dateTime)
        {
            return new Holiday
            {
                Name = "周末",
                StartTime = new Date
                {
                    Month = dateTime.Month,
                    Day = dateTime.Day
                },
                EndTime = new Date
                {
                    Month = dateTime.AddDays(1).Month,
                    Day = dateTime.AddDays(1).Day
                },
                IsChineseFestival = false
            };
        }

        public static IList<Holiday> PossibleHolidays(DateTime dateTime, out DateTime nextWorkDay)
        {
            var possibleHolidays = new List<Holiday>();

            nextWorkDay = dateTime;

            DateTime lastModifiedDate;
            do
            {
                lastModifiedDate = nextWorkDay;
                var holidays = IsDateInHoliday(nextWorkDay);
                if (holidays.Count > 0)
                {
                    var longestHoliday = dateTime;

                    foreach (var holiday in holidays)
                    {
                        var currentWorkDay = FinishHoliday(nextWorkDay, holiday);
                        if (currentWorkDay.CompareTo(longestHoliday) > 0)
                        {
                            longestHoliday = currentWorkDay;
                        }

                        possibleHolidays.Add(holiday);
                    }

                    nextWorkDay = longestHoliday;
                }
                else
                {
                    switch (nextWorkDay.DayOfWeek)
                    {
                        case DayOfWeek.Saturday:
                        {
                            possibleHolidays.Add(BuildWeekEnd(nextWorkDay));
                            nextWorkDay = nextWorkDay.AddDays(2);
                            break;
                        }
                        case DayOfWeek.Sunday:
                        {
                            possibleHolidays.Add(BuildWeekEnd(dateTime.AddDays(-1)));
                            nextWorkDay = nextWorkDay.AddDays(1);
                            break;
                        }
                    }
                }
            }
            while (lastModifiedDate != nextWorkDay);

            return possibleHolidays;
        }
    }
}
