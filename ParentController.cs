using DARSJsonWebService.DBContext;
using DARSJsonWebService.DBContext.Admin;
using DARSJsonWebService.DBContext.Front;
using DARSJsonWebService.Models.CustomObjects;
using DARSJsonWebService.Models.Parameters;
using DARSJsonWebService.Models.Responses;
using DARSJsonWebService.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;

namespace DARSJsonWebService.Controllers
{
    [RoutePrefix("api/parent")]
    public class ParentController : ApiController
    {
        private static int IsArabic(string inputstring)
        {
            char[] chars = inputstring.ToCharArray();
            foreach (char ch in chars)
                if (ch >= '\u0627' && ch <= '\u0649')
                    return 1;
            return 0;
        }

        [HttpPost]
        public ChildrenResponse GetFamilyChildren(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return ChildrenResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return ChildrenResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(param.collegeId), "ONLINE_URL");

                        comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), Convert.ToInt32(param.collegeId), ti.ID, 0, "children");

                        if (ti.LoginLevel != 4) // PARENT
                            return ChildrenResponse.Create(0, param.language == "EN" ? "Access denied" : param.language == "FR" ? "Accès refusé" : "غير مسموح بالدخول", null);
                        else
                        {
                            List<Children> childs = new List<Children>();

                            FrontComm frntComm = new FrontComm(ConnectionString);
                            List<Ent_FamilleElevesResult> eleves = frntComm.Get_FamilleEleves(Convert.ToInt32(ti.Id_ISIS), Convert.ToInt32(ti.Id_College), Annee);

                            if (eleves.Count > 0)
                            {
                                foreach (Ent_FamilleElevesResult ele in eleves)
                                {
                                    Children child = new Children();
                                    child.identity = ele.ID;
                                    child.collegeIdentity = ele.Id_College.Value;
                                    child.username = ele.Username;
                                    child.displayName = ele.DisplayName;
                                    child.firstName = ele.Prenom;
                                    child.classe = ele.UserInfo;
                                    child.classCode = ele.Classe;
                                    child.studentIdentity = ele.Id_ISIS.Value;
                                    child.parentIdentity = Convert.ToInt32(ti.ID_User);
                                    child.image = ONLINE_URL + ele.ImageURL.Replace("~/", "");
                                    child.sessionId = Convert.ToInt32(ti.ID_User);
                                    child.agenda = ele.Agenda;
                                    child.evaluation = ele.Evaluation;
                                    child.remarks = ele.Remarques;
                                    child.reportCard = ele.Carnet;
                                    child.absences = ele.Absences;
                                    child.skills = ele.Competence;
                                    child.timeline = ele.Timeline;
                                    child.forum = ele.Forum;
                                    child.quiz = true;

                                    childs.Add(child);
                                }
                                return ChildrenResponse.Create(1, "", childs);
                            }
                            else
                                return ChildrenResponse.Create(0, param.language == "EN" ? "No children found" : param.language == "FR" ? "Aucun enfant trouvé" : "لم يتم العثور على أي طفل", new List<Children>());
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return ChildrenResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetFamilyChildren", exp.Message);

                    return ChildrenResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }


        [HttpPost]
        public StoryResponse GetStories(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return StoryResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);

                    if (ti == null)
                        return StoryResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(param.collegeId), "ONLINE_URL");

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, 0, "Stories");

                        List<Story> lstStory = new List<Story>();
                        List<Mob_StoryBoardResult> lst = frntComm.GetStoryBoard(Convert.ToInt32(param.collegeId), ti.ID_User.Value,
                            ti.Id_ISIS.Value, Annee, Convert.ToInt32(param.PageSize), Convert.ToInt32(param.PageNumber));

                        if (lst.Count > 0)
                        {
                            string StoryRem = (param.language == "FR" ? "Remarque" : param.language == "EN" ? "Remark" : "ملاحظات");
                            string StoryNews = (param.language == "FR" ? "Nouvelle" : param.language == "EN" ? "News" : "أخبار");
                            string StoryEval = (param.language == "FR" ? "Evaluation" : param.language == "EN" ? "Evaluation" : "تقييم");
                            string StoryAgenda = (param.language == "FR" ? "Agenda" : param.language == "EN" ? "Agenda" : "فرض");
                            string StoryForum = (param.language == "FR" ? "Forum" : param.language == "EN" ? "Forum" : "");
                            string StoryAbs = (param.language == "FR" ? "Absence" : param.language == "EN" ? "Absence" : "غياب");

                            CultureInfo ci;

                            if (param.language == "AR")
                                ci = new CultureInfo("ar-LB");
                            else if (param.language == "FR")
                                ci = new CultureInfo("fr-FR");
                            else
                                ci = new CultureInfo("en-US");

                            foreach (Mob_StoryBoardResult dbstory in lst)
                            {
                                Story st = new Story();
                                st.ID = dbstory.ID.Value;
                                st.StoryType = dbstory.StoryType == "NEWS" && dbstory.Id_MailType == 5 ? "SURVEY" : dbstory.StoryType;
                                st.StoryTitle = (dbstory.StoryType == "REM" ? StoryRem :
                                                (dbstory.StoryType == "NEWS" ? StoryNews :
                                                (dbstory.StoryType == "EVAL" ? StoryEval :
                                                (dbstory.StoryType == "AGENDA" ? StoryAgenda :
                                                (dbstory.StoryType == "FORUM" ? StoryForum :
                                                (dbstory.StoryType == "ABS" ? StoryAbs : ""))))));

                                //Used only for news to show the uploaded files
                                if (dbstory.StoryText.Contains("/RadControls/Editor/Files") || dbstory.StoryText.Contains("/Uploads/"))
                                {
                                    st.StoryText = dbstory.StoryText.Replace("href=\"/", ONLINE_URL + "href=\"" + ONLINE_URL + "/");
                                    st.StoryText = dbstory.StoryText.Replace("src=\"/", ONLINE_URL + "src=\"" + ONLINE_URL + "/");
                                }
                                else
                                    st.StoryText = dbstory.StoryText;

                                st.StoryDate = dbstory.StoryDate.Value.ToString("MMM dd", ci);
                                st.StoryFullDate = dbstory.StoryDate.Value.ToString("yyyy-MM-dd");
                                // the story is a circulaire remove the sender name
                                st.DisplayName = dbstory.Id_MailType == 2 ? dbstory.StorySubject : dbstory.DisplayName;
                                // the story is a circulaire show the school cover image
                                st.ImageURL = dbstory.Id_MailType == 2 ? ONLINE_URL + dbstory.NewsCover.Replace("~", "") : ONLINE_URL + dbstory.ImageURL.Replace("~", "");
                                st.Subject = dbstory.Id_MailType == 2 ? "" : dbstory.StorySubject;
                                st.StudentIdentity = dbstory.Id_Student.Value;
                                st.isRead = dbstory.isRead.Value ? 1 : 0;
                                st.IconCss = (dbstory.StoryType == "REM" ? "alert-circle-outline" :
                                             (dbstory.StoryType == "EVAL" ? "document-outline" :
                                             (dbstory.StoryType == "AGENDA" ? "calendar-outline" :
                                             (dbstory.StoryType == "ABS" ? "remove-circle-outline" :
                                             (dbstory.StoryType == "HOMEWORK" ? "library_books" : "")))));
                                st.Id_MailType = dbstory.Id_MailType;

                                st.pinned = dbstory.Pinned != null && dbstory.Pinned != false ? "1" : "0";
                                lstStory.Add(st);
                            }

                            return StoryResponse.Create(1, "", lstStory);
                        }
                        else
                        {
                            return StoryResponse.Create(0, "", new List<Story>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return StoryResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetStories", e.Message);

                    return StoryResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public AbsenceResponse GetAbsence(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return AbsenceResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return AbsenceResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        List<Mob_StudentAbsenceResult> lstAbs = frntComm.GetStudentAbsence(param.studentId, Annee, Convert.ToInt32(param.collegeId));

                        comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, Convert.ToInt32(param.studentId), "Absence");

                        CultureInfo ci;

                        if (param.language == "AR")
                            ci = new CultureInfo("ar-LB");
                        else if (param.language == "FR")
                            ci = new CultureInfo("fr-FR");
                        else
                            ci = new CultureInfo("en-US");

                        List<Absence> AddRemarquesEleve = new List<Absence>();
                        if (lstAbs.Count > 0)
                        {
                            foreach (Mob_StudentAbsenceResult Rem in lstAbs)
                            {
                                Absence EleveRmq = new Absence();
                                EleveRmq.ID = Rem.ID;
                                EleveRmq.ColumnName = Rem.LibelleColonne;
                                EleveRmq.RemarkType = Rem.Type_Remarque;
                                EleveRmq.Description = Rem.Description;
                                EleveRmq.RemarkDate = Rem.Date_Remarque.Value.ToString("MMM dd", ci);
                                EleveRmq.ColumnNumber = Convert.ToInt16(Rem.NumColonne);
                                EleveRmq.AbsColor = Rem.Color;
                                EleveRmq.ContainsArabic = IsArabic(Rem.Description);

                                AddRemarquesEleve.Add(EleveRmq);
                            }
                            return AbsenceResponse.Create(1, "", AddRemarquesEleve);
                        }
                        else
                        {
                            return AbsenceResponse.Create(0, param.language == "EN" ? "No remark is added" : param.language == "FR" ? "Aucune remarque n'est ajoutée" : "لا يوجد أية ملاحظة", new List<Absence>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return AbsenceResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetAbsence", e.Message);

                    return AbsenceResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public SkillsTermsResponse GetStudentSkillsTerms(TokenParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return SkillsTermsResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return SkillsTermsResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        TeacherComm frntComm = new TeacherComm(ConnectionString);
                        AdminComm admCom = new AdminComm(ConnectionString);
                        int Annee = admCom.GetCurrentYear(Convert.ToInt32(param.collegeId));

                        List<Ent_LibelleNumColonneResult> lstNumColonne = frntComm.GetEnt_LibelleNumColonne(Convert.ToInt32(param.collegeId),
                            0, "", Annee, null, 0, param.studentId, false, false, true, false);
                        List<Terms> AddNumColonne = new List<Terms>();

                        Terms def = new Terms();
                        def.ID = "0";
                        def.ColumnName = (param.language == "FR" ? "Colonne" : param.language == "EN" ? "Column" : "خانة");
                        AddNumColonne.Add(def);

                        if (lstNumColonne.Count > 0)
                        {
                            foreach (Ent_LibelleNumColonneResult NumCol in lstNumColonne)
                            {
                                Terms col = new Terms();
                                col.ID = NumCol.Numcolonne.ToString();
                                col.ColumnName = NumCol.libelle;

                                AddNumColonne.Add(col);
                            }
                            return SkillsTermsResponse.Create(1, "", AddNumColonne);
                        }
                        else
                        {
                            return SkillsTermsResponse.Create(0, "", new List<Terms>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return SkillsTermsResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetStudentSkillsTerms", e.Message);

                    return SkillsTermsResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public SkillsResponse GetStudentSkills(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return SkillsResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return SkillsResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        List<Fct_StudentView> ele = frntComm.getIsc_ViewEleveByID(Convert.ToInt32(param.collegeId), Convert.ToInt32(param.studentId), Annee);
                        if (ele.Count == 0)
                            return SkillsResponse.Create(-100, param.language == "EN" ? "Invalid student" : param.language == "FR" ? "Étudiant invalide" : "طالب غير صالح", null);
                        else
                        {
                            comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, Convert.ToInt32(param.studentId), "Skills");

                            if (param.columnNum != "")
                            {
                                List<Skills> AddSkills = new List<Skills>();
                                List<CMPS_RptBulletinAPIPreviewResult> lstSkills = frntComm.GetRptBulletin(Convert.ToInt32(param.collegeId),
                                    Convert.ToInt32(param.studentId), Annee, ele[0].Classe, Convert.ToInt32(param.columnNum));

                                if (lstSkills.Count > 0)
                                {
                                    foreach (CMPS_RptBulletinAPIPreviewResult SkillsEleve in lstSkills)
                                    {
                                        Skills sk = new Skills();
                                        sk.Level = SkillsEleve.SkillLevelRank.ToString();
                                        sk.Description = SkillsEleve.skillItem;
                                        sk.Result = SkillsEleve.IsGraded ? SkillsEleve.Result : "";
                                        sk.IsGraded = SkillsEleve.IsGraded ? "1" : "0";
                                        sk.Ara = SkillsEleve.ItemAra;
                                        sk.Color = SkillsEleve.SkillColor;

                                        AddSkills.Add(sk);
                                    }
                                    return SkillsResponse.Create(1, "", AddSkills);
                                }
                                else
                                    return SkillsResponse.Create(0, param.language == "EN" ? "no skills is added" : param.language == "FR" ? "aucune compétence n'est ajoutée" : "لا يوجد أي مهارات", new List<Skills>());
                            }
                            else
                                return SkillsResponse.Create(-1, "", null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return SkillsResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetStudentSkills", e.Message);

                    return SkillsResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public StatementResponse GetFamilyStatement(TokenParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return StatementResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm admCom = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = admCom.tokenInfo(param.token);
                    if (ti == null)
                        return StatementResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        admCom.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, 0, "Statement");

                        List<Statement> Addstatement = new List<Statement>();
                        FrontComm frntComm = new FrontComm(ConnectionString);

                        List<Mob_ReleveFamilleResult> statement = frntComm.Get_Rpt_ReleveFamille(ti.Id_ISIS.Value, Convert.ToInt32(param.collegeId), true);

                        if (statement.Count > 0)
                        {
                            CultureInfo ci;

                            if (param.language == "AR")
                                ci = new CultureInfo("ar-LB");
                            else if (param.language == "FR")
                                ci = new CultureInfo("fr-FR");
                            else
                                ci = new CultureInfo("en-US");

                            foreach (Mob_ReleveFamilleResult stat in statement)
                            {
                                Statement st = new Statement();
                                st.ID = stat.ID;
                                st.FamilyID = Convert.ToInt32(stat.ID_Famille);
                                st.Number = stat.Numero;
                                st.Reference =
                                    stat.Type_Document == "Facture " ? (param.language == "EN" ? "Invoice " : param.language == "FR" ? "Facture " : "فاتورة رقم ") + stat.Numero
                                    : stat.Type_Document == "Reçu " ? (param.language == "EN" ? "Receipt " : param.language == "FR" ? "Reçu " : "وصل ") + stat.Refecence + " " + stat.Numero
                                    : param.language == "AR" ? stat.Refecence.Replace("Balance", "رصيد") : stat.Refecence;
                                st.DbCr = stat.Sens;
                                st.DateDocument = stat.Date_Document == null ? "" : stat.Date_Document.Value.ToString("MMM dd", ci);
                                st.TypeDocument = stat.Type_Document;
                                st.Balance = Convert.ToDecimal(stat.Solde);
                                st.Ccy = stat.Devise;
                                st.Total = Convert.ToDecimal(stat.Total);

                                Addstatement.Add(st);
                            }
                            return StatementResponse.Create(1, "", Addstatement);
                        }
                        else
                        {
                            return StatementResponse.Create(0, "", new List<Statement>());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return StatementResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetFamilyStatement", ex.Message);

                    return StatementResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public BusesResponse BusesList()
        {
            List<Buses> bus = new List<Buses>();
            bus.Add(new Buses("", ""));
            bus.Add(new Buses("bus 2", "http://track.maliatec.com/locator/index.html?t=a446666c8ee25ae074a387790ff08faf5743CA0D1E13B9065F1B000FA221C1E995A55DFC"));
            bus.Add(new Buses("bus 3", "http://track.maliatec.com/locator/index.html?t=3dd79947f3b9405e23eb4359e83a174aDFCDD151B1EB21ABD55E17CF2A7C8AEB99DC1612"));
            bus.Add(new Buses("bus 4", "http://track.maliatec.com/locator/index.html?t=9f215b6f2a4a8240bd4e80e3f42a04568ED9CCEDE995F41BAA6A5F60DFDE078D0A7C58E6"));
            bus.Add(new Buses("bus 5", "http://track.maliatec.com/locator/index.html?t=4dab1bf801a80f5e8a7962b2fe34d283C346189ADC1D7D8A4E3A535D32FB97F2DB541847"));
            bus.Add(new Buses("bus 8", "http://track.maliatec.com/locator/index.html?t=f7df4668445e5a319ad48af73e90bc03876E97CB57A236D6E196573D4936AD28319669F6"));
            bus.Add(new Buses("bus 9", "http://track.maliatec.com/locator/index.html?t=2e9c6244df612071176808e4f3c1904220AF56C7ECF43B026FF8CA0D82314825339BCE1B"));
            bus.Add(new Buses("bus 10", "http://track.maliatec.com/locator/index.html?t=ce504af3baf6c6c8201be3841a483a728178F53D984870D3F2FA3AD14E22055E121D4F10"));
            bus.Add(new Buses("bus 11", "http://track.maliatec.com/locator/index.html?t=d09cef3d236e5744712ce6feefd3d379F81D31F82B663F859D79E254E0B12E938C4272E1"));
            bus.Add(new Buses("bus 13", "http://track.maliatec.com/locator/index.html?t=0930d5c9b1ed1bc34780334fc9a0c2cd998A61E60593D5B75DF1518EB371023DE9234D4D"));
            bus.Add(new Buses("bus 14", "http://track.maliatec.com/locator/index.html?t=3d584a9444f8a535b87de0b739be2da05CADEA5555B187B0CBD820D12A0F2B336F7C39AD"));
            bus.Add(new Buses("bus 15", "http://track.maliatec.com/locator/index.html?t=dd2a7384f9be9c7480126bf4d9efd66fC87729D92802BD699303EB234C97807988FF6F64"));

            return BusesResponse.Create(1, "", bus);
        }

        [HttpPost]
        public StudentResponse GetChildrenInfo(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return StudentResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return StudentResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), Convert.ToInt32(param.collegeId), param.studentId, 0, "Children");

                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(ti.Id_College), "ONLINE_URL");

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        List<Ent_EleveByUserIDResult> ele = frntComm.Get_EleveByUserID(Convert.ToInt32(param.collegeId), param.studentId, Annee);

                        if (ele.Count > 0)
                        {
                            Children child = new Children();

                            child.identity = ele[0].ID;
                            child.collegeIdentity = ele[0].Id_College.Value;
                            child.username = ele[0].Username;
                            child.displayName = ele[0].DisplayName;
                            child.firstName = ele[0].Prenom;
                            child.classe = ele[0].UserInfo;
                            child.classCode = ele[0].Classe;
                            child.studentIdentity = ele[0].Id_ISIS.Value;
                            child.parentIdentity = Convert.ToInt32(ele[0].Id_ParentUser);
                            child.image = ONLINE_URL + ele[0].ImageURL.Replace("~/", "");
                            child.sessionId = Convert.ToInt32(ti.ID_User);
                            child.agenda = ele[0].Agenda;
                            child.evaluation = ele[0].Evaluation;
                            child.remarks = ele[0].Remarques;
                            child.reportCard = ele[0].Carnet;
                            child.absences = ele[0].Absences;
                            child.skills = ele[0].Competence;
                            child.timeline = ele[0].Timeline;
                            child.forum = ele[0].Forum;
                            child.quiz = true;

                            return StudentResponse.Create(1, "", child);
                        }
                        else
                            return StudentResponse.Create(0, param.language == "EN" ? "Invalid student" : param.language == "FR" ? "Étudiant invalide" : "طالب غير صالح", new Children());
                    }
                }
            }
            catch (Exception exp)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return StudentResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetChildrenInfo", exp.Message);

                    return StudentResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public AgendaDayResponse GetStudentAgendaDay(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return AgendaDayResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return AgendaDayResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(param.collegeId), "ONLINE_URL");

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        TeacherComm teacherComm = new TeacherComm(ConnectionString);
                        comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, Convert.ToInt32(param.studentId), "Agenda");

                        AgendaCategory ac = null;
                        List<AgendaCategory> resultAgenda = new List<AgendaCategory>();

                        List<Mob_StudentAgendaDayResult> lstAgenda = frntComm.GetStudentAgendaDay(Convert.ToInt32(param.collegeId),
                            param.studentId, Annee, Convert.ToDateTime(param.dayDate), param.language);

                        int prevCatID = -1;

                        if (lstAgenda.Count > 0)
                        {
                            CultureInfo ci;

                            if (param.language == "AR")
                                ci = new CultureInfo("ar-LB");
                            else if (param.language == "FR")
                                ci = new CultureInfo("fr-FR");
                            else
                                ci = new CultureInfo("en-US");

                            foreach (Mob_StudentAgendaDayResult Agenda in lstAgenda)
                            {
                                if (prevCatID != Agenda.Cat)
                                {
                                    ac = new AgendaCategory();
                                    ac.Cat = Agenda.Cat;
                                    ac.CatName = Agenda.CatName;
                                    ac.LstAgendaDay = new List<AgendaDay>();
                                }
                                AgendaDay agday = new AgendaDay();

                                agday.RowNum = Convert.ToInt64(Agenda.RowNum);

                                agday.DisplayName = Agenda.DisplayName;
                                agday.ImageURL = ONLINE_URL + Agenda.ImageURL.Replace("~", "");
                                agday.ID = Agenda.ID;
                                agday.Id_User = Convert.ToInt32(Agenda.Id_User);
                                agday.Homework = Agenda.Devoir;
                                agday.DayDate = Agenda.DateJour.Value.ToString("MMM dd", ci);
                                agday.DateAdded = Agenda.DateAdded.Value.ToString("MMM dd", ci);
                                string estimTime = "";
                                if (Agenda.NbHours != "00")
                                    estimTime = Agenda.NbHours + "h";
                                if (Agenda.NbMins != "00")
                                    estimTime = estimTime != "" ? ":" : "" + Agenda.NbMins + "mn";
                                agday.EstimTime = estimTime;

                                agday.NbHours = Agenda.NbHours;
                                agday.NbMins = Agenda.NbMins;
                                agday.ClasseSec = Agenda.ClasseSec;
                                agday.Subject = Agenda.matiere;
                                agday.Edited = Agenda.StatusID == 2 ? "1" : "0";
                                agday.ProfileURL = Agenda.ProfileURL;
                                agday.StatusColor = Agenda.StatusColor;
                                agday.CategoryColor = Agenda.CategoryColor;
                                agday.CategoryDesc = Agenda.CategoryDesc;
                                agday.PublishingDate = Agenda.PublishingDate.Value.ToString("MMM dd", ci);
                                agday.ReminderImage = ONLINE_URL + Agenda.ReminderImage.Replace("~", "");
                                agday.AllowEdit = 0;
                                agday.AllowApprove = 0;
                                agday.ContainsArabic = IsArabic(Agenda.Devoir);

                                List<HomeworkFile> lstFiles = new List<HomeworkFile>();
                                foreach (EntSP_FilesByAgendaIDResult elem in teacherComm.GetEnt_FilesByAgendaID(Agenda.ID, Agenda.Id_College.Value))
                                {
                                    string fileName = elem.FileName;
                                    if (fileName.Length > 30)
                                        fileName = elem.FileName.Substring(0, 30) + "...";

                                    lstFiles.Add(new HomeworkFile(elem.ID.ToString(),
                                        ONLINE_URL + "images/" + elem.IconURL.Replace(".png", "_64x64.png"),
                                        fileName, ONLINE_URL + elem.FolderUploaded));
                                }

                                agday.agendaFiles = lstFiles;

                                ac.LstAgendaDay.Add(agday);
                                if (prevCatID != Agenda.Cat)
                                    resultAgenda.Add(ac);
                                prevCatID = Agenda.Cat;
                            }

                            return AgendaDayResponse.Create(1, "", resultAgenda);
                        }
                        else
                        {
                            return AgendaDayResponse.Create(0, param.language == "EN" ? "No agenda is added" : param.language == "FR" ? "Aucune agenda n'est ajoutée" : "لا يتم إضافة أية أجندة", new List<AgendaCategory>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return AgendaDayResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetStudentAgendaDay", e.Message);

                    return AgendaDayResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public ColumnNumberResponse GetEvaluationColumn(TokenParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return ColumnNumberResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return ColumnNumberResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        TeacherComm frntComm = new TeacherComm(ConnectionString);
                        AdminComm admCom = new AdminComm(ConnectionString);
                        int Annee = admCom.GetCurrentYear(Convert.ToInt32(param.collegeId));

                        List<Ent_LibelleNumColonneResult> lstNumColonne = frntComm.GetEnt_LibelleNumColonne(Convert.ToInt32(param.collegeId),
                            0, "", Annee, null, 0, param.studentId, false, false, true, false);
                        List<ColumnNumber> AddNumColonne = new List<ColumnNumber>();

                        ColumnNumber def = new ColumnNumber();
                        def.ColumnNum = 0;
                        def.ColumnName = (param.language == "FR" ? "Colonne" : param.language == "EN" ? "Column" : "خانة");
                        AddNumColonne.Add(def);

                        if (lstNumColonne.Count > 0)
                        {
                            foreach (Ent_LibelleNumColonneResult NumCol in lstNumColonne)
                            {
                                ColumnNumber col = new ColumnNumber();
                                col.ColumnNum = Convert.ToInt32(NumCol.Numcolonne);
                                col.ColumnName = NumCol.libelle;

                                AddNumColonne.Add(col);
                            }
                            return ColumnNumberResponse.Create(1, "", AddNumColonne);
                        }
                        else
                        {
                            return ColumnNumberResponse.Create(0, "", new List<ColumnNumber>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return ColumnNumberResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetEvaluationColumn", e.Message);

                    return ColumnNumberResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public SubjectsResponse GetEvaluationSubjects(TokenParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return SubjectsResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return SubjectsResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        AdminComm admCom = new AdminComm(ConnectionString);
                        int Annee = admCom.GetCurrentYear(Convert.ToInt32(param.collegeId));

                        List<Mob_StudentSubjectsResult> lstSubjects = frntComm.GetStudentSubjects(param.studentId, Convert.ToInt32(param.collegeId), Annee);
                        List<Subjects> subjects = new List<Subjects>();

                        Subjects def = new Subjects();
                        def.SubjectId = 0;
                        def.SubjectName = (param.language == "FR" ? "Matière" : param.language == "EN" ? "Subject" : "المادة");
                        subjects.Add(def);

                        if (lstSubjects.Count > 0)
                        {
                            foreach (Mob_StudentSubjectsResult sub in lstSubjects)
                            {
                                Subjects s = new Subjects();
                                s.SubjectId = sub.Id_Matiere.Value;
                                s.SubjectName = sub.Matiere;

                                subjects.Add(s);
                            }
                            return SubjectsResponse.Create(1, "", subjects);
                        }
                        return SubjectsResponse.Create(0, "", new List<Subjects>());
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return SubjectsResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetEvaluationSubjects", e.Message);

                    return SubjectsResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public EvaluationResponse GetEvaluationDetails(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return EvaluationResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return EvaluationResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, Convert.ToInt32(param.studentId), "EvalDetails");

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        List<Mob_StudentEvaluationColumnResult> lstEvaluation = frntComm.GetStudentEvaluationColumn(Convert.ToInt32(param.studentId),
                            ti.Id_College, Annee, Convert.ToInt32(param.columnNum), Convert.ToInt32(param.subjectId), 0, 1000);

                        if (lstEvaluation.Count > 0)
                        {
                            CultureInfo ci;

                            if (param.language == "AR")
                                ci = new CultureInfo("ar-LB");
                            else if (param.language == "FR")
                                ci = new CultureInfo("fr-FR");
                            else
                                ci = new CultureInfo("en-US");

                            List<Evaluation> AddEvaluation = new List<Evaluation>();

                            foreach (Mob_StudentEvaluationColumnResult Evaluation in lstEvaluation)
                            {
                                Evaluation EvalCol = new Evaluation();

                                EvalCol.RecNo = Convert.ToInt64(Evaluation.RecNo);
                                EvalCol.GradeOver = String.Format(Evaluation.NoteSur % 1 == 0 ? "{0:###,##0}" : "{0:###,##0.00}", Evaluation.NoteSur.Value);
                                EvalCol.Weight = Evaluation.Show_Evaluation_Weight == "TRUE" && Evaluation.Coeff.Value != 0 && Evaluation.DisplayType == ""
                                    ? (param.language == "EN" ? "weight: " : param.language == "FR" ? "poid: " : " : الوزن ") + String.Format("{0:###,##0.00}", Evaluation.Coeff.Value)
                                    : "";
                                EvalCol.ColumnNumber = Convert.ToInt32(Evaluation.NumColonne);
                                EvalCol.ColumnName = Evaluation.LibelleSousColonne;
                                EvalCol.Remark = Evaluation.ShowEvaluationRemark == "TRUE" && Evaluation.Remarques != "" ? "(" + Evaluation.Remarques + ")" : "";
                                EvalCol.EvaluationDate = Evaluation.DateEval.Value.ToString("MMM dd", ci);
                                EvalCol.EntryDate = Evaluation.DateSaisie.Value.ToString("MMM dd", ci);
                                EvalCol.ClassAvg = Convert.ToDecimal(Evaluation.MoyenneClasse);
                                EvalCol.EvaluationType = Evaluation.EvaluationType;
                                EvalCol.ClassName = Evaluation.Classe;
                                EvalCol.Section = Evaluation.Section;
                                EvalCol.Subject = Evaluation.Show_Arabic_Subjects == "FALSE" ? Evaluation.Matiere : Evaluation.MatiereAr;
                                if (Evaluation.Note == -5)
                                    EvalCol.Grade = param.language == "EN" ? "Abs" : param.language == "FR" ? "Abs" : "غياب";
                                else
                                    EvalCol.Grade = Evaluation.DisplayType != "" ? Evaluation.Lettre :
                                        String.Format("{0:###,##0.00}", Evaluation.Note.Value) + "/" + EvalCol.GradeOver;

                                AddEvaluation.Add(EvalCol);
                            }
                            return EvaluationResponse.Create(1, "", AddEvaluation);
                        }
                        else
                        {
                            return EvaluationResponse.Create(0, "", new List<Evaluation>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return EvaluationResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetEvaluationDetails", e.Message);

                    return EvaluationResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public RemarkResponse GetRemark(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return RemarkResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return RemarkResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(param.collegeId), "ONLINE_URL");

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, Convert.ToInt32(param.studentId), "Remark");

                        List<Remark> AddProfRemarque = new List<Remark>();
                        List<Mob_TeacherRemarksResult> lstProfRmq = frntComm.GetTeacherRemarks(Convert.ToInt32(param.collegeId),
                            ti.ID_User.Value, Convert.ToInt32(param.studentId), Annee);

                        if (lstProfRmq.Count > 0)
                        {
                            CultureInfo ci;

                            if (param.language == "AR")
                                ci = new CultureInfo("ar-LB");
                            else if (param.language == "FR")
                                ci = new CultureInfo("fr-FR");
                            else
                                ci = new CultureInfo("en-US");

                            foreach (Mob_TeacherRemarksResult rem in lstProfRmq)
                            {
                                Remark Rq = new Remark();
                                Rq.ID = rem.ID;
                                Rq.Id_College = Convert.ToInt32(rem.Id_College);
                                Rq.Id_User = Convert.ToInt32(rem.Id_User);
                                Rq.RemarkType = rem.Type_Remarque;
                                Rq.ColumnNumber = Convert.ToInt32(rem.NumColonne);
                                Rq.RemarkText = rem.Remarque;
                                Rq.RemarkDate = rem.Remarque_Date.Value.ToString("MMM dd", ci);
                                Rq.Status = Convert.ToBoolean(rem.Status);
                                Rq.AddedDate = rem.AddedDate.Value.ToString("MMM dd", ci);
                                Rq.Subject = rem.Show_Arabic_Subjects == "TRUE" ? rem.NomArabe : rem.Nom;
                                Rq.Details = rem.Details.ToString();
                                Rq.ColumnName = rem.Libelle.ToString();
                                Rq.DisplayName = rem.Location == "LOCAL" || rem.DisplayName == "" ? (param.language == "AR" ? "الإدارة" : "Administration") : rem.DisplayName;
                                Rq.ImageURL = rem.Location == "LOCAL" || rem.ImageURL == "" ? ONLINE_URL + "/images/guestavatar.gif" : ONLINE_URL + rem.ImageURL.Replace("~", "");
                                Rq.Color = rem.Color;
                                Rq.ContainsArabic = IsArabic(rem.Remarque);

                                AddProfRemarque.Add(Rq);
                            }

                            return RemarkResponse.Create(1, "", AddProfRemarque);
                        }
                        else
                        {
                            return RemarkResponse.Create(0, param.language == "EN" ? "No remarks is added" : param.language == "FR" ? "Aucune remarque n'est ajoutée" : "لا يوجد أي ملاحظة", new List<Remark>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return RemarkResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetRemark", e.Message);

                    return RemarkResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public ReportCardResponse GetStudentReportCard(TokenParam param)
        {
            AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
            List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

            if (c.Count == 0)
                return ReportCardResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

            else
            {
                string ConnectionString = c[0].ConnectionString;
                AdminComm comm = new AdminComm(ConnectionString);

                Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                if (ti == null)
                    return ReportCardResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                else
                {
                    FrontComm frntComm = new FrontComm(ConnectionString);

                    string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(ti.Id_College), "ONLINE_URL");

                    comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, Convert.ToInt32(param.studentId), "ReportCard");

                    List<EntSP_GetStudentReportCardResult> studentRC = frntComm.GetStudentReportCard(
                        Convert.ToInt32(param.collegeId), param.schoolYear, param.studentId, "any");

                    if (studentRC.Count == 0)
                        return ReportCardResponse.Create(0, param.language == "EN" ? "File not found" : param.language == "FR" ? "fichier non trouvé" : "الملف غير موجود", new List<ReportCard>());
                    else
                    {
                        List<ReportCard> lst = new List<ReportCard>();

                        foreach (EntSP_GetStudentReportCardResult report in studentRC)
                        {
                            string ReportCard = report.ReportCardPath;
                            string reportCardFileURL = ReportCard.ToLower().Replace("/uploads", ONLINE_URL + "/uploads");

                            string reportCardTitle = "";

                            if (report.ReportCardType == "reportcard")
                                reportCardTitle = (param.language == "EN" ? "Report card (" : param.language == "FR" ? "Carnet (" : " بطاقة العلامات (")
                                    + (report.Annee - 1).ToString() + "-" + report.Annee.ToString() + ")";
                            else
                                reportCardTitle = (param.language == "EN" ? "Skills (" : param.language == "FR" ? "Compétences (" : " المهارات (")
                                    + (report.Annee - 1).ToString() + "-" + report.Annee.ToString() + ")";

                            try
                            {
                                WebRequest req = WebRequest.Create(reportCardFileURL);
                                WebResponse res = req.GetResponse();

                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reportCardFileURL);

                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                {
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        ReportCard obj = new ReportCard(reportCardFileURL, reportCardTitle, ONLINE_URL + "images/fileTypeIcon/pdf.png", report.Annee.ToString(), report.ReportCardType == "reportcard" ? "NOT" : "CMP");
                                        lst.Add(obj);
                                    }
                                    else
                                    {
                                        ReportCard obj = new ReportCard("", reportCardTitle, ONLINE_URL + "images/fileTypeIcon/pdf.png", report.Annee.ToString(), report.ReportCardType == "reportcard" ? "NOT" : "CMP");
                                        lst.Add(obj);
                                    }
                                }
                            }
                            catch (WebException ex)
                            {
                                ReportCard obj = new ReportCard("", reportCardTitle, ONLINE_URL + "images/fileTypeIcon/pdf.png", report.Annee.ToString(), report.ReportCardType == "reportcard" ? "NOT" : "CMP");
                                lst.Add(obj);
                            }
                        }

                        return ReportCardResponse.Create(1, "", lst);
                    }
                }
            }
        }

        [HttpPost]
        public ExecResult ReadReportCard(TokenParam param)
        {
            AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
            List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

            if (c.Count == 0)
                return new ExecResult(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة");

            else
            {
                string ConnectionString = c[0].ConnectionString;
                AdminComm comm = new AdminComm(ConnectionString);

                Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                if (ti == null)
                    return new ExecResult(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة");
                else
                {
                    FrontComm frntComm = new FrontComm(ConnectionString);

                    comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, Convert.ToInt32(param.studentId), "ReportCard");

                    bool result = frntComm.ReadReportCard(param.studentId, Convert.ToInt32(param.collegeId), param.schoolYear, param.uType);

                    if (result) return new ExecResult(100, "Done");

                    return new ExecResult(-100, "Error occured");

                }
            }
        }


        [HttpPost]
        public RfrObjectResponse GetAgendaNextDay(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        List<RfrObject> lst = new List<RfrObject>();

                        TeacherComm teacher = new TeacherComm(ConnectionString);
                        DateTime date = teacher.Usr_WeekDay(Convert.ToInt32(param.collegeId), Convert.ToDateTime(param.dayDate));
                        RfrObject obj = new RfrObject(date.ToString("yyyy-MM-dd"), date.ToString("yyyy-MM-dd"), "0");
                        lst.Add(obj);

                        return RfrObjectResponse.Create(1, "", lst);
                    }
                }
            }
            catch (Exception ex)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetAgendaNextDay", ex.Message);

                    return RfrObjectResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public RfrObjectResponse GetStudentSchoolYears(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        List<RfrObject> lst = new List<RfrObject>();
                        List<Ent_InfoClasseEleveResult> info = frntComm.GetInfoClasseEleve(Convert.ToInt32(param.studentId), Convert.ToInt32(param.collegeId), c[0].MobileAppLang);
                        if (info.Count == 0)
                        {
                            return RfrObjectResponse.Create(0, "", new List<RfrObject>());
                        }
                        foreach (Ent_InfoClasseEleveResult cl in info)
                        {
                            RfrObject obj = new RfrObject(cl.Annee.ToString(), cl.Annee_Scolaire, (cl.Annee == Annee ? "1" : "0"));
                            lst.Add(obj);
                        }

                        return RfrObjectResponse.Create(1, "", lst);
                    }
                }
            }
            catch (Exception ex)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetStudentSchoolYears", ex.Message);

                    return RfrObjectResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public StoryResponse GetStoriesStudent(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return StoryResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return StoryResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(ti.Id_College), "ONLINE_URL");

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        comm.InsertMobActivity(Convert.ToInt32(ti.ID_User), ti.Id_College, ti.ID, ti.Id_ISIS.Value, "SStories");

                        List<Story> lstStory = new List<Story>();
                        List<Mob_StoryBoardStudentResult> lstProfRmq = frntComm.GetStoryBoardStudent(Convert.ToInt32(param.collegeId),
                            ti.ID_User.Value, ti.Id_ISIS.Value, Annee, Convert.ToInt32(param.PageSize), Convert.ToInt32(param.PageNumber));

                        if (lstProfRmq.Count > 0)
                        {
                            string StoryRem = (param.language == "FR" ? "Remarque" : param.language == "EN" ? "Remark" : "ملاحظات");
                            string StoryNews = (param.language == "FR" ? "Nouvelle" : param.language == "EN" ? "News" : "أخبار");
                            string StoryEval = (param.language == "FR" ? "Evaluation" : param.language == "EN" ? "Evaluation" : "تقييم");
                            string StoryAgenda = (param.language == "FR" ? "Agenda" : param.language == "EN" ? "Agenda" : "فرض");
                            string StoryForum = (param.language == "FR" ? "Forum" : param.language == "EN" ? "Forum" : "");
                            string StoryAbs = (param.language == "FR" ? "Absence" : param.language == "EN" ? "Absence" : "غياب");

                            CultureInfo ci;

                            if (param.language == "AR")
                                ci = new CultureInfo("ar-LB");
                            else if (param.language == "FR")
                                ci = new CultureInfo("fr-FR");
                            else
                                ci = new CultureInfo("en-US");

                            foreach (Mob_StoryBoardStudentResult dbstory in lstProfRmq)
                            {
                                Story st = new Story();
                                st.ID = dbstory.ID.Value;
                                st.StoryType = dbstory.StoryType == "NEWS" && dbstory.Id_MailType == 5 ? "SURVEY" : dbstory.StoryType;
                                st.StoryTitle = dbstory.StoryType == "REM" ? StoryRem :
                                                dbstory.StoryType == "NEWS" ? StoryNews :
                                                dbstory.StoryType == "EVAL" ? StoryEval :
                                                dbstory.StoryType == "AGENDA" ? StoryAgenda :
                                                dbstory.StoryType == "FORUM" ? StoryForum :
                                                dbstory.StoryType == "ABS" ? StoryAbs : "";

                                //Used only for news to show the uploaded files
                                if (dbstory.StoryText.Contains("/RadControls/Editor/Files") || dbstory.StoryText.Contains("/Uploads/"))
                                {
                                    st.StoryText = dbstory.StoryText.Replace("href=\"/", ONLINE_URL + "href=\"" + ONLINE_URL + "/");
                                    st.StoryText = dbstory.StoryText.Replace("src=\"/", ONLINE_URL + "src=\"" + ONLINE_URL + "/");
                                    //st.StoryText = st.StoryText.Replace("<a", "<a target='_blank'");
                                }
                                else
                                    st.StoryText = dbstory.StoryText;

                                st.StoryDate = dbstory.StoryDate.Value.ToString("MMM dd", ci);
                                st.StoryFullDate = dbstory.StoryDate.Value.ToString("yyyy-MM-dd");
                                // the story is a circulaire remove the sender name
                                st.DisplayName = dbstory.Id_MailType == 2 ? "" : dbstory.DisplayName;
                                // the story is a circulaire show the school cover iamge
                                st.ImageURL = dbstory.Id_MailType == 2 ? ONLINE_URL + dbstory.NewsCover.Replace("~", "") : ONLINE_URL + dbstory.ImageURL.Replace("~", "");
                                st.Subject = dbstory.StorySubject;
                                st.StudentIdentity = dbstory.Id_Student.Value;
                                st.isRead = dbstory.isRead.Value ? 1 : 0;
                                st.IconCss = (dbstory.StoryType == "REM" ? "alert-circle-outline" :
                                             (dbstory.StoryType == "EVAL" ? "document-outline" :
                                             (dbstory.StoryType == "AGENDA" ? "calendar-outline" :
                                             (dbstory.StoryType == "ABS" ? "remove-circle-outline" :
                                             (dbstory.StoryType == "HOMEWORK" ? "library_books" : "")))));
                                st.Id_MailType = dbstory.Id_MailType;
                                st.pinned = dbstory.Pinned != null && dbstory.Pinned != false ? "1" : "0";
                                lstStory.Add(st);
                            }

                            return StoryResponse.Create(1, "", lstStory);
                        }
                        else
                        {
                            return StoryResponse.Create(0, "", new List<Story>());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return StoryResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetStoriesStudent", e.Message);

                    return StoryResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public ChildrenCoursesResponse GetChildrenCourses(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return ChildrenCoursesResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);

                    if (ti == null)
                        return ChildrenCoursesResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        List<MainCourses> mainCourses = new List<MainCourses>();

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        List<Ent_UserCoursesResult> lst = frntComm.GetUserCourses(Convert.ToInt32(ti.Id_College), param.UserId, param.studentId, param.uType, Annee);

                        if (lst.Count > 0)
                        {
                            foreach (Ent_UserCoursesResult ele in lst)
                            {
                                if (ele.ParentID == null)
                                {
                                    MainCourses course = new MainCourses();
                                    course.identity = ele.ID.Value;
                                    course.parentId = ele.ParentID;
                                    course.childId = ele.ChildID;
                                    course.subjectName = ele.MobileDesc;
                                    course.cntItems = ele.cntResources;
                                    course.lstSubCourses = new List<SubCourses>();

                                    mainCourses.Add(course);
                                }
                                else
                                {
                                    List<MainCourses> parentCourse = mainCourses.Where(obj => obj.childId == ele.ParentID).ToList();
                                    if (parentCourse.Count > 0)
                                    {
                                        SubCourses sub = new SubCourses();
                                        sub.identity = ele.ID.Value;
                                        sub.parentId = ele.ParentID;
                                        sub.childId = ele.ChildID;
                                        sub.subjectName = ele.MobileDesc;
                                        sub.cntItems = ele.cntResources;

                                        parentCourse[0].lstSubCourses.Add(sub);
                                    }
                                }
                            }
                            return ChildrenCoursesResponse.Create(1, "", mainCourses);
                        }
                        else
                            return ChildrenCoursesResponse.Create(0, param.language == "EN" ? "No courses found" : param.language == "FR" ? "Aucune matière trouvée" : "لم يتم العثور على أي مادة", null);
                    }
                }
            }
            catch (Exception exp)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return ChildrenCoursesResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetChildrenCourses", exp.Message);

                    return ChildrenCoursesResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public CourseSectionsResponse GetCourseSections(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return CourseSectionsResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);

                    if (ti == null)
                        return CourseSectionsResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        List<CourseSections> lst = new List<CourseSections>();

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        List<EntSP_CourseSectionsResult> lstSections = frntComm.GetCourseSections(Convert.ToInt32(ti.Id_College),
                            Convert.ToInt32(param.subjectId), param.UserId, param.studentId, param.uType, Annee, 0, 0);

                        foreach (EntSP_CourseSectionsResult section in lstSections)
                        {
                            CourseSections sec = new CourseSections();
                            sec.identity = section.ID;
                            sec.courseName = section.ClasseMatiere;
                            sec.sectionName = section.SectionName;
                            sec.cntNotDone = section.cntNotDone;
                            sec.accomplishedPerc = section.AccomplishedPerc;

                            lst.Add(sec);
                        }

                        return CourseSectionsResponse.Create(1, "", lst);
                    }
                }
            }
            catch (Exception exp)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return CourseSectionsResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetChildrenCourses", exp.Message);

                    return CourseSectionsResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public CourseResourcesResponse GetCourseResources(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return CourseResourcesResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);

                    if (ti == null)
                        return CourseResourcesResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(param.collegeId), "ONLINE_URL");
                        List<CourseResources> courseResourcesList = new List<CourseResources>();

                        FrontComm frntComm = new FrontComm(ConnectionString);
                        TeacherComm teacherComm = new TeacherComm(ConnectionString);

                        List<Mob_TimelineByUserResult> lst = frntComm.GetTimelineByUser(Convert.ToInt32(param.collegeId),
                            ti.ID_User.Value, param.uType, param.subjectId, param.sectionId, param.studentId, Annee);

                        foreach (Mob_TimelineByUserResult resource in lst)
                        {
                            CourseResources res = new CourseResources();
                            res.timeline_id = resource.RecNo.ToString();
                            res.IsToday = resource.IsToday;
                            res.Color = resource.Color;
                            res.IconURL = resource.IconURL;
                            res.AllowEdit = resource.AllowEdit;
                            res.Id_ForeignKey = resource.Id_ForeignKey;
                            res.Id_Section = resource.Id_Section;
                            res.HasSkills = resource.HasSkills > 0 ? true : false;
                            res.Id_MailType = resource.Id_MailType;
                            res.guidid = Guid.NewGuid().ToString();
                            res.classematiere = resource.Id_Course;
                            res.utype = param.uType;
                            res.StorySubject = resource.StorySubject;
                            res.AllowCopyToOtherCourseSection = false; //resource.Id_MailType == 16 || resource.Id_MailType == 17 || resource.Id_MailType == 18 ? true : false;
                            res.AllowCloneToOtherCourse = false; //resource.Id_MailType == 6 || resource.Id_MailType == 7 || resource.Id_MailType == 16 || resource.Id_MailType == 18 ? true : false;
                            res.ExtraDetails = resource.ExtraDetails == "" || resource.StoryReadOnly ? "" : resource.ExtraDetails + (param.language.ToUpper() == "FR" ? " réponses " : param.language.ToUpper() == "EN" ? " replies " : " رد ");
                            res.StoryBody = resource.StoryBody == null ? "" : resource.StoryBody.Replace("src=\"/Uploads/", "src=\"" + ONLINE_URL + "/Uploads/");
                            res.PublishDate = resource.PublishDate.Value.ToString("dd/MM/yyyy");
                            res.till_text = resource.DueDate != null && resource.Id_MailType == 6 ? (param.language == "FR" ? " jusqu'à " : param.language == "EN" ? " Till " : " حتى ") : "";
                            res.DueDate = resource.DueDate != null && resource.Id_MailType == 6 ? resource.DueDate.Value.ToString("dd/MM/yyyy") : "";
                            res.Checked = resource.IsAccomplished;
                            //res.disabled = res.utype = param.uType == "student" || resource.SYear != resource.currentSyear ? true : false;
                            // on change
                            res.IsAccomplished = resource.IsAccomplished;
                            res.AllowMarkAsDoneByStudent = resource.AllowMarkAsDoneByStudent && resource.StoryReadOnly ? true : false;
                            //res.AllowCloneFromPreviousYear = SYear != CurrentSYear ? "block" : "none"
                            res.IsDone = resource.IsDone;
                            res.DoneStatus = !resource.IsDone;
                            res.id_student = resource.Id_UserStudent;
                            res.card_class = !resource.AllowMarkAsDoneByStudent ? "card" : resource.IsDone ? "card card-stats-success" : "card card-stats-warning";
                            //res.LinkAccess = SYear != CurrentSYear ? "none" : "all"
                            //res.URL
                            res.id_student = resource.Id_Student;
                            res.ColumnLabel = resource.ColumnLabel;
                            res.TypeDesc = resource.TypeDesc;
                            res.for_text = param.language == "FR" ? " pour " : param.language == "EN" ? " for " : " ل ";
                            res.Id_Classe = resource.Id_Class;
                            res.NumColumn = resource.NumColumn;
                            res.StorySubject = resource.StorySubject;
                            res.ExtraDetails = resource.ExtraDetails == "" ? "" : "<br/>" + resource.ExtraDetails;

                            List<ResourceFiles> TimelineFilesList = new List<ResourceFiles>();
                            //         Homeworks        ||         Forums          ||          Pages           ||     Video Conferences    ||   Videos (Youtube, ...)
                            if (resource.Id_MailType == 6 || resource.Id_MailType == 7 || resource.Id_MailType == 16 || resource.Id_MailType == 17 || resource.Id_MailType == 18)
                            {
                                foreach (EntSP_MailFilesResult file in frntComm.GetMailFiles(resource.Id_ForeignKey, 0, Convert.ToInt32(param.collegeId), "main"))
                                {
                                    ResourceFiles timelineFile = new ResourceFiles();
                                    timelineFile.fileIdentity = file.ID;
                                    timelineFile.fileCategory = file.FileCategory;
                                    timelineFile.fileIconURL = ONLINE_URL + "images/" + file.FileIconURL.Replace(".png", "_64x64.png");
                                    timelineFile.fileName = file.FileName;
                                    timelineFile.filePath = ONLINE_URL + file.FilePath;
                                    timelineFile.fileType = file.FileType;

                                    string mime;

                                    if (timelineFile.fileType == "pdf")
                                        mime = "application/pdf";
                                    else if(timelineFile.fileType == "jpeg" || timelineFile.fileType == "jpg")
                                        mime = "image/jpeg";
                                    else if (timelineFile.fileType == "doc")
                                        mime = "application/msword";
                                    else if (timelineFile.fileType == "docx")
                                        mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                    else if (timelineFile.fileType == "gif")
                                        mime = "image/gif";
                                    else if (timelineFile.fileType == "png")
                                        mime = "image/png";
                                    else
                                        mime = "";

                                    timelineFile.fileMIMEType = mime;

                                    TimelineFilesList.Add(timelineFile);
                                }
                            }
                            // Agenda
                            else if (resource.Id_MailType == 15)
                            {
                                foreach (EntSP_FilesByAgendaIDResult file in teacherComm.GetEnt_FilesByAgendaID(resource.Id_ForeignKey, Convert.ToInt32(param.collegeId)))
                                {
                                    ResourceFiles timelineFile = new ResourceFiles();
                                    timelineFile.fileIdentity = file.ID;
                                    timelineFile.fileCategory = file.FileCategory;
                                    timelineFile.fileIconURL = ONLINE_URL + "images/" + file.IconURL.Replace(".png", "_64x64.png");
                                    timelineFile.fileName = file.FileName;
                                    timelineFile.filePath = ONLINE_URL + file.FolderUploaded;
                                    timelineFile.fileType = file.FileType;

                                    string mime;

                                    if (file.FileType == "pdf")
                                        mime = "application/pdf";
                                    else if (file.FileType == "jpeg" || timelineFile.fileType == "jpg")
                                        mime = "image/jpeg";
                                    else if (file.FileType == "doc")
                                        mime = "application/msword";
                                    else if (file.FileType == "docx")
                                        mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                    else if (file.FileType == "gif")
                                        mime = "image/gif";
                                    else if (file.FileType == "png")
                                        mime = "image/png";
                                    else
                                        mime = "";

                                    timelineFile.fileMIMEType = mime;

                                    TimelineFilesList.Add(timelineFile);
                                }
                            }

                            if (resource.AllowMarkAsDoneByStudent && !resource.IsDone && resource.StoryReadOnly)
                                frntComm.MarkAsRead(true, DateTime.Now, resource.Id_ForeignKey, resource.Id_UserStudent, Convert.ToInt32(param.collegeId));

                            res.ResourceFiles = TimelineFilesList;
                            courseResourcesList.Add(res);
                        }

                        return CourseResourcesResponse.Create(1, "", courseResourcesList);
                    }
                }
            }
            catch (Exception ex)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return CourseResourcesResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetCourseResources", ex.Message);

                    return CourseResourcesResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }


        [HttpPost]
        public RfrObjectResponse ChangeResourceReadStatus(TokenParam param)
        {
            try
            {
                AdminComm admCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = admCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    int Annee = comm.GetCurrentYear(Convert.ToInt32(param.collegeId));
                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);

                    if (ti == null)
                        return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        FrontComm frntComm = new FrontComm(ConnectionString);

                        List<RfrObject> lst = new List<RfrObject>();

                        //ExecResult res = frntComm.UpdateEnt_CourseResourceReadStatus(Convert.ToInt32(param.collegeId),
                        //    param.courseResourceId, param.studentId, ti.ID_User.Value, param.isDone == 1 ? true : false);
                        bool isDone = false;

                        if (param.isDone == 1)
                            isDone = true;

                        ExecResult res = frntComm.MarkAsDone(isDone, param.courseResourceId, param.UserId, param.studentId, Convert.ToInt32(param.collegeId));

                        lst.Add(new RfrObject(res.RetValue.ToString(), res.Message, "0"));

                        return RfrObjectResponse.Create(1, "", lst);
                    }
                }
            }
            catch (Exception ex)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return RfrObjectResponse.Create(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetCourseResources", ex.Message);

                    return RfrObjectResponse.Create(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }

        [HttpPost]
        public MailPreviewResponse MailPreview(TokenParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new MailPreviewResponse(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return new MailPreviewResponse(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(param.collegeId), "ONLINE_URL");
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        TeacherComm teacherComm = new TeacherComm(ConnectionString);
                        AdminComm admCom = new AdminComm(ConnectionString);

                        List<EntSP_MailResult> list = frntComm.MailPreview(param.threadId, 0, param.UserId, Convert.ToInt32(param.collegeId), param.uType, param.language);

                        if (list.Count == 0)
                        {
                            return new MailPreviewResponse(0, param.language == "EN" ? "No mail is added" : param.language == "FR" ? "Aucun courrier n'est ajoutée" : "لا يوجد أي بريد", new List<MailPrev>());
                        }

                        if (param.uType == "main")
                            frntComm.SetNewsRead(Convert.ToInt32(param.UserId), Convert.ToInt32(ti.Id_College), Convert.ToInt32(param.threadId), true);

                        List<MailPrev> listToReturn = new List<MailPrev>();

                        bool AllowShowScore = false;
                        decimal Score = 0, totalScore = 0, achievedScore = 0;

                        foreach (EntSP_MailResult item in list)
                        {
                            MailPrev newItem = new MailPrev();
                            newItem.StartTag = item.StartTag;
                            newItem.EndTag = item.EndTag;
                            newItem.IdSeq = item.IdSeq;
                            newItem.DisplayName = item.DisplayName;
                            newItem.ImageURL = ONLINE_URL + item.ImageURL.Replace("~/", "");
                            newItem.TypeUser = item.TypeUser;
                            newItem.Id_Mail = item.Id_Mail;
                            newItem.Id_College = item.Id_College;
                            newItem.Id_MainMail = item.Id_MainMail;
                            newItem.Id_ReplyToMail = item.Id_ReplyToMail;
                            newItem.Id_MailType = item.Id_MailType;
                            newItem.CreatedBy = item.CreatedBy;
                            newItem.CreatedDate = item.CreatedDate;
                            newItem.PublishDate = item.PublishDate;
                            newItem.DueDate = item.DueDate;
                            newItem.MailSubject = item.MailSubject;
                            newItem.MailBody = item.MailBody == null ? "" : item.MailBody.Replace("src=\"/Uploads/", "src=\"" + ONLINE_URL + "/Uploads/");
                            newItem.MailBody_NoHtml = item.MailBody_NoHtml == null ? "" : item.MailBody_NoHtml;
                            newItem.IsReadOnly = item.IsReadOnly;
                            newItem.IsDraft = item.IsDraft;
                            newItem.IsDeleted = item.IsDeleted;
                            newItem.AllUsers = item.AllUsers;
                            newItem.ShowInCalendar = item.ShowInCalendar;
                            newItem.Pinned = item.Pinned;
                            newItem.Id_AgendaCategory = item.Id_AgendaCategory;
                            newItem.Id_AgendaStatus = item.Id_AgendaStatus;
                            newItem.AgendaNbHours = item.AgendaNbHours;
                            newItem.AgendaNbMinutes = item.AgendaNbMinutes;
                            newItem.AgendaApprovalDate = item.AgendaApprovalDate;
                            newItem.IsEdited = item.IsEdited;
                            newItem.DeletedBy = item.DeletedBy;
                            newItem.DeletedDate = item.DeletedDate;
                            newItem.IsAccomplished = item.IsAccomplished;
                            newItem.AccomplishedDate = item.AccomplishedDate;
                            newItem.Mail_Color = item.Mail_Color;
                            newItem.AllowReview = item.AllowReview;
                            newItem.AllowMarkAsAccomplished = item.AllowMarkAsAccomplished;
                            newItem.AllowMarkAsDoneByStudent = item.AllowMarkAsDoneByStudent;
                            newItem.AllowAddDates = item.AllowAddDates;
                            newItem.AllowPin = item.AllowPin;
                            newItem.AllowSendPushNotification = item.AllowSendPushNotification;
                            newItem.AllowReply = item.AllowReply;
                            newItem.AllowShowInCalendar = item.AllowShowInCalendar;
                            newItem.IsByStudent = item.IsByStudent;
                            newItem.ReplyAll = item.ReplyAll;
                            newItem.FilesHTML = item.FilesHTML;
                            newItem.IsRead = item.IsRead;
                            newItem.IsDone = item.IsDone;
                            newItem.AllowSection = item.AllowSection;
                            newItem.MainMail_CreatedDate = item.MainMail_CreatedDate;
                            newItem.AllowEdit = item.AllowEdit;

                            if (item.Id_MailType == 5) //Surveys
                            {
                                int cntGraded = 0;

                                foreach (EntSP_MailRepliesQuestionsResult quest in frntComm.GetMailRepliesQuestions(item.Id_College, item.Id_Mail, param.UserId, item.CreatedBy))
                                {
                                    if (quest.isGraded)
                                    {
                                        cntGraded += 1;
                                        totalScore += 100;
                                        achievedScore += quest.Grade == null ? 0 : quest.Grade.Value;
                                    }
                                }

                                AllowShowScore = cntGraded > 0;
                                if (totalScore != 0)
                                    Score = Math.Round(achievedScore * 100 / totalScore, 2);
                            }

                            newItem.AllowShowScore = AllowShowScore;
                            newItem.Score = Score;

                            List<ResourceFiles> TimelineFilesList = new List<ResourceFiles>();
                            //         Homeworks        ||         Forums          ||          Pages           ||     Video Conferences    ||   Videos (Youtube, ...)   ||  Surveys
                            //if (item.Id_MailType == 6 || item.Id_MailType == 7 || item.Id_MailType == 16 || item.Id_MailType == 17 || item.Id_MailType == 18 || item.Id_MailType == 5)
                            //{
                                foreach (EntSP_MailFilesResult file in frntComm.GetMailFiles(item.Id_Mail, 0, Convert.ToInt32(param.collegeId), "main"))
                                {
                                    ResourceFiles timelineFile = new ResourceFiles();
                                    timelineFile.fileIdentity = file.ID;
                                    timelineFile.fileCategory = file.FileCategory;
                                    timelineFile.fileIconURL = ONLINE_URL + "images/" + file.FileIconURL.Replace(".png", "_64x64.png");
                                    timelineFile.fileName = file.FileName;
                                    timelineFile.filePath = ONLINE_URL + file.FilePath;
                                    timelineFile.fileType = file.FileType;

                                    TimelineFilesList.Add(timelineFile);
                                }
                            //}
                            // Agenda
                            if (item.Id_MailType == 15)
                            {
                                foreach (EntSP_FilesByAgendaIDResult file in teacherComm.GetEnt_FilesByAgendaID(item.Id_Mail, Convert.ToInt32(param.collegeId)))
                                {
                                    ResourceFiles timelineFile = new ResourceFiles();
                                    timelineFile.fileIdentity = file.ID;
                                    timelineFile.fileCategory = file.FileCategory;
                                    timelineFile.fileIconURL = ONLINE_URL + "images/" + file.IconURL.Replace(".png", "_64x64.png");
                                    timelineFile.fileName = file.FileName;
                                    timelineFile.filePath = ONLINE_URL + file.FolderUploaded;
                                    timelineFile.fileType = file.FileType;

                                    TimelineFilesList.Add(timelineFile);
                                }
                            }
                            newItem.ResourceFiles = TimelineFilesList;




                            listToReturn.Add(newItem);
                        }

                        return new MailPreviewResponse(1, "", listToReturn);

                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new MailPreviewResponse(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "MailPreview", e.Message);

                    return new MailPreviewResponse(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }

        }

        [HttpPost]
        public ExecResult MailReplyToMail(TokenParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new ExecResult(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة");

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return new ExecResult(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة");
                    else
                    {
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        AdminComm admCom = new AdminComm(ConnectionString);
                        int Annee = admCom.GetCurrentYear(Convert.ToInt32(param.collegeId));

                        //subjectId for replyId

                        List<Ent_Mail> mainMail = frntComm.GetMailByID(Convert.ToInt32(param.collegeId), param.threadId);
                        List<Ent_MailType> mailType = frntComm.GetMailType(mainMail[0].Id_College, mainMail[0].Id_MailType);

                        int CreatedBy = (int)ti.ID_User;

                        if (mailType[0].IsByStudent)
                            CreatedBy = param.UserId;

                        EntSP_MailReplyToMailResult result = frntComm.MailReplyToMail(Convert.ToInt32(param.collegeId),
                            param.threadId, param.subjectId, CreatedBy, param.UserId, param.mailBody, param.mailBody_NoHtml);

                        if (result == null || result.AddedMailId == 0)
                        {
                            return new ExecResult(0, param.language == "EN" ? "No mail is added" : param.language == "FR" ? "Aucun courrier n'est ajoutée" : "لا يوجد أي بريد");
                        }

                        return new ExecResult(1, result.AddedMailId.ToString());

                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new ExecResult(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة");
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "MailReplyToMail", e.Message);

                    return new ExecResult(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل");
                }
            }

        }


        [HttpPost]
        public IHttpActionResult UploadFile()
        {
            int collegeId = Convert.ToInt32(HttpContext.Current.Request.Form["collegeId"]);
            int mail = Convert.ToInt32(HttpContext.Current.Request.Form["mail"]);
            int main_mail = Convert.ToInt32(HttpContext.Current.Request.Form["main_mail"]);

            AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
            List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(collegeId);
            string ConnectionString = c[0].ConnectionString;
            AdminComm comm = new AdminComm(ConnectionString);

            FrontComm frntComm = new FrontComm(ConnectionString);

            string root = comm.GetConfigByKey(Convert.ToInt32(collegeId), "BasePath");
            string targetFolder = root + "/Uploads/" + collegeId + "/mails/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/";
            string db_targetFolder = "/Uploads/" + collegeId + "/mails/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/";

            bool exists = Directory.Exists(targetFolder);

            if (!exists)
                Directory.CreateDirectory(targetFolder);

            HttpFileCollection files = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files : null;

            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFile file = files[i];

                //old
                //var fileName = Path.GetFileName(file.FileName);

                //var path = Path.Combine(
                //    HttpContext.Current.Server.MapPath("~/uploads"),
                //    fileName
                //);a

                //file.SaveAs(path);

                if (file != null && file.ContentLength > 0)
                {
                    string FileName = Regex.Replace(file.FileName, "[^\\w\\._]", "_", RegexOptions.Compiled);
                    string ext = Path.GetExtension(FileName);
                    if (FileName.Length > 100)
                        FileName = FileName.Substring(0, 100) + ext;

                    string fileCategory = "";
                    string fileIconURL = "file.png";
                    string fileType = ext.Replace(".", "");

                    Ent_FileTypeIcon fileTypeIcon = frntComm.GetFileTypeIcon(fileType);
                    if (fileTypeIcon != null)
                    {
                        fileIconURL = fileTypeIcon.IconURL;
                        fileCategory = fileTypeIcon.FileCategory;
                    }
                    else
                        fileCategory = "other";


                    // hash and size are calculated based on the original file not the compress one
                    int Id_College = collegeId;
                    string hash = "";
                    decimal size = file.ContentLength / 1024;
                    Stream stream;

                    using (MD5 md5 = MD5.Create())
                    {
                        stream = file.InputStream;
                        hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                    }

                    string DBFilePath = frntComm.GetFilePathByHashAndSize(Id_College, hash, size);
                    string FolderUploaded = "", FolderUploadedToCompress = "", db_FolderUploaded = "";

                    //the file is uploaded for the first time
                    if (DBFilePath == "")
                    {
                        Guid g = Guid.NewGuid();

                        if (fileCategory == "image")
                        {
                            if (fileType == "png" || fileType == "gif" || fileType == "jpg" || fileType == "jpeg")
                            {
                                FolderUploadedToCompress = targetFolder + g.ToString() + "_original-file" + ext;
                                FolderUploaded = targetFolder + g.ToString() + ext;
                                db_FolderUploaded = db_targetFolder + g.ToString() + ext;
                            }
                            else // we need to convert the file to jpg so we can scale it
                            {
                                FolderUploadedToCompress = targetFolder + g.ToString() + ext;
                                FolderUploaded = targetFolder + g.ToString() + ext;
                                db_FolderUploaded = db_targetFolder + g.ToString() + ext;
                            }
                        }
                        else
                        {
                            FolderUploadedToCompress = targetFolder + g.ToString() + ext;
                            FolderUploaded = targetFolder + g.ToString() + ext;
                            db_FolderUploaded = db_targetFolder + g.ToString() + ext;
                        }
                    }
                    else
                    {
                        FolderUploaded = DBFilePath;
                        FolderUploadedToCompress = DBFilePath;
                        db_FolderUploaded = DBFilePath;
                    }

                    ExecResult res = frntComm.InsertEnt_MailFiles(Id_College, mail, main_mail, FileName,
                    fileType, fileCategory, fileIconURL, db_FolderUploaded, hash, size);

                    if (DBFilePath == "")
                    {
                        if (res.RetValue != -1)
                        {
                            if (FolderUploaded == FolderUploadedToCompress)
                            {
                                file.InputStream.Position = 0;
                                file.SaveAs(FolderUploaded);
                            }
                            else
                            {
                                file.InputStream.Position = 0;
                                file.SaveAs(FolderUploadedToCompress);
                                ResizeImage.CompressImage(FolderUploadedToCompress, FolderUploaded);
                                File.Delete(FolderUploadedToCompress);
                            }
                        }
                        else
                        {
                            //return BadRequest("Cannot upload");
                        }
                    }
                }
            }

            return Ok();
        }

        [HttpPost]
        public MailRepliesQuestionsResponse GetMailRepliesQuestions(TokenParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new MailRepliesQuestionsResponse(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return new MailRepliesQuestionsResponse(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(param.collegeId), "ONLINE_URL");
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        TeacherComm teacherComm = new TeacherComm(ConnectionString);
                        AdminComm admCom = new AdminComm(ConnectionString);
                        int Annee = admCom.GetCurrentYear(Convert.ToInt32(param.collegeId));

                        List<EntSP_MailRepliesQuestionsResult> list = frntComm.GetMailRepliesQuestions(Convert.ToInt32(param.collegeId), param.threadId, param.UserId, param.creator_Id);

                        if (list.Count == 0)
                        {
                            return new MailRepliesQuestionsResponse(0, param.language == "EN" ? "No mail is added" : param.language == "FR" ? "Aucun courrier n'est ajoutée" : "لا يوجد أي بريد", new List<MailRepliesQuestions>());
                        }

                        List<MailRepliesQuestions> listToReturn = new List<MailRepliesQuestions>();
                        foreach (EntSP_MailRepliesQuestionsResult item in list)
                        {
                            MailRepliesQuestions newItem = new MailRepliesQuestions();
                            newItem.QuestionID = item.QuestionID.ToString();
                            newItem.Id_Mail = item.Id_Mail;
                            newItem.Id_College = item.Id_College;
                            newItem.QuestionTypeCode = item.QuestionTypeCode;
                            newItem.QuestionRank = item.QuestionRank;
                            newItem.QuestionContent = JsonConvert.DeserializeObject<SurveyQuestion>(item.QuestionContent
                                .Replace("<img", "<img class=\\\"custom-image\\\"")
                                .Replace("src=\\\"/Uploads/", "src=\\\"" + ONLINE_URL + "/uploads/"));
                            newItem.isGraded = item.isGraded;
                            newItem.isDone = item.isDone;
                            newItem.Bcc = item.Bcc;
                            newItem.AllowReview = item.AllowReview;
                            newItem.MailAnswerID = item.MailAnswerID.ToString();
                            newItem.IsAnswered = item.IsAnswered;
                            newItem.UserAnswer = item.UserAnswer == null ? "" : item.UserAnswer.Replace("<p>", "").Replace("</p>", "").Replace("<br/>", "\n");
                            newItem.LastAnswerDateTime = item.LastAnswerDateTime;
                            newItem.NbReviews = item.NbReviews;
                            newItem.Grade = Convert.ToDouble(item.Grade);
                            newItem.EditBy = item.EditBy;
                            newItem.ContentHTML = item.ContentHTML;
                            newItem.MaxQuestionRank = item.MaxQuestionRank;
                            listToReturn.Add(newItem);
                        }

                        return new MailRepliesQuestionsResponse(1, "", listToReturn);

                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new MailRepliesQuestionsResponse(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "GetMailRepliesQuestions", e.Message);

                    return new MailRepliesQuestionsResponse(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }

        }

        [HttpPost]
        public ExecResult UpdateSurveyAnswer(SurveyParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new ExecResult(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة");

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return new ExecResult(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة");
                    else
                    {
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        TeacherComm teacherComm = new TeacherComm(ConnectionString);
                        AdminComm admCom = new AdminComm(ConnectionString);

                        bool updateDone = frntComm.UpdateMailQuestionAnswers(Guid.Parse(param.questionId), Guid.Parse(param.mailAnswerId), param.userAnswer, param.editBy, int.Parse(param.collegeId));
                        if (!updateDone)
                        {
                            return new ExecResult(0, param.language == "EN" ? "No mail is added" : param.language == "FR" ? "Aucun courrier n'est ajoutée" : "لا يوجد أي بريد");
                        }

                        return new ExecResult(1, "");

                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new ExecResult(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة");
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);
                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "UpdateSurveyAnswer", e.Message);

                    return new ExecResult(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل");
                }
            }
        }

        [HttpPost]
        public DownloadTimelineResponse DownloadTimeline(TokenParam param)
        {
            try
            {
                AdminComm com = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = com.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new DownloadTimelineResponse(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);

                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    Mob_TokenInfoResult ti = comm.tokenInfo(param.token);
                    if (ti == null)
                        return new DownloadTimelineResponse(-100, param.language == "EN" ? "Invalid session" : param.language == "FR" ? "Session invalide" : "جلسة غير صالحة", null);
                    else
                    {
                        FrontComm frntComm = new FrontComm(ConnectionString);
                        TeacherComm teacherComm = new TeacherComm(ConnectionString);

                        List<ChildrenOffline> children = new List<ChildrenOffline>();

                        string ONLINE_URL = comm.GetConfigByKey(Convert.ToInt32(param.collegeId), "ONLINE_URL");

                        //int childId = -1, courseId = -1, sectionId = -1;
                        //ChildrenOffline child = null;
                        //MainCoursesOffline course = null;
                        //CourseSectionsOffline section = null;

                        //int RecNo = 0, cntItems = 0, cntNotDone = 0, cntResources = 0, cntAccomplishedResources = 0;

                        //foreach (Mob_TimelineOfflineResult itm in frntComm.GetTimelineOffline(Convert.ToInt32(param.collegeId), ti.ID_User.Value, param.schoolYear))
                        //{
                        //    studentId = itm.Id_Student;
                        //    courseId = itm.Id_Course;
                        //    sectionId = itm.Id_Section;
                        //    foreignKey = itm.Id_ForeignKey;

                        //    if (itm.Id_Student != childId)
                        //    {
                        //        child = new ChildrenOffline();

                        //        child.identity = itm.Id_UserStudent;
                        //        child.collegeIdentity = itm.Id_College;
                        //        child.username = itm.Student_username;
                        //        child.displayName = itm.Student_displayName;
                        //        child.firstName = itm.Student_firstName;
                        //        child.classe = itm.Student_classe;
                        //        child.classCode = itm.Student_classCode;
                        //        child.studentIdentity = itm.Id_Student;
                        //        child.parentIdentity = itm.Id_FamilyUser.Value;
                        //        child.sessionId = ti.ID;
                        //        child.image = ONLINE_URL + itm.Student_image.Replace("~/", "");
                        //        child.courses = new List<MainCoursesOffline>();

                        //        children.Add(child);

                        //        childId = itm.Id_Student;
                        //    }

                        //    if (itm.Id_Course != courseId)
                        //    {
                        //        course = new MainCoursesOffline();

                        //        course.identity = itm.Id_Course;
                        //        course.childId = itm.Id_Course.ToString();
                        //        course.parentId = itm.Id_CourseParent == null ? null : itm.Id_CourseParent.ToString();
                        //        course.subjectName = itm.CourseName;
                        //        course.cntItems = !itm.IsDone ? 1 : 0;
                        //        course.courseSections = new List<CourseSectionsOffline>();

                        //        child.courses.Add(course);

                        //        courseId = itm.Id_Course;
                        //        cntItems = !itm.IsDone ? 1 : 0;
                        //    }
                        //    else
                        //    {
                        //        cntItems += !itm.IsDone ? 1 : 0;
                        //        course.cntItems = cntItems;
                        //    }

                        //    if (itm.Id_Section != sectionId)
                        //    {
                        //        section = new CourseSectionsOffline();

                        //        section.identity = itm.Id_Section;
                        //        section.courseName = itm.Student_classe + " " + itm.CourseName;
                        //        section.sectionName = itm.SectionName;
                        //        section.cntNotDone = !itm.IsDone ? 1 : 0;
                        //        section.accomplishedPerc = itm.HasResources && itm.IsAccomplished ? 100 : 0;
                        //        section.courseResources = new List<CourseResourcesOffline>();

                        //        course.courseSections.Add(section);

                        //        RecNo = 1;
                        //        sectionId = itm.Id_Section;
                        //        cntNotDone = !itm.IsDone ? 1 : 0;
                        //        cntResources = itm.HasResources ? 1 : 0;
                        //        cntAccomplishedResources = itm.IsAccomplished ? 1 : 0;
                        //    }
                        //    else
                        //    {
                        //        RecNo += 1;
                        //        cntNotDone += !itm.IsDone ? 1 : 0;
                        //        section.cntNotDone = cntNotDone;
                        //        cntResources += (itm.HasResources ? 1 : 0);
                        //        cntAccomplishedResources += (itm.IsAccomplished ? 1 : 0);

                        //        section.accomplishedPerc = cntResources == 0 ? 0 : Math.Round(100 * cntAccomplishedResources / Convert.ToDecimal(cntResources), 2);
                        //    }

                        //    CourseResourcesOffline resource = new CourseResourcesOffline();

                        //    resource.timeline_id = RecNo.ToString();
                        //    resource.IsToday = itm.PublishDate == null ? 0 : (itm.PublishDate.Value.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd") ? 1 : 0);
                        //    resource.Color = itm.Color;
                        //    resource.IconURL = itm.IconURL;
                        //    resource.AllowEdit = itm.AllowEdit;
                        //    resource.Id_ForeignKey = itm.Id_ForeignKey;
                        //    resource.Id_Section = itm.Id_Section;
                        //    resource.HasSkills = itm.HasSkills > 0 ? true : false;
                        //    resource.Id_MailType = itm.Id_MailType;
                        //    resource.guidid = Guid.NewGuid().ToString();
                        //    resource.classematiere = itm.Id_Course;
                        //    resource.utype = param.uType;
                        //    resource.StorySubject = itm.StorySubject;
                        //    resource.AllowCopyToOtherCourseSection = false;
                        //    resource.AllowCloneToOtherCourse = false;
                        //    resource.ExtraDetails = itm.ExtraDetails == "" || itm.StoryReadOnly ? "" : itm.ExtraDetails + (param.language.ToUpper() == "FR" ? " réponses " : param.language.ToUpper() == "EN" ? " replies " : " رد ");
                        //    resource.StoryBody = itm.StoryBody.Replace("src=\"/Uploads/", "src=\"" + ONLINE_URL + "/Uploads/");
                        //    resource.PublishDate = itm.PublishDate != null ? itm.PublishDate.Value.ToString("dd/MM/yyyy") : "";
                        //    resource.till_text = itm.DueDate != null && resource.Id_MailType == 6 ? (param.language == "FR" ? " jusqu'à " : param.language == "EN" ? " Till " : " حتى ") : "";
                        //    resource.DueDate = itm.DueDate != null && resource.Id_MailType == 6 ? itm.DueDate.Value.ToString("dd/MM/yyyy") : "";
                        //    resource.Checked = itm.IsAccomplished;
                        //    resource.IsAccomplished = itm.IsAccomplished;
                        //    resource.AllowMarkAsDoneByStudent = itm.AllowMarkAsDoneByStudent && itm.StoryReadOnly ? true : false;
                        //    resource.IsDone = itm.IsDone;
                        //    resource.DoneStatus = !itm.IsDone;
                        //    resource.id_student = itm.Id_UserStudent;
                        //    resource.card_class = !itm.AllowMarkAsDoneByStudent ? "card" : itm.IsDone ? "card card-stats-success" : "card card-stats-warning";
                        //    resource.id_student = itm.Id_Student;
                        //    resource.ColumnLabel = itm.ColumnLabel;
                        //    resource.TypeDesc = itm.TypeDesc;
                        //    resource.for_text = param.language == "FR" ? " pour " : param.language == "EN" ? " for " : " ل ";
                        //    resource.Id_Classe = itm.Id_Class;
                        //    resource.NumColumn = itm.NumColumn;
                        //    resource.StorySubject = itm.StorySubject;
                        //    resource.ExtraDetails = itm.ExtraDetails == "" ? "" : "<br/>" + resource.ExtraDetails;
                        //    resource.ResourceFiles = new List<ResourceFilesOffline>();

                        //    List<ResourceFilesOffline> TimelineFilesList = new List<ResourceFilesOffline>();
                        //    //         Homeworks        ||         Forums          ||          Pages           ||     Video Conferences    ||   Videos (Youtube, ...)
                        //    if (resource.Id_MailType == 6 || resource.Id_MailType == 7 || resource.Id_MailType == 16 || resource.Id_MailType == 17 || resource.Id_MailType == 18)
                        //    {
                        //        foreach (EntSP_MailFilesResult file in frntComm.GetMailFiles(resource.Id_ForeignKey, 0, Convert.ToInt32(param.collegeId), "main"))
                        //        {
                        //            ResourceFilesOffline timelineFile = new ResourceFilesOffline();

                        //            timelineFile.fileIdentity = file.ID;
                        //            timelineFile.fileCategory = file.FileCategory;
                        //            timelineFile.fileIconURL = ONLINE_URL + "images/" + file.FileIconURL.Replace(".png", "_64x64.png");
                        //            timelineFile.fileName = file.FileName;
                        //            timelineFile.filePath = ONLINE_URL + file.FilePath;
                        //            timelineFile.fileType = file.FileType;

                        //            string mime;

                        //            if (timelineFile.fileType == "pdf")
                        //                mime = "application/pdf";
                        //            else if (timelineFile.fileType == "jpeg" || timelineFile.fileType == "jpg")
                        //                mime = "image/jpeg";
                        //            else if (timelineFile.fileType == "doc")
                        //                mime = "application/msword";
                        //            else if (timelineFile.fileType == "docx")
                        //                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        //            else if (timelineFile.fileType == "gif")
                        //                mime = "image/gif";
                        //            else if (timelineFile.fileType == "png")
                        //                mime = "image/png";
                        //            else
                        //                mime = "";

                        //            timelineFile.fileMIMEType = mime;

                        //            TimelineFilesList.Add(timelineFile);
                        //        }
                        //    }
                        //    // Agenda
                        //    else if (resource.Id_MailType == 15)
                        //    {
                        //        foreach (EntSP_FilesByAgendaIDResult file in teacherComm.GetEnt_FilesByAgendaID(resource.Id_ForeignKey, Convert.ToInt32(param.collegeId)))
                        //        {
                        //            ResourceFilesOffline timelineFile = new ResourceFilesOffline();

                        //            timelineFile.fileIdentity = file.ID;
                        //            timelineFile.fileCategory = file.FileCategory;
                        //            timelineFile.fileIconURL = ONLINE_URL + "images/" + file.IconURL.Replace(".png", "_64x64.png");
                        //            timelineFile.fileName = file.FileName;
                        //            timelineFile.filePath = ONLINE_URL + file.FolderUploaded;
                        //            timelineFile.fileType = file.FileType;

                        //            string mime;

                        //            if (file.FileType == "pdf")
                        //                mime = "application/pdf";
                        //            else if (file.FileType == "jpeg" || timelineFile.fileType == "jpg")
                        //                mime = "image/jpeg";
                        //            else if (file.FileType == "doc")
                        //                mime = "application/msword";
                        //            else if (file.FileType == "docx")
                        //                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        //            else if (file.FileType == "gif")
                        //                mime = "image/gif";
                        //            else if (file.FileType == "png")
                        //                mime = "image/png";
                        //            else
                        //                mime = "";

                        //            timelineFile.fileMIMEType = mime;

                        //            TimelineFilesList.Add(timelineFile);
                        //        }
                        //    }

                        //    resource.ResourceFiles = TimelineFilesList;
                        //    section.courseResources.Add(resource);
                        //}

                        //return new DownloadTimelineResponse(1, "", children);

                        if (param.studentId == 0)
                        {
                            ChildrenResponse cr = GetFamilyChildren(param);
                            if (cr.status == 1)
                            {
                                foreach (Children chld in cr.data)
                                {
                                    ChildrenOffline co = new ChildrenOffline();
                                    co.identity = chld.identity;
                                    co.collegeIdentity = chld.collegeIdentity;
                                    co.username = chld.username;
                                    co.displayName = chld.displayName;
                                    co.firstName = chld.firstName;
                                    co.classe = chld.classe;
                                    co.classCode = chld.classCode;
                                    co.studentIdentity = chld.studentIdentity;
                                    co.parentIdentity = chld.parentIdentity;
                                    co.image = chld.image;
                                    co.sessionId = chld.sessionId;
                                    co.agenda = chld.agenda;
                                    co.evaluation = chld.evaluation;
                                    co.remarks = chld.remarks;
                                    co.reportCard = chld.reportCard;
                                    co.absences = chld.absences;
                                    co.skills = chld.skills;
                                    co.timeline = chld.timeline;
                                    co.forum = chld.forum;
                                    co.quiz = chld.quiz;
                                    children.Add(co);
                                }
                            }
                        }
                        else
                        {
                            StudentResponse sr = GetChildrenInfo(param);
                            if (sr.status == 1)
                            {
                                Children chdl = sr.data;
                                ChildrenOffline co = new ChildrenOffline();
                                co.identity = chdl.identity;
                                co.collegeIdentity = chdl.collegeIdentity;
                                co.username = chdl.username;
                                co.displayName = chdl.displayName;
                                co.firstName = chdl.firstName;
                                co.classe = chdl.classe;
                                co.classCode = chdl.classCode;
                                co.studentIdentity = chdl.studentIdentity;
                                co.parentIdentity = chdl.parentIdentity;
                                co.image = chdl.image;
                                co.sessionId = chdl.sessionId;
                                co.agenda = chdl.agenda;
                                co.evaluation = chdl.evaluation;
                                co.remarks = chdl.remarks;
                                co.reportCard = chdl.reportCard;
                                co.absences = chdl.absences;
                                co.skills = chdl.skills;
                                co.timeline = chdl.timeline;
                                co.forum = chdl.forum;
                                co.quiz = chdl.quiz;
                                children.Add(co);
                            }
                        }
                        //List<ChildrenOffline> childrenC = new List<ChildrenOffline>(children);
                        foreach (ChildrenOffline child in children)
                        {
                            ChildrenCoursesResponse ccr = GetChildrenCourses(new TokenParam
                            {
                                collegeId = param.collegeId,
                                token = param.token,
                                studentId = child.studentIdentity,
                                UserId = child.identity,
                                uType = "student",
                                language = param.language
                            });

                            List<MainCourses> mcList = ccr.data;
                            List<MainCoursesOffline> mcoList = new List<MainCoursesOffline>();

                            foreach (MainCourses mc in mcList)
                            {
                                MainCoursesOffline mco = new MainCoursesOffline();
                                mco.identity = mc.identity;
                                mco.parentId = mc.parentId;
                                mco.childId = mc.childId;
                                mco.subjectName = mc.subjectName;
                                mco.cntItems = mc.cntItems;

                                List<MainCoursesOffline> soList = new List<MainCoursesOffline>();
                                foreach (SubCourses sub in mc.lstSubCourses)
                                {
                                    MainCoursesOffline so = new MainCoursesOffline();
                                    so.identity = sub.identity;
                                    so.parentId = sub.parentId;
                                    so.childId = sub.childId;
                                    so.subjectName = sub.subjectName;
                                    so.cntItems = sub.cntItems;

                                    soList.Add(so);
                                }
                                //mco.lstSubCourses = soList;
                                mcoList.Add(mco);
                                mcoList.AddRange(soList);
                            }

                            foreach (MainCoursesOffline mco in mcoList)
                            {
                                CourseSectionsResponse courseSecRes = GetCourseSections(new TokenParam
                                {
                                    collegeId = param.collegeId,
                                    token = param.token,
                                    uType = "student",
                                    studentId = child.studentIdentity,
                                    UserId = child.identity,
                                    subjectId = mco.identity,
                                    language = param.language
                                });

                                List<CourseSections> courseSec = courseSecRes.data;
                                List<CourseSectionsOffline> courseSecOffline = new List<CourseSectionsOffline>();
                                foreach (CourseSections courseSection in courseSec)
                                {
                                    CourseSectionsOffline cso = new CourseSectionsOffline();
                                    cso.identity = courseSection.identity;
                                    cso.courseName = courseSection.courseName;
                                    cso.sectionName = courseSection.sectionName;
                                    cso.cntNotDone = courseSection.cntNotDone;
                                    cso.accomplishedPerc = courseSection.accomplishedPerc;

                                    CourseResourcesResponse courseResourceResponse = GetCourseResources(new TokenParam
                                    {
                                        collegeId = param.collegeId,
                                        token = param.token,
                                        studentId = child.studentIdentity,
                                        subjectId = mco.identity,
                                        sectionId = cso.identity,
                                        language = param.language
                                    });
                                    List<CourseResources> courseResources = courseResourceResponse.data;
                                    List<CourseResourcesOffline> courseResourcesOffline = new List<CourseResourcesOffline>();
                                    foreach (CourseResources courseResource in courseResources)
                                    {
                                        CourseResourcesOffline cro = new CourseResourcesOffline();
                                        cro.timeline_id = courseResource.timeline_id;
                                        cro.IsToday = courseResource.IsToday;
                                        cro.Color = courseResource.Color;
                                        cro.IconURL = courseResource.IconURL;
                                        cro.AllowEdit = courseResource.AllowEdit;
                                        cro.Id_ForeignKey = courseResource.Id_ForeignKey;
                                        cro.Id_Section = courseResource.Id_Section;
                                        cro.HasSkills = courseResource.HasSkills;
                                        cro.Id_MailType = courseResource.Id_MailType;
                                        cro.guidid = courseResource.guidid;
                                        cro.classematiere = courseResource.classematiere;
                                        cro.utype = courseResource.utype;
                                        cro.StorySubject = courseResource.StorySubject;
                                        cro.AllowCopyToOtherCourseSection = courseResource.AllowCopyToOtherCourseSection;
                                        cro.AllowCloneToOtherCourse = courseResource.AllowCloneToOtherCourse;
                                        cro.ExtraDetails = courseResource.ExtraDetails;
                                        cro.StoryBody = courseResource.StoryBody;
                                        cro.PublishDate = courseResource.PublishDate;
                                        cro.till_text = courseResource.till_text;
                                        cro.DueDate = courseResource.DueDate;
                                        cro.Checked = courseResource.Checked;
                                        cro.disabled = courseResource.disabled;
                                        cro.onchange = courseResource.onchange;
                                        cro.IsAccomplished = courseResource.IsAccomplished;
                                        cro.AllowMarkAsDoneByStudent = courseResource.AllowMarkAsDoneByStudent;
                                        cro.AllowCloneFromPreviousYear = courseResource.AllowCloneFromPreviousYear;
                                        cro.IsDone = courseResource.IsDone;
                                        cro.DoneStatus = courseResource.DoneStatus;
                                        cro.id_student = courseResource.id_student;
                                        cro.card_class = courseResource.card_class;
                                        cro.LinkAccess = courseResource.LinkAccess;
                                        cro.URL = courseResource.URL;
                                        cro.Id_Classe = courseResource.Id_Classe;
                                        cro.ColumnLabel = courseResource.ColumnLabel;
                                        cro.TypeDesc = courseResource.TypeDesc;
                                        cro.for_text = courseResource.for_text;
                                        cro.NumColumn = courseResource.NumColumn;
                                        List<ResourceFilesOffline> filesList = new List<ResourceFilesOffline>();
                                        foreach (ResourceFiles file in courseResource.ResourceFiles)
                                        {
                                            ResourceFilesOffline rfo = new ResourceFilesOffline();
                                            rfo.fileIdentity = file.fileIdentity;
                                            rfo.fileType = file.fileType;
                                            rfo.fileCategory = file.fileCategory;
                                            rfo.fileIconURL = file.fileIconURL;
                                            rfo.fileName = file.fileName;
                                            rfo.filePath = file.filePath;
                                            rfo.fileMIMEType = file.fileMIMEType;

                                            filesList.Add(rfo);
                                        }
                                        cro.ResourceFiles = filesList;
                                        courseResourcesOffline.Add(cro);
                                    }

                                    cso.courseResources = courseResourcesOffline;
                                    courseSecOffline.Add(cso);
                                }

                                mco.courseSections = courseSecOffline;
                            }

                            child.courses = mcoList;
                        }

                        return new DownloadTimelineResponse(1, "", children);
                    }
                }
            }
            catch (Exception e)
            {
                AdminComm defCom = new AdminComm(ConfigurationManager.ConnectionStrings["ISISOnlineConnectionString"].ConnectionString);
                List<DBContext.Admin.Usr_College> c = defCom.GetUsr_CollegeByID(Convert.ToInt32(param.collegeId));

                if (c.Count == 0)
                    return new DownloadTimelineResponse(-100, param.language == "EN" ? "Invalid school" : param.language == "FR" ? "Collège invalide" : "مدرسة غير صالحة", null);
                else
                {
                    string ConnectionString = c[0].ConnectionString;
                    AdminComm comm = new AdminComm(ConnectionString);

                    comm.LogError(Convert.ToInt32(param.collegeId), "ServiceDARS", "DownloadTimeline", e.Message);

                    return new DownloadTimelineResponse(-1, param.language == "EN" ? "Error registering" : param.language == "FR" ? "Erreur d'enregistrement" : "خطأ في التسجيل", null);
                }
            }
        }
    }
}
