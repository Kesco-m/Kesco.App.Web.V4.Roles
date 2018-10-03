<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Roles.aspx.cs" Inherits="Kesco.App.Web.Roles.Roles" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="base" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><%= Resx.GetString("ROLES_AppTitle") %></title>
    <script src="Kesco.Roles.js" type="text/javascript"></script>
    <script src="/styles/Kesco.V4/JS/jquery.floatThead.min.js" type="text/javascript"></script>
    <style type="text/css">
        .floatLeft { float: left; }

        .marginL { margin-left: 5px; }

        .marginT { margin-top: 3px; }

        .marginT2 { margin-top: 7px; }
        
        .marginB2 { margin-bottom: 7px; }

        .w75 {
            white-space: nowrap;
            width: 75px;
        }

        .w100 {
            white-space: nowrap;
            width: 100px;
        }

        .disp_inlineBlock {
            display: inline-block;
            vertical-align: top;
        }

        .disp_inlineBlockS {
            display: inline-block;
            height: 99%;
            vertical-align: top;
        }

  
        
    </style>
</head>
<body>
<%= RenderDocumentHeader() %>
<h1 class="marginL"><%= Resx.GetString("ROLES_AppTitle") %></h1>

<div class="v4formContainer">

    <div id="divFilter" class="marginL marginT">
        <div class="floatLeft w75">
            <%= Resx.GetString("ROLES_labelRole") %>:
        </div>
        <div class="disp_inlineBlock">
            <div class="disp_inlineBlockS ">
                <dbs:DBSRole ID="efFilter_Role" TabIndex="1" runat="server" Width="300" OnChanged="efFilter_Role_OnChanged" AutoSetSingleValue="True" MaxItemsInPopup="100" MaxItemsInQuery="101"></dbs:DBSRole>
            </div>
            <div class="disp_inlineBlockS ">
                <dbs:DBSPersonRole ID="efFilter_Customer" TabIndex="2" runat="server" Width="250" OnChanged="efFilter_Customer_OnChanged" AutoSetSingleValue="True" IsCustomRecordInPopup="True" CustomRecordId="0" CustomRecordText="<все организации>" MaxItemsInPopup="300" MaxItemsInQuery="301"></dbs:DBSPersonRole>
            </div>
        </div>
        <div id="divRoleDescription" class="marginT2"></div>
    </div>

    <div class="marginL">
        <div id="divData" class="marginT2 gridWrapper"></div>
    </div>
</div>


<!--================ Добавление роли сотрудникам ================-->
<div id="divRoleAdd" style="display: none;">
    <div id="divRoleAdd_Body" class="marginL marginT">
        <div class="marginT marginL">
            <div class="floatLeft w100">
                <%= Resx.GetString("ROLES_labelRole") %>:
            </div>
            <div class="disp_inlineBlockS">
                <dbs:DBSRole ID="efPosition_Role" runat="server" Width="230" OnChanged="efPosition_Role_OnChanged" AutoSetSingleValue="True" IsRequired="True" MaxItemsInPopup="100" MaxItemsInQuery="101"></dbs:DBSRole>
            </div>
        </div>
        <div class="marginT2 marginL">
            <div class="floatLeft w100">
                <%= Resx.GetString("cEmplName") %>:
            </div>
            <div class="disp_inlineBlockS">
                <dbs:DBSEmployee ID="efPosition_Employee" runat="server" Width="230" OnChanged="efPosition_Employee_OnChanged" AutoSetSingleValue="True" IsAlwaysAdvancedSearch="True" IsRequired="True"></dbs:DBSEmployee>
            </div>
        </div>
        <div class="marginT2 marginL">
            <div class="floatLeft w100">
                <%= Resx.GetString("lblOrganization") %>:
            </div>
            <div class="disp_inlineBlockS">
                <dbs:DBSPerson ID="efPosition_Customer" runat="server" Width="230" OnChanged="efPosition_Customer_OnChanged" AutoSetSingleValue="True" IsCustomRecordInPopup="True" CustomRecordId="0" CustomRecordText="<все организации>"></dbs:DBSPerson>
            </div>
        </div>
        <div class="marginT2 marginL">
            <div class="floatLeft w100">
                <%= Resx.GetString("lblBProject") %>:
            </div>
            <div class="disp_inlineBlockS">
                <dbs:DBSBusinessProject ID="efPosition_BProject" runat="server" Width="230" OnChanged="efPosition_BProject_OnChanged" AutoSetSingleValue="True" MaxItemsInPopup="100" MaxItemsInQuery="101"></dbs:DBSBusinessProject>
            </div>
        </div>       
    </div>

     <div class="marginT2 marginL marginB2" style="font-size: 7pt">            
            <div>&nbsp;</div>
            <base:Changed runat="server" ID="efChanged"></base:Changed>
           
     </div>

   
       
</div>
</body>
</html>