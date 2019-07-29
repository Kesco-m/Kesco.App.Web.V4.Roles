using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
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
    ///     Класс формы Список общих папок
    /// </summary>
    public partial class ListDirs : EntityPage
    {
        /// <summary>
        ///     Нужно ли возвращать значение
        /// </summary>
        private int _returnState;

        /// <summary>
        ///     Список выбранных папок
        /// </summary>
        private List<string> _selectedIds = new List<string>();

        #region Init controls

        /// <summary>
        ///     Инициализация кнопок меню
        /// </summary>
        private void SetMenuButtons()
        {
            var btnChoose = new Button
            {
                ID = "btnChoose",
                V4Page = this,
                Text = Resx.GetString("ppBtnChoose"),
                Title = Resx.GetString("ROLES_labelChooseFolders"),
                IconJQueryUI = ButtonIconsEnum.Ok,
                Width = 105,
                OnClick = "roles_getCheckedItemsDataList();"
            };

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
                OnClick = "v4_closeWindow();"
            };

            if (_returnState == 2)
            {
                Button[] buttons = {btnChoose, btnRefresh, btnClose};
                AddMenuButton(buttons);
            }
            else
            {
                Button[] buttons = {btnRefresh, btnClose};
                AddMenuButton(buttons);
            }
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
                if (Request.QueryString["return"] != null && Request.QueryString["return"] != "")
                    _returnState = int.Parse(Request.QueryString["return"]);
                if (Request.QueryString["ids"] != null && Request.QueryString["ids"] != "")
                    _selectedIds = Request.QueryString["ids"].Split(',').ToList();
                JS.Write(@"domain='{0}';", Config.domain);
            }
        }

        protected override void EntityInitialization(Entity copy = null)
        {
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
                        SQLQueries.SELECT_CommonFolders, CommandType.Text,
                        Config.DS_user))
            {
                if (dbReader.HasRows)
                {
                    var colКодОбщейПапки = dbReader.GetOrdinal("КодОбщейПапки");
                    var colОбщаяПапка = dbReader.GetOrdinal("ОбщаяПапка");

                    w.Write("<table class='grid'>");
                    w.Write("<tr class='gridHeader'>");
                    if (_returnState != 0) w.Write("<td></td>");
                    w.Write("<td>{0}</td>", Resx.GetString("sName"));
                    w.Write("</tr>");
                    while (dbReader.Read())
                    {
                        w.Write("<tr>");
                        if (_returnState == 1)
                        {
                            w.Write(
                                "<td><a href=\"javascript:void();\" onclick=\"v4_returnValue({0},'{1}');\"><img src=\"/styles/backtolist.gif\" border=0/></a></td>",
                                dbReader.GetInt32(colКодОбщейПапки),
                                HttpUtility.HtmlEncode(dbReader.GetString(colОбщаяПапка)));
                        }
                        else if (_returnState == 2)
                        {
                            var checkedItem = _selectedIds.Contains(dbReader.GetInt32(colКодОбщейПапки).ToString());

                            w.Write("<td><input type=\"checkbox\" id=\"ch{0}\" data-id=\"{0}\" data-name=\"{1}\" {2}/>",
                                dbReader.GetInt32(colКодОбщейПапки),
                                HttpUtility.HtmlEncode(dbReader.GetString(colОбщаяПапка)),
                                checkedItem ? "checked" : ""
                            );
                        }

                        w.Write("<td>{0}</td>", HttpUtility.HtmlEncode(dbReader.GetString(colОбщаяПапка)));

                        w.Write("</tr>");
                    }

                    w.Write("</table>");
                }
            }
        }

        #endregion
    }
}