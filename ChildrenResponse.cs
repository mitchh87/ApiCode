using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace DARSJsonWebService.Models.Responses
{
    [DataContract]
    public class ChildrenResponse
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public List<Children> data { get; set; }

        public static ChildrenResponse Create(int status, string msg, List<Children> data)
        {
            ChildrenResponse obj = new ChildrenResponse();
            obj.status = status;
            obj.msg = msg;
            obj.data = data;
            return obj;
        }
    }

    [DataContract]
    public class Children
    {
        int _identity;
        int _collegeIdentity;
        string _username;
        string _displayName;
        string _firstName;
        string _classe;
        string _classCode;
        int _studentIdentity;
        int _parentIdentity;
        string _image;
        decimal _sessionId;
        string _onlineUrl;
        bool _agenda;
        bool _evaluation;
        bool _remarks;
        bool _reportCard;
        bool _absences;
        bool _skills;
        bool _timeline;
        bool _forum;
        bool _quiz;

        [DataMember]
        public int identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        [DataMember]
        public int collegeIdentity
        {
            get { return _collegeIdentity; }
            set { _collegeIdentity = value; }
        }

        [DataMember]
        public string username
        {
            get { return _username; }
            set { _username = value; }
        }

        [DataMember]
        public string displayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        [DataMember]
        public string firstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        [DataMember]
        public string classe
        {
            get { return _classe; }
            set { _classe = value; }
        }

        [DataMember]
        public string classCode
        {
            get { return _classCode; }
            set { _classCode = value; }
        }

        [DataMember]
        public int studentIdentity
        {
            get { return _studentIdentity; }
            set { _studentIdentity = value; }
        }

        [DataMember]
        public int parentIdentity
        {
            get { return _parentIdentity; }
            set { _parentIdentity = value; }
        }

        [DataMember]
        public string image
        {
            get { return _image; }
            set { _image = value; }
        }

        [DataMember]
        public decimal sessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; }
        }

        [DataMember]
        public bool agenda
        {
            get { return _agenda; }
            set { _agenda = value; }
        }

        [DataMember]
        public bool evaluation
        {
            get { return _evaluation; }
            set { _evaluation = value; }
        }

        [DataMember]
        public bool remarks
        {
            get { return _remarks; }
            set { _remarks = value; }
        }

        [DataMember]
        public bool reportCard
        {
            get { return _reportCard; }
            set { _reportCard = value; }
        }

        [DataMember]
        public bool absences
        {
            get { return _absences; }
            set { _absences = value; }
        }

        [DataMember]
        public bool skills
        {
            get { return _skills; }
            set { _skills = value; }
        }

        [DataMember]
        public bool timeline
        {
            get { return _timeline; }
            set { _timeline = value; }
        }

        [DataMember]
        public bool forum
        {
            get { return _forum; }
            set { _forum = value; }
        }

        [DataMember]
        public bool quiz
        {
            get { return _quiz; }
            set { _quiz = value; }
        }
    }
}