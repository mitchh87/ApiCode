using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace DARSJsonWebService.Models.Responses
{
    [DataContract]
    public class AbsenceResponse
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public List<Absence> data { get; set; }

        public static AbsenceResponse Create(int status, string msg, List<Absence> data)
        {
            AbsenceResponse obj = new AbsenceResponse();
            obj.status = status;
            obj.msg = msg;
            obj.data = data;
            return obj;
        }
    }

    [DataContract]
    public class Absence
    {
        int _ID;
        string _RemarkType;
        string _Description;
        string _RemarkDate;
        short _ColumnNumber;
        string _ColumnName;
        string _AbsColor;
        int _ContainsArabic;

        [DataMember]
        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        [DataMember]
        public string RemarkType
        {
            get { return _RemarkType; }
            set { _RemarkType = value; }
        }

        [DataMember]
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        [DataMember]
        public string RemarkDate
        {
            get { return _RemarkDate; }
            set { _RemarkDate = value; }
        }

        [DataMember]
        public short ColumnNumber
        {
            get { return _ColumnNumber; }
            set { _ColumnNumber = value; }
        }

        [DataMember]
        public string ColumnName
        {
            get { return _ColumnName; }
            set { _ColumnName = value; }
        }

        [DataMember]
        public string AbsColor
        {
            get { return _AbsColor; }
            set { _AbsColor = value; }
        }

        [DataMember]
        public int ContainsArabic
        {
            get { return _ContainsArabic; }
            set { _ContainsArabic = value; }
        }
    }
}