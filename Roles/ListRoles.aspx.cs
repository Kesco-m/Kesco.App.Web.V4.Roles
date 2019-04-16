using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Web;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Roles
{
    /// <summary>
    ///     Класс формы Список ролей
    /// </summary>
    public partial class ListRoles : EntityPage
    {
        /// <summary>
        ///     Нужно ли возвращать значение
        /// </summary>
        private bool _returnState;

        #region Init controls

        /// <summary>
        ///     Инициализация кнопок меню
        /// </summary>
        private void SetMenuButtons()
        {
            var btnRefresh = new Button
            {
                ID = "btnRefresh",
                V4Page = this,
                Text = Resx.GetString("cmdRefresh"),
                Title = Resx.GetString("cmdRefreshTitle"),
                IconJQueryUI = ButtonIconsEnum.Refresh,
                Width = 105,
                OnClick = "roles_refreshDataList();"
            };

            var btnClose = new Button
            {
                ID = "btnClose",
                V4Page = this,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdCloseTitleApp"),
                IconJQueryUI = ButtonIconsEnum.Close,
                Width = 105,
                //Нажатие кнопки приведет к закрытию окна методом window.close();
                OnClick = "v4_closeWindow();"
            };

            Button[] buttons = {btnRefresh, btnClose};

            AddMenuButton(buttons);
        }

        #endregion

        #region Override

        /// <summary>
        ///     Задание ссылки на справку
        /// </summary>
        public override string HelpUrl
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
            if (!V4IsPostBack)
            {
                if (Request.QueryString["return"] != null) _returnState = true;
                JS.Write(@"domain='{0}';", Config.domain);
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
                case "RefreshDataList":
                    RefreshDataList();
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
            RestoreCursor();
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
        ///     Отрисовка таблицы с данными
        /// </summary>
        protected void RefreshDataList()
        {
            var w = new StringWriter();
            RenderDataList(w);
            JS.Write("var objRoleList= gi('divDataList'); if (objRoleList) objRoleList.innerHTML = '{0}';",
                HttpUtility.JavaScriptStringEncode(w.ToString()));
        }

        /// <summary>
        ///     Подготовка разметки для отрисовки таблицы с данными
        /// </summary>
        /// <param name="w">Поток вывода</param>
        protected void RenderDataList(TextWriter w)
        {
            using (
                var dbReader =
                    new DBReader(
                        SQLQueries.SELECT_Роли + " ORDER BY КодРоли", CommandType.Text,
                        Config.DS_user))
            {
                if (dbReader.HasRows)
                {
                    var colКодРоли = dbReader.GetOrdinal("КодРоли");
                    var colРоль = dbReader.GetOrdinal("Роль");
                    var colОписание = dbReader.GetOrdinal("Описание");

                    w.Write("<table class='grid'>");
                    w.Write("<tr class='gridHeader'>");
                    if (_returnState) w.Write("<td></td>");
                    w.Write("<td>{0}</td>", Resx.GetString("sCode"));
                    w.Write("<td>{0}</td>", Resx.GetString("sName"));
                    w.Write("<td>{0}</td>", Resx.GetString("lblDescriptionForm"));
                    w.Write("</tr>");
                    while (dbReader.Read())
                    {
                        w.Write("<tr>");
                        if (_returnState)
                            w.Write(
                                "<td><a href=\"javascript:void();\" onclick=\"v4_returnValue({0},'{1}');\"><img src=\"/styles/backtolist.gif\" border=0/></a></td>",
                                dbReader.GetInt32(colКодРоли), HttpUtility.HtmlEncode(dbReader.GetString(colРоль)));
                        w.Write("<td>{0}</td>", dbReader.GetInt32(colКодРоли));
                        w.Write("<td>{0}</td>", HttpUtility.HtmlEncode(dbReader.GetString(colРоль)));
                        w.Write("<td>{0}</td>", HttpUtility.HtmlEncode(dbReader.GetString(colОписание)));
                        w.Write("</tr>");
                    }

                    w.Write("</table>");
                }
            }
        }

        #endregion
    }
}