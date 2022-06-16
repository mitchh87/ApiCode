using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DARSJsonWebService.Models.Responses
{
    [DataContract]
    public class CalendarResponse
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public List<CalendarEvent> data { get; set; }
        public static CalendarResponse Create(int status, string msg, List<CalendarEvent> data)
        {
            CalendarResponse obj = new CalendarResponse();
            obj.status = status;
            obj.msg = msg;
            obj.data = data;
            return obj;
        }
    }

    [DataContract]
    public class CalendarEvent
    {
        string _rowNb;
        int _eventYear;
        int _eventMonth;
        int _eventDay;
        int _mailId;
        string _mailColor;
        string _eventTitle;
        string _eventDescription;

        [DataMember]
        public string rowNb
        {
            get { return _rowNb; }
            set { _rowNb = value; }
        }

        [DataMember]
        public int eventYear
        {
            get { return _eventYear; }
            set { _eventYear = value; }
        }

        [DataMember]
        public int eventMonth
        {
            get { return _eventMonth; }
            set { _eventMonth = value; }
        }

        [DataMember]
        public int eventDay
        {
            get { return _eventDay; }
            set { _eventDay = value; }
        }

        [DataMember]
        public int mailId
        {
            get { return _mailId; }
            set { _mailId = value; }
        }

        [DataMember]
        public string mailColor
        {
            get { return _mailColor; }
            set { _mailColor = value; }
        }

        [DataMember]
        public string eventTitle
        {
            get { return _eventTitle; }
            set { _eventTitle = value; }
        }

        [DataMember]
        public string eventDescription
        {
            get { return _eventDescription; }
            set { _eventDescription = value; }
        }
    }
}