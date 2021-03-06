﻿var roles_userId, roles_roleId, roles_personId;
var roles_clientLocalization = {};

function roles_setElementFocus(className, elId) {
    if (elId != null && elId.length > 0) {
        setTimeout(function() {
                var obj = gi(elId);
                if (obj) {
                    obj.focus();
                }
            },
            100);
    } else
        $("." + className).first().focus();
}

roles_RecordsAdd.form = null;

function roles_RecordsAdd(titleForm, userId, roleId, personId) {
    var title = roles_clientLocalization.ROLES_TitleAddRole;
    if (titleForm && titleForm != "") title = titleForm;
    if (userId && userId != "" && roleId && roleId != "" && personId && personId != "") {
        roles_userId = userId;
        roles_roleId = roleId;
        roles_personId = personId;
    } else
        roles_userId = roles_roleId = roles_personId = "";


    var width = 400;
    var height = 250;
    var onOpen = null; //function () { roles_addRole(); };
    var onClose = function() { v4s_hidePopup(true); };
    var buttons = [
        {
            id: "btnPosition_Save",
            text: roles_clientLocalization.cmdSave,
            icons: {
                primary: v4_buttonIcons.Ok
            },
            width: 100,
            click: roles_Records_Save,
            kescoCheck: 1
        },
        {
            id: "btnPosition_Cancel",
            text: roles_clientLocalization.cmdCancel,
            icons: {
                primary: v4_buttonIcons.Cancel
            },
            width: 100,
            click: roles_Records_Close
        }
    ];

    roles_RecordsAdd.form = v4_dialog("divRoleAdd", $("#divRoleAdd"), title, width, height, onOpen, onClose, buttons);


    $("#divRoleAdd").dialog("option", "title", title);
    roles_RecordsAdd.form.dialog("open");

}

function roles_Records_Save(check) {

    cmdasync("cmd",
        "PositionSave",
        "UserId",
        roles_userId,
        "RoleId",
        roles_roleId,
        "PersonId",
        roles_personId,
        "Check",
        (check == 0) ? 0 : 1);
}

function roles_Records_Close() {
    if (null != roles_RecordsAdd.form)
        roles_RecordsAdd.form.dialog("close");
    cmd("cmd", "ClearArtifact");
    roles_setElementFocus(null, "efFilter_Role_0");
}


function roles_clearData() {
    cmdasync("cmd", "ClearData");
}

function roles_refreshData() {
    cmdasync("cmd", "RefreshData");
}

function roles_refreshDataList() {
    cmdasync("cmd", "RefreshDataList");
}

function roles_addRole() {
    cmd("cmd", "AddRole");
}

function roles_addRoleEmployee(userId, roleId) {

    cmd("cmd", "AddRoleEmployee", "Userid", userId, "RoleId", roleId);
}

function roles_deleteRole(userId, roleId, personId) {
    cmdasync("cmd", "Delete", "UserId", userId, "RoleId", roleId, "PersonId", personId);
}

function roles_editPositionRole(userId, roleId, personId) {
    cmd("cmd", "Edit", "UserId", userId, "RoleId", roleId, "PersonId", personId);
}

function roles_sortRoleList(fieldName) {
    cmdasync("cmd", "Sort", "FieldName", fieldName);

}

function roles_getCheckedItemsDataList() {
    var selected = [];
    var inx = 0;
    $(".divCheck input:checked").each(function() {
        var id = $(this).attr("data-id");
        var name = $(this).attr("data-name");
        selected[inx] = {
            value: id,
            label: name
        };
        inx++;
    });

    v4_returnValueArray(selected);
}


$(document).ready(function() {

    var objFrom_ListRoles = gi("divDataList");
    var objFrom_ListData = gi("divData");
    if (objFrom_ListRoles) $("#divDataList").height($(window).height() - 60);
    if (objFrom_ListData) $("#divData").height($(window).height() - 100);


    $(window).resize(function() {

        if (objFrom_ListRoles) $("#divDataList").height($(window).height() - 60);
        if (objFrom_ListData) $("#divData").height($(window).height() - 100);

    });
});

function fixedHeader() {
    $("table.grid").floatThead({
        position: "absolute",
        scrollContainer: true
    });
}

function fixedHeaderDestroy() {
    $("table.grid").floatThead("destroy");
}