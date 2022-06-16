using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DARSJsonWebService.Models.Responses
{
    [DataContract]
    public class AgendaListResponse
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public List<AgendaList> data { get; set; }

        public static AgendaListResponse Create(int status, string msg, List<AgendaList> data)
        {
            AgendaListResponse obj = new AgendaListResponse();
            obj.status = status;
            obj.msg = msg;
            obj.data = data;
            return obj;
        }
    }

    public class AgendaList
    {
        int _DayNb;
        string _Date;
        string _actualDate;
        string _classId;
        string _className;
        int _AllowEdit;
        int _AllowAdd;
        int _AllowApprove;
        string _uType;
        string _DateJour;
        List<AgendaDay> _LstAgendaDay;

        [DataMember]
        public int DayNb
        {
            get { return _DayNb; }
            set { _DayNb = value; }
        }
        [DataMember]
        public string Date
        {
            get { return _Date; }
            set { _Date = value; }
        }
        [DataMember]
        public string DateJour
        {
            get { return _DateJour; }
            set { _DateJour = value; }
        }

        [DataMember]
        public string actualDate
        {
            get { return _actualDate; }
            set { _actualDate = value; }
        }

        [DataMember]
        public string classId
        {
            get { return _classId; }
            set { _classId = value; }
        }

        [DataMember]
        public string className
        {
            get { return _className; }
            set { _className = value; }
        }

        [DataMember]
        public int AllowEdit
        {
            get { return _AllowEdit; }
            set { _AllowEdit = value; }
        }

        [DataMember]
        public int AllowAdd
        {
            get { return _AllowAdd; }
            set { _AllowAdd = value; }
        }

        [DataMember]
        public int AllowApprove
        {
            get { return _AllowApprove; }
            set { _AllowApprove = value; }
        }

        [DataMember]
        public string uType
        {
            get { return _uType; }
            set { _uType = value; }
        }

        [DataMember]
        public List<AgendaDay> LstAgendaDay
        {
            get { return _LstAgendaDay; }
            set { _LstAgendaDay = value; }
        }
    }
}