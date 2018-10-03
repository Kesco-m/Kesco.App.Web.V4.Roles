using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Corporate;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Roles
{
    /// <summary>
    ///     Класс формы фильтрации ролей
    /// </summary>
    public partial class Roles : EntityPage
    {
        /// <summary>
        ///     Очередь сортировки
        /// </summary>
        private SpecialQueue<string> _sortOrder;

        #region Init controls

        /// <summary>
        ///     Инициализация/создание кнопок меню
        /// </summary>
        private void SetMenuButtons()
        {
            var btnAdd = new Button
            {
                ID = "btnAdd",
                V4Page = this,
                Text = Resx.GetString("cmdAdd"),
                Title = Resx.GetString("ROLES_cmdAddTitle"),
                Width = 105,
                IconJQueryUI = ButtonIconsEnum.Add,
                OnClick = "roles_addRole();"
            };

            var btnRefresh = new Button
            {
                ID = "btnRefresh",
                V4Page = this,
                Text = Resx.GetString("cmdRefresh"),
                Title = Resx.GetString("cmdRefreshTitle"),
                IconJQueryUI = ButtonIconsEnum.Refresh,
                Width = 105,
                OnClick = "roles_refreshData();"
            };

            var btnClear = new Button
            {
                ID = "btnClear",
                V4Page = this,
                Text = Resx.GetString("lClear"),
                Title = Resx.GetString("cmdClearTitle"),
                IconJQueryUI = ButtonIconsEnum.Cancel,
                Width = 105,
                OnClick = "roles_clearData();"
            };

            var btnClose = new Button
            {
                ID = "btnClose",
                V4Page = this,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdCloseTitleApp"),
                IconJQueryUI = ButtonIconsEnum.Close,
                Width = 105,
                OnClick = "v4_closeWindow();"
            };

            Button[] buttons = {btnAdd, btnRefresh, btnClear, btnClose};

            AddMenuButton(buttons);
        }

        /// <summary>
        ///     Установка handler-ов на контролы
        /// </summary>
        private void SetHandlers()
        {
            efFilter_Role.OnRenderNtf += efFilter_Role_OnRenderNtf;
            efFilter_Customer.BeforeSearch += efFilter_Customer_BeforeSearch;
        }

        /// <summary>
        ///     Инициализация контролоа(фильтры, фокусы)
        /// </summary>
        private void InitControls()
        {
            efFilter_Role.NextControl = "efFilter_Customer_0";
            efFilter_Customer.NextControl = "btnAdd";

            efPosition_Role.NextControl = "efPosition_Employee_0";
            efPosition_Employee.NextControl = "efPosition_Customer_0";
            efPosition_Customer.NextControl = "btnPosition_Save";
            efPosition_BProject.NextControl = "btnPosition_Save";

            efPosition_Employee.Filter.Status.ValueStatus = СотоянияСотрудника.Работающие;
            efPosition_Employee.Filter.HasLogin.ValueHasLogin = НаличиеЛогина.ЕстьЛогин;
            efPosition_Customer.Filter.PersonValidAt = DateTime.Now;
        }

        #endregion

        #region Override

        /// <summary>
        ///     Задание ссылки на справку
        /// </summary>
        protected override string HelpUrl
        {
            get { return ""; }
            set { value = ""; }
        }

        /// <summary>
        ///     Событие загрузки страницы
        /// </summary>
        /// <param name="sender">Объект страницы</param>
        /// <param name="e">Аргументы</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            if (!V4IsPostBack)
            {
                JS.Write(@"roles_clientLocalization = {{
cmdSave:""{0}"",
cmdCancel:""{1}"",
ROLES_TitleAddRole:""{2}""
}};",
                    Resx.GetString("cmdSave"),
                    Resx.GetString("cmdCancel"),
                    Resx.GetString("ROLES_TitleAddRole")
                    );

                SetHandlers();
                efFilter_Role.Focus();
                _sortOrder = new SpecialQueue<string>();
            }
        }

        /// <summary>
        ///     Обработка клиентских команд
        /// </summary>
        /// <param name="cmd">Команды</param>
        /// <param name="param">Параметры</param>
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "RefreshData":
                    RefreshData();
                    break;
                case "ClearArtifact":
                    ClearArtifact();
                    break;
                case "ClearData":
                    ClearData();
                    efFilter_Role.Focus();
                    break;
                case "Delete":
                    Delete(param["UserId"], param["RoleId"], param["PersonId"]);
                    break;
                case "Edit":
                    Edit(param["UserId"], param["RoleId"], param["PersonId"]);
                    break;
                case "Sort":
                    Sort(param["FieldName"]);
                    break;
                case "PositionSave":
                    PositionSave(param["UserId"], param["RoleId"], param["PersonId"],
                        param["Check"] == "1" ? true : false);
                    break;
                case "AddRole":
                    AddRole();
                    break;
                case "AddRoleEmployee":
                    AddRoleEmployee(param["UserId"], param["RoleId"]);
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
            RestoreCursor();
        }

        #endregion

        #region Button events

        /// <summary>
        ///     Кнопка: Добавить
        /// </summary>
        private void AddRole()
        {
            ClearPositionForm();

            efPosition_Role.Value = efFilter_Role.Value;
            efPosition_Customer.Value = efFilter_Customer.Value;

            efChanged.ChangedByID = null;
            OpenPositionForm();
        }

        /// <summary>
        ///     Кнопка: Обновить
        /// </summary>
        private void RefreshData()
        {
            var w = new StringWriter();

            if (efFilter_Role.Value.Length == 0 && efFilter_Customer.Value.Length == 0)
            {
                RenderNtf(w, new List<string> {Resx.GetString("ROLES_msgSetRoleOrOrganization")}, NtfStatus.Recommended);
                RenderData(w);
                return;
            }

            if (_sortOrder.Count == 0)
            {
                _sortOrder.Enqueue("Организация");
                _sortOrder.Enqueue("Сотрудник");
                _sortOrder.Enqueue("Роль");
            }

            var sqlParams = new Dictionary<string, object>
            {
                {"@КодРоли", efFilter_Role.Value.Length == 0 ? -1 : int.Parse(efFilter_Role.Value)},
                {"@КодЛица", efFilter_Customer.Value.Length == 0 ? -1 : int.Parse(efFilter_Customer.Value)}
            };
            var cnPerson = new SqlConnection(Config.DS_person);
            var personServer = cnPerson.DataSource;
            using (
                var dbReader =
                    new DBReader(
                        string.Format(SQLQueries.SELECT_РолиПоиск, personServer) + " ORDER BY " +
                        _sortOrder.ReverseListValues, CommandType.Text,
                        Config.DS_user, sqlParams))
            {
                if (dbReader.HasRows)
                {
                    var colКодСотрудника = dbReader.GetOrdinal("КодСотрудника");
                    var colФИО = dbReader.GetOrdinal("ФИО");
                    var colСотрудник = dbReader.GetOrdinal("Сотрудник");
                    var colКодРоли = dbReader.GetOrdinal("КодРоли");
                    var colРоль = dbReader.GetOrdinal("Роль");
                    var colКодЛица = dbReader.GetOrdinal("КодЛица");
                    var colОрганизация = dbReader.GetOrdinal("Организация");


                    w.Write("<table class='grid' >");
                    w.Write("<thead>");
                    w.Write("<tr class=\"gridHeader\">");
                    w.Write("<th style='width:16px'>&nbsp;</th>");
                    if (efFilter_Role.Value.Length == 0)
                    {
                        w.Write("<th>");
                        w.Write(
                            "<a href=\"javascript:void(0);\" onclick=\"roles_sortRoleList('Роль');\" title='сортировать'>{0}</a>",
                            Resx.GetString("ROLES_labelRole"));
                        w.Write("</th>");
                    }
                    w.Write("<th>");
                    w.Write(
                        "<a href=\"javascript:void(0);\" onclick=\"roles_sortRoleList('Сотрудник');\" title='сортировать'>{0}</a>",
                        Resx.GetString("ROLES_labelEmplInRole"));
                    w.Write("</th>");
                    w.Write("<th>");
                    w.Write(
                        "<a href=\"javascript:void(0);\" onclick=\"roles_sortRoleList('Организация');\" title='сортировать'>{0}</a>",
                        Resx.GetString("ROLES_labelEmplPerson"));
                    w.Write("</th>");
                    w.Write("</tr>");
                    w.Write("</thead>");
                    
                    w.Write("<tbody>");
                     
                    while (dbReader.Read())
                    {
                        w.Write("<tr>");
                        w.Write(
                            "<td><a href=\"javascript:void(0);\" onclick=\"{0}\" title='{1}'><img src='/styles/delete.gif' border=0/></a></td>",
                            ShowConfirmDeleteGetJS(
                                string.Format("'roles_deleteRole({0},{1},{2});'",
                                    dbReader.GetInt32(colКодСотрудника),
                                    dbReader.GetInt32(colКодРоли),
                                    dbReader.GetInt32(colКодЛица)),
                                HttpUtility.HtmlEncode(dbReader.GetString(colСотрудник) + ": " +
                                                       dbReader.GetString(colРоль) + " -> " +
                                                       dbReader.GetString(colОрганизация))
                                ), Resx.GetString("ROLES_labelDeleteEmplRole"));

                        if (efFilter_Role.Value.Length == 0)
                        {
                            w.Write("<td>");
                            w.Write(dbReader.GetString(colРоль));
                            w.Write("</td>");
                        }
                        w.Write("<td>");
                        var htmlId = Guid.NewGuid();
                        RenderLinkEmployee(w, htmlId.ToString(), dbReader.GetInt32(colКодСотрудника).ToString(),
                            dbReader.GetString(colСотрудник), NtfStatus.Empty);
                        w.Write("</td>");
                        w.Write("<td>");
                        w.Write(
                            "<a href=\"javascript:void(0);\" onclick=\"roles_editPositionRole({0},{1},{2});\" title=\"{3}\">{4}</a>",
                            dbReader.GetInt32(colКодСотрудника),
                            dbReader.GetInt32(colКодРоли),
                            dbReader.GetInt32(colКодЛица),
                            Resx.GetString("Msg_РедактироватьПозицию"),
                            dbReader.GetString(colОрганизация));
                        if (dbReader.GetInt32(colКодЛица) > 0)
                        {
                            w.Write(
                                "<a class='marginL' href=\"javascript:void(0);\" onclick=\"roles_addRoleEmployee({0},{1});\" title='{2}'><img src='/styles/new.gif' border=0/></a>",
                                dbReader.GetInt32(colКодСотрудника), dbReader.GetInt32(colКодРоли),
                                Resx.GetString("cmdAddTitle"));
                        }
                        w.Write("</td>");
                        w.Write("</tr>");
                    }
                    w.Write("</tbody>");
                    w.Write("</table>");
                }
                else
                    RenderNtf(w, new List<string> {Resx.GetString("lNoData")}, NtfStatus.Information);
            }

            RenderData(w);
        }

        /// <summary>
        ///     Управление сортировкой по полям результирующей таблицы
        /// </summary>
        /// <param name="field">Поле сортировки</param>
        private void Sort(string field)
        {
            var columnDESC = field + " DESC";
            if (_sortOrder.Count > 0)
            {
                if (_sortOrder.Last().Equals(field))
                {
                    _sortOrder.Remove(field);
                    field = columnDESC;
                }
            }

            _sortOrder.Remove(field);
            _sortOrder.Remove(columnDESC);

            _sortOrder.Enqueue(field);


            RefreshData();
        }

        /// <summary>
        ///     Кнопка: Очистить
        /// </summary>
        private void ClearData()
        {
            efFilter_Role.Value = "";
            efFilter_Customer.Value = "";
            RefreshData();
            RenderRoleDescription();
        }

        /// <summary>
        ///     Кнопка: Удалить(в результирующей таблице)
        /// </summary>
        /// <param name="userId">Код сотрудника</param>
        /// <param name="roleId">Код роли</param>
        /// <param name="personId">Код лица</param>
        private void Delete(string userId, string roleId, string personId)
        {
            var inputParameters = new Dictionary<string, object>();

            inputParameters.Add("@КодСотрудника", int.Parse(userId));
            inputParameters.Add("@КодРоли", int.Parse(roleId));
            inputParameters.Add("@КодЛица", int.Parse(personId));
            DBManager.ExecuteNonQuery(SQLQueries.DELETE_РолиСотрудников, CommandType.Text, Config.DS_user,
                inputParameters);
            RefreshData();
        }

        /// <summary>
        ///     Кнопка: Редактировать(ссылка на сотруднике в результирующей таблице)
        /// </summary>
        /// <param name="userId">Код сотрудника</param>
        /// <param name="roleId">Код роли</param>
        /// <param name="personId">Код лица</param>
        private void Edit(string userId, string roleId, string personId)
        {
            ClearPositionForm();

            efPosition_Role.Value = roleId;
            efPosition_Employee.Value = userId;
            efPosition_Customer.Value = personId;

            var inputParameters = new Dictionary<string, object>
            {
                {"@КодРоли", int.Parse(roleId)},
                {"@КодСотрудника", int.Parse(userId)},
                {"@КодЛица", int.Parse(personId)}
            };

            var dt = DBManager.GetData(SQLQueries.SELECT_РолиСотрудника_КодСотрудника_КодРоли_КодЛица, Config.DS_user,
                CommandType.Text, inputParameters);
            if (dt.Rows.Count == 1)
            {
                efChanged.SetChangeDateTime = (DateTime) dt.Rows[0]["Изменено"];
                efChanged.ChangedByID = (int) dt.Rows[0]["Изменил"];
            }
            else
                efChanged.ChangedByID = null;


            OpenPositionForm(Resx.GetString("ROLES_TitleEditRole"), userId, roleId, personId);
        }

        /// <summary>
        ///     Кнопка: Добавить роль выбранному сотруднику((в результирующей таблице))
        /// </summary>
        /// <param name="userId">Код сотрудника</param>
        /// <param name="roleId">Код роли</param>
        private void AddRoleEmployee(string userId, string roleId)
        {
            ClearPositionForm();

            efPosition_Role.Value = roleId;
            efPosition_Employee.Value = userId;

            efChanged.ChangedByID = null;

            OpenPositionForm();
        }

        #endregion

        #region Handlers

        #region Filter

        /// <summary>
        ///     Событие, валидидующее значение конрола выбора ролей в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="ntf">Класс нотификации</param>
        protected void efFilter_Role_OnRenderNtf(object sender, Ntf ntf)
        {
        }

        /// <summary>
        ///     Событие, устанавливающее параметры фильтрации перед поиском лиц в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        protected void efFilter_Customer_BeforeSearch(object sender)
        {
            if (efFilter_Role.Value.Length > 0)
                efFilter_Customer.Filter.PersonRole.Value = efFilter_Role.Value;
            else
                efFilter_Customer.Filter.PersonRole.Value = "";
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола фильтрации ролей
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void efFilter_Role_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            JS.Write("roles_refreshData();");
            RenderRoleDescription();
            FocusControl = "efFilter_Customer_0";
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола фильтрации лиц
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void efFilter_Customer_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            JS.Write("roles_refreshData();");
        }

        #endregion

        #region Positions

        /// <summary>
        ///     Событие, отслеживающее изменение контрола выбора роли в форме редактирования(позиции)
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void efPosition_Role_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            FocusControl = "efPosition_Employee_0";
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола выбора сотрдуника в форме редактирования(позиции)
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void efPosition_Employee_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            FocusControl = "efPosition_Customer_0";
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола выбора лица в форме редактирования(позиции)
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void efPosition_Customer_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            FocusControl = "btnPosition_Save";
            efPosition_BProject.Value = "";
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола выбора бизнес-проекта в форме редактирования(позиции)
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void efPosition_BProject_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            FocusControl = "btnPosition_Save";
            efPosition_Customer.Value = "";
        }

        #endregion

        #endregion

        #region Positions

        /// <summary>
        ///     Очистка формы редактирования(позиции) перед открытием
        /// </summary>
        private void ClearPositionForm()
        {
            efPosition_Role.Value =
                efPosition_Employee.Value = efPosition_Customer.Value = efPosition_BProject.Value = "";
            efChanged.ChangedByID = null;
        }

        /// <summary>
        ///     Открытие формы редактирования(позиции) с переданными параметрами
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="userId">Код сотрудника</param>
        /// <param name="roleId">Код роли</param>
        /// <param name="personId">Код лица</param>
        private void OpenPositionForm(string title = "", string userId = "", string roleId = "", string personId = "")
        {
            JS.Write("roles_RecordsAdd('{0}', '{1}','{2}','{3}');", HttpUtility.JavaScriptStringEncode(title), userId,
                roleId, personId);

            if (efPosition_Role.Value.Length == 0)
                JS.Write("roles_setElementFocus(null,'efPosition_Role_0');");
            else if (efPosition_Employee.Value.Length == 0)
                JS.Write("roles_setElementFocus(null,'efPosition_Employee_0');");
            else if (efPosition_Customer.Value.Length == 0)
                JS.Write("roles_setElementFocus(null,'efPosition_Customer_0');");
            else if (efPosition_BProject.Value.Length == 0)
            {
                if (efPosition_Employee.Value.Length == 0)
                    JS.Write("roles_setElementFocus(null,'efPosition_BProject_0');");
                else
                    JS.Write("roles_setElementFocus(null,'btnPosition_Save');");
            }
            ClearArtifact();
        }

        private void ClearArtifact()
        {
            return;
            if (efPosition_Role.Value.Length == 0) efPosition_Role.ValueText = "";
            if (efPosition_Employee.Value.Length == 0) efPosition_Employee.ValueText = "";
            if (efPosition_Customer.Value.Length == 0) efPosition_Customer.ValueText = "";
            if (efPosition_BProject.Value.Length == 0) efPosition_BProject.ValueText = "";
        }

        /// <summary>
        ///     Сохранение данных формы редактивания(позиции)
        /// </summary>
        /// <param name="userId">Код сотрудника</param>
        /// <param name="roleId">Код роли</param>
        /// <param name="personId">Код лица</param>
        /// <param name="check">Выполнять проверку данных перед сохранением</param>
        private void PositionSave(string userId, string roleId, string personId, bool check)
        {
            ClearArtifact();
            if (check)
            {
                if (efPosition_Role.Value.Length == 0)
                {
                    ShowMessage(Resx.GetString("ROLES_msgNeedRole"), Resx.GetString("_Msg_RequiredFields"),
                        MessageStatus.Warning,
                        "efPosition_Role_0");
                    return;
                }

                if (efPosition_Employee.Value.Length == 0)
                {
                    ShowMessage(Resx.GetString("ROLES_msgNeedEmpl"), Resx.GetString("_Msg_RequiredFields"),
                        MessageStatus.Warning,
                        "efPosition_Employee_0");
                    return;
                }

                if ((efPosition_Customer.Value.Length == 0 || efPosition_Customer.Value == "0") &&
                    efPosition_BProject.Value.Length == 0)
                {
                    ShowConfirm(Resx.GetString("ROLES_msgConfirmAddAll"),
                        "roles_Records_Save(0);", null);
                    return;
                }
            }

            var inputParameters = new Dictionary<string, object>();


            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(roleId) && !string.IsNullOrEmpty(personId))
            {
                if (efPosition_Role.Value == roleId
                    && efPosition_Employee.Value == userId
                    && efPosition_Customer.Value == personId)
                {
                    //ShowMessage("Данные формы полностью совпадают с записью в БД.<br>Измените данные или закройте форму с помощью кнопки отмена!","Сообщение");
                    JS.Write("roles_Records_Close();");
                    return;
                }

                inputParameters.Add("@КодРоли", int.Parse(roleId));
                inputParameters.Add("@КодСотрудника", int.Parse(userId));
                inputParameters.Add("@КодЛица", int.Parse(personId));
                DBManager.ExecuteNonQuery(SQLQueries.DELETE_РолиСотрудников, CommandType.Text, Config.DS_user,
                    inputParameters);
            }

            inputParameters.Clear();
            inputParameters.Add("@КодРоли", int.Parse(efPosition_Role.Value));
            inputParameters.Add("@КодСотрудника", int.Parse(efPosition_Employee.Value));

            if (efPosition_Customer.Value.Length > 0 || efPosition_BProject.Value.Length == 0)
            {
                inputParameters.Add("@КодЛица",
                    efPosition_Customer.Value.Length == 0 ? 0 : int.Parse(efPosition_Customer.Value));
                DBManager.ExecuteNonQuery(SQLQueries.INSERT_РолиСотрудников, CommandType.Text, Config.DS_user,
                    inputParameters);
            }
            else
            {
                var cnPerson = new SqlConnection(Config.DS_person);
                inputParameters.Add("@КодБизнесПроекта", int.Parse(efPosition_BProject.Value));
                DBManager.ExecuteNonQuery(
                    string.Format(SQLQueries.INSERT_РолиСотрудников_БизнесПроект, cnPerson.DataSource), CommandType.Text,
                    Config.DS_user,
                    inputParameters);
            }

            efFilter_Role.Value = efFilter_Role.ValueText = "";
            efFilter_Customer.Value = efFilter_Customer.ValueText = "";

            efFilter_Role.Value = efPosition_Role.Value;
            efFilter_Customer.Value = efPosition_Customer.Value;

            efChanged.ChangedByID = null;
            RefreshData();
            RenderRoleDescription();

            JS.Write("roles_Records_Close();");
        }

        #endregion

        #region Render

        /// <summary>
        ///     Подготовка данных для отрисовки заголовка страницы(панели с кнопками)
        /// </summary>
        /// <returns></returns>
        protected string RenderDocumentHeader()
        {
            using (var w = new StringWriter())
            {
                try
                {
                    ClearMenuButtons();
                    SetMenuButtons();
                    RenderButtons(w);
                }
                catch (Exception e)
                {
                    var dex = new DetailedException("Не удалось сформировать кнопки формы: " + e.Message, e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                return w.ToString();
            }
        }

        /// <summary>
        ///     Отрисовка описания выранного значения роли(вызывается после изменения значения контрола выбора роли)
        /// </summary>
        private void RenderRoleDescription()
        {
            var w = new StringWriter();

            if (efFilter_Role.Value.Length == 0)
                w.Write("");
            else
            {
                var role = new Lib.Entities.Corporate.Role(efFilter_Role.Value);
                if (role.Unavailable)
                    w.Write("");
                else
                    w.Write("<div class='floatLeft w75'>{1}:</div><div class='disp_inlineBlock'>{0}</div>",
                        role.Description, Resx.GetString("lblDescriptionForm"));
            }
            JS.Write("var objRoleDesrc= gi('divRoleDescription'); if (objRoleDesrc) objRoleDesrc.innerHTML = '{0}';",
                HttpUtility.JavaScriptStringEncode(w.ToString()));
        }

        /// <summary>
        ///     Отрисовка в поток таблицы с данными
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderData(TextWriter w)
        {
            JS.Write("fixedHeaderDestroy();");
            JS.Write("$('#divData').html('{0}');", HttpUtility.JavaScriptStringEncode(w.ToString()));
            JS.Write("fixedHeader();");
        }

        #endregion
    }
}