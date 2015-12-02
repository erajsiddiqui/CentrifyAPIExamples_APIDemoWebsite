<%@ Page Title="Applications" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Apps.aspx.cs" Inherits="Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <div class="row" runat="server" id="NotLoggedIn">
        <h2>Please Log In</h2>
            <p>
                You are not logged in. Please log in to access restricted content.
            </p>
            <p>
                <a class="btn btn-default" runat="server" href="~/Account/Login">Log In &raquo;</a>
            </p>
    </div>
    <div class="row" runat="server" id="Apps">
        <h2>Centrify Applications</h2>
            <p>
                This is a list of applications from Centrify that you are allowed to access
            </p>
    </div>
</asp:Content>
