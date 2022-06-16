using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace DARSJsonWebService.Models.Parameters
{
    [DataContract]
    public class Login
    {
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string password { get; set; }
        [DataMember]
        public string schoolId { get; set; }
        [DataMember]
        public string lang { get; set; }
    }

    [DataContract]
    public class LoginResponse
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public UserLogin data { get; set; }

        public static LoginResponse Create(int status, string msg, UserLogin data)
        {
            LoginResponse obj = new LoginResponse();
            obj.status = status;
            obj.msg = msg;
            obj.data = data;
            return obj;
        }
    }

    [DataContract]
    public class UserLogin
    {
        string _username;
        string _welcomeMessage;
        int _id;
        int _identity;
        int _collegeIdentity;
        string _displayName;
        string _typeUser;
        string _userIsTeacher;
        string _teacherEvaluation;
        string _teacherAgenda;
        string _teacherRemark;
        string _userIsCoordinator;
        string _coordinatorEvaluation;
        string _coordinatorAgenda;
        string _coordinatorRemark;
        string _userIsPrincipal;
        string _principalEvaluation;
        string _principalAgenda;
        string _principalRemark;
        string _userSkillsAccess;
        string _userIsParent;
        string _userIsStudent;
        string _userIsStaff;
        string _allowSendMessage;
        string _allowCoordinatorApproveEvalLog;
        string _image;
        string _language;
        decimal _sessionId;
        string _tokenId;
        int _schoolYear;
        string _showStatement;
        string _showBuses;
        string _RegistraionUrl;

        [DataMember]
        public string username
        {
            get { return _username; }
            set { _username = value; }
        }

        [DataMember]
        public string welcomeMessage
        {
            get { return _welcomeMessage; }
            set { _welcomeMessage = value; }
        }

        [DataMember]
        public int id
        {
            get { return _id; }
            set { _id = value; }
        }

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
        public string displayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        [DataMember]
        public string typeUser
        {
            get { return _typeUser; }
            set { _typeUser = value; }
        }

        [DataMember]
        public string userIsTeacher
        {
            get { return _userIsTeacher; }
            set { _userIsTeacher = value; }
        }

        [DataMember]
        public string teacherEvaluation
        {
            get { return _teacherEvaluation; }
            set { _teacherEvaluation = value; }
        }

        [DataMember]
        public string teacherAgenda
        {
            get { return _teacherAgenda; }
            set { _teacherAgenda = value; }
        }

        [DataMember]
        public string teacherRemark
        {
            get { return _teacherRemark; }
            set { _teacherRemark = value; }
        }

        [DataMember]
        public string userIsCoordinator
        {
            get { return _userIsCoordinator; }
            set { _userIsCoordinator = value; }
        }

        [DataMember]
        public string coordinatorEvaluation
        {
            get { return _coordinatorEvaluation; }
            set { _coordinatorEvaluation = value; }
        }

        [DataMember]
        public string coordinatorAgenda
        {
            get { return _coordinatorAgenda; }
            set { _coordinatorAgenda = value; }
        }

        [DataMember]
        public string coordinatorRemark
        {
            get { return _coordinatorRemark; }
            set { _coordinatorRemark = value; }
        }

        [DataMember]
        public string userIsPrincipal
        {
            get { return _userIsPrincipal; }
            set { _userIsPrincipal = value; }
        }

        [DataMember]
        public string principalEvaluation
        {
            get { return _principalEvaluation; }
            set { _principalEvaluation = value; }
        }

        [DataMember]
        public string principalAgenda
        {
            get { return _principalAgenda; }
            set { _principalAgenda = value; }
        }

        [DataMember]
        public string principalRemark
        {
            get { return _principalRemark; }
            set { _principalRemark = value; }
        }

        [DataMember]
        public string userSkillsAccess
        {
            get { return _userSkillsAccess; }
            set { _userSkillsAccess = value; }
        }

        [DataMember]
        public string userIsParent
        {
            get { return _userIsParent; }
            set { _userIsParent = value; }
        }

        [DataMember]
        public string userIsStudent
        {
            get { return _userIsStudent; }
            set { _userIsStudent = value; }
        }

        [DataMember]
        public string userIsStaff
        {
            get { return _userIsStaff; }
            set { _userIsStaff = value; }
        }

        [DataMember]
        public string allowSendMessage
        {
            get { return _allowSendMessage; }
            set { _allowSendMessage = value; }
        }

        [DataMember]
        public string allowCoordinatorApproveEvalLog
        {
            get { return _allowCoordinatorApproveEvalLog; }
            set { _allowCoordinatorApproveEvalLog = value; }
        }

        [DataMember]
        public string image
        {
            get { return _image; }
            set { _image = value; }
        }

        [DataMember]
        public string language
        {
            get { return _language; }
            set { _language = value; }
        }

        [DataMember]
        public decimal sessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; }
        }
        [DataMember]
        public string tokenId
        {
            get { return _tokenId; }
            set { _tokenId = value; }
        }
        [DataMember]
        public int schoolYear
        {
            get { return _schoolYear; }
            set { _schoolYear = value; }
        }
        [DataMember]
        public string showStatement
        {
            get { return _showStatement; }
            set { _showStatement = value; }
        }
        [DataMember]
        public string showBuses
        {
            get { return _showBuses; }
            set { _showBuses = value; }
        }

        [DataMember]
        public string RegistraionUrl
        {
            get { return _RegistraionUrl; }
            set { _RegistraionUrl = value; }
        }
    }
}