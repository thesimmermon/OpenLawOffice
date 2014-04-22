﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<OpenLawOffice.WebClient.ViewModels.Tasks.TaskResponsibleUserViewModel>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    ResponsibleUsers
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        ResponsibleUsers</h2>
    <table class="listing_table">
        <tr>
            <th style="text-align: center;">
                User
            </th>
            <th style="text-align: center;">
                Responsibility
            </th>
            <th style="width: 150px;">
            </th>
        </tr>
        <% foreach (var item in Model)
           { %>
        <tr>
            <td>
                <%: item.User.Username %>
            </td>
            <td>
                <%: item.Responsibility %>
            </td>
            <td>
                <%: Html.ActionLink("Edit", "Edit", "TaskResponsibleUsers", new { id = item.Id.Value }, null)%>
                |
                <%: Html.ActionLink("Details", "Details", "TaskResponsibleUsers", new { id = item.Id.Value }, null)%>
                |
                <%: Html.ActionLink("Delete", "Delete", "TaskResponsibleUsers", new { id = item.Id.Value }, null)%>
            </td>
        </tr>
        <% } %>
    </table>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MenuContent" runat="server">
    <li>Navigation</li>
    <ul style="list-style: none outside none; padding-left: 1em;">
        <li>
            <%: Html.ActionLink("Add Resp. User", "Create", "TaskResponsibleUsers", new { id = RouteData.Values["Id"].ToString() }, null)%></li>
        <li>
            <%: Html.ActionLink("Task", "Details", "Tasks", new { Id = RouteData.Values["Id"].ToString() }, null)%></li>
    </ul>
</asp:Content>