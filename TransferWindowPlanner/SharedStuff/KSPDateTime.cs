﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public class KSPDateTime
    {
        //Define the Epoch
        static public int EpochDay { get { return KSPDateTimeStructure.EpochDay; } set { KSPDateTimeStructure.EpochDay = value; } }
        static public int EpochYear { get; set; }

        //Define the Calendar
        static public int SecondsPerMinute { get; set; }
        static public int MinutesPerHour { get; set; }
        static public int HoursPerDay { get; set; }
        static public int DaysPerYear { get; set; }

        static public int SecondsPerHour { get { return SecondsPerMinute * MinutesPerHour; } }
        static public int SecondsPerDay { get { return SecondsPerHour * HoursPerDay; } }
        static public int SecondsPerYear { get { return SecondsPerDay * DaysPerYear; } }


        
        //Descriptors of DateTime - uses UT as the Root value
        public int Year { 
            get { return EpochYear + (Int32)UT / SecondsPerYear; }
            set { UT = UT - (Year - EpochYear) * SecondsPerYear + (value - EpochYear) * SecondsPerYear; } 
        }
        public int Day { 
            get { return EpochDay + (Int32)UT / SecondsPerDay % SecondsPerYear; } 
            set { UT = UT - (Day-EpochDay) * SecondsPerDay + (value - EpochDay) * SecondsPerDay; } 
        }
        public int Hour { 
            get { return (Int32)UT / SecondsPerHour % SecondsPerDay; } 
            set { UT = UT - Hour * SecondsPerHour + value * SecondsPerHour; } 
        }
        public int Minute {
            get { return (Int32)UT / SecondsPerMinute % SecondsPerHour; } 
            set { UT = UT - Minute * SecondsPerMinute + value* SecondsPerMinute; } 
        }
        public int Second { 
            get { return (Int32)UT % SecondsPerMinute; } 
            set { UT = UT - Second + value; } 
        }
        public int Millisecond { 
            get { return (Int32) (Math.Round(UT - Math.Floor(UT), 3) * 1000); } 
            set { UT = Math.Floor(UT) + ((Double)value / 1000); } 
        }

        /// <summary>
        /// Replaces the normal "Ticks" function. This is Seconds of UT since game time 0
        /// </summary>
        public Double UT { get { return _UT; } set { _UT = value; } }

        private Double _UT;


        #region Constructors
        static KSPDateTime()
        {
            EpochYear = 1;
            EpochDay = 1;
            SecondsPerMinute = 60;
            MinutesPerHour = 60;
            HoursPerDay = 6;
            DaysPerYear = 425;
        }


        public KSPDateTime()
        {
            _UT = 0;

        }
        public KSPDateTime(int year, int day)
            : this()
        {
            Year = year; Day = day;
        }
        public KSPDateTime(int year, int day, int hour, int minute, int second)
            : this(year, day)
        {
            Hour = hour; Minute = minute; Second = second;
        }
        public KSPDateTime(int year, int day, int hour, int minute, int second, int millisecond)
            : this(year, day, hour, minute, second)
        {
            Millisecond = millisecond;
        }

        public KSPDateTime(Double ut)
            : this()
        {
            UT = ut;
        } 
        #endregion


        #region Calculated Properties
        public KSPDateTime Date { get { return new KSPDateTime(Year, Day); } }
        public KSPTimeSpan TimeOfDay { get { return new KSPTimeSpan(UT % SecondsPerDay); } }


        public static KSPDateTime Now {
            get { return new KSPDateTime(Planetarium.GetUniversalTime()); }
        }
        public static KSPDateTime Today {
            get { return new KSPDateTime(Planetarium.GetUniversalTime()).Date; }
        }
        #endregion


        #region Instance Methods
        #region Mathematic Methods
        public KSPDateTime Add(KSPTimeSpan value)
        {
            return new KSPDateTime(UT + value.UT);
        }
        public KSPDateTime AddYears(Double value)
        {
            return new KSPDateTime(UT + value * SecondsPerYear);
        }
        public KSPDateTime AddDays(Double value)
        {
            return new KSPDateTime(UT + value * SecondsPerDay);
        }
        public KSPDateTime AddHours(Double value)
        {
            return new KSPDateTime(UT + value * SecondsPerHour);
        }
        public KSPDateTime AddMinutes(Double value)
        {
            return new KSPDateTime(UT + value * SecondsPerMinute);
        }
        public KSPDateTime AddSeconds(Double value)
        {
            return new KSPDateTime(UT + value);
        }
        public KSPDateTime AddMilliSeconds(Double value)
        {
            return new KSPDateTime(UT + value / 1000);
        }
        public KSPDateTime AddUT(Double value)
        {
            return new KSPDateTime(UT + value);
        }

        public KSPDateTime Subtract(KSPDateTime value)
        {
            return new KSPDateTime(UT - value.UT);
        }
        public KSPTimeSpan Subtract(KSPTimeSpan value)
        {
            return new KSPTimeSpan(UT - value.UT);
        }

        #endregion


        #region Comparison Methods
        public Int32 CompareTo(KSPDateTime value) {
            return KSPDateTime.Compare(this, value);
        }
        public Int32 CompareTo(System.Object value) {
            return this.CompareTo((KSPDateTime)value);
        }
        public Boolean Equals(KSPDateTime value) {
            return KSPDateTime.Equals(this, value);
        }
        public override bool Equals(System.Object obj) {
            return this.Equals((KSPDateTime)obj);
        }
        #endregion        
        

        public override int GetHashCode()
        {
            return UT.GetHashCode();
        }

        #endregion


        #region Static Methods
        public static Int32 Compare(KSPDateTime t1, KSPDateTime t2)
        {
            if (t1.UT < t2.UT)
                return -1;
            else if (t1.UT > t2.UT)
                return 1;
            else
                return 0;
        }
        public static Boolean Equals(KSPDateTime t1, KSPDateTime t2)
        {
            return t1.UT == t2.UT;
        }


        #endregion

        public static KSPTimeSpan operator -(KSPDateTime d1, KSPDateTime d2) {
            return new KSPTimeSpan(d1.UT - d2.UT);
        }
        public static KSPDateTime operator -(KSPDateTime d, KSPTimeSpan t) {
            return new KSPDateTime(d.UT - t.UT);
        }
        public static KSPDateTime operator +(KSPDateTime d, KSPTimeSpan t) {
            return new KSPDateTime(d.UT + t.UT);
        }
        public static Boolean operator !=(KSPDateTime d1, KSPDateTime d2) {
            return !(d1 == d2);
        }
        public static Boolean operator ==(KSPDateTime d1, KSPDateTime d2) {
            return d1.UT == d2.UT;
        }



        public static Boolean operator <=(KSPDateTime d1, KSPDateTime d2) {
            return d1.CompareTo(d2)<=0;
        }
        public static Boolean operator <(KSPDateTime d1, KSPDateTime d2)
        {
            return d1.CompareTo(d2) < 0;
        }
        public static Boolean operator >=(KSPDateTime d1, KSPDateTime d2)
        {
            return d1.CompareTo(d2) >= 0;
        }
        public static Boolean operator >(KSPDateTime d1, KSPDateTime d2)
        {
            return d1.CompareTo(d2) > 0;
        }

        //DaysInMonth
        //Day - is Day Of Month
        //DayOfYear
        //IsLeapYear



        //From


        //To
    }



    public class KSPMonth
    {
        public int Days { get; set; }
        public String Name { get; set; }
    }
}
