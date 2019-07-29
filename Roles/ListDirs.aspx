<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListDirs.aspx.cs" Inherits="Kesco.App.Web.Roles.ListDirs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><%= Resx.GetString("ROLES_FolderList") %></title>
    <script src="Kesco.Roles.js?v=1" type="text/javascript"></script>
    <style type="text/css">
        .marginL { margin-left: 5px; }
    </style>
</head>
<body>
<form id="mvcDialogResult" action="<%= Request["callbackUrl"] %>" method="post">
    <input type="hidden" name="escaped" value="0"/>
    <input type="hidden" name="control" value=""/>
    <input type="hidden" name="multiReturn" value=""/>
    <input type="hidden" name="value" value=""/>
</form>

<%= RenderDocumentHeader() %>
<h1 class="marginL"><%= Resx.GetString("ROLES_FolderList") %></h1>
<div class="v4formContainer">
    <div id="divDataList" style="overflow: auto" class="divCheck">
        <% RenderDataList(Response.Output); %>
    </div>
</div>
</body>
</html>