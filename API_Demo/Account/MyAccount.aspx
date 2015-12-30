<%@ Page Title="My Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="MyAccount.aspx.cs" Inherits="Account_Manage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <div class="form-horizontal">
        <h4>This page is used to manage your personal account.</h4>
        <br /><br />
        <asp:PlaceHolder runat="server" ID="ErrorMessage" Visible="false">
            <p class="text-danger">
                <asp:Literal runat="server" ID="FailureText" />
            </p>
        </asp:PlaceHolder>
        <div class="row" runat="server" id="NotLoggedIn">
            <h2>Please Log In</h2>
            <p>
                You are not logged in. Please log in to access restricted content.
            </p>
            <p>
                <a class="btn btn-default" runat="server" href="~/Account/Login">Log In &raquo;</a>
            </p>
        </div>
        <div class="row" runat="server" id="AccountOverview">
            <div class="form-group">
                <asp:Label runat="server" ID="UserName_Label" AssociatedControlID="UserName" CssClass="col-md-2 control-label">User Name</asp:Label>    
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="UserName" CssClass="col-md-2 form-control" />     
                </div> 
                <asp:Label runat="server" ID="LoginName_Label" AssociatedControlID="LoginName" CssClass="col-md-2 control-label">LoginName</asp:Label>
                <div class="form-group">           
                    <asp:TextBox runat="server" ID="LoginName" CssClass="col-md-2 form-control" />                    
                    <asp:TextBox runat="server" ID="Alias" CssClass="col-md-2 form-control" />                                          
                </div>                 
                <asp:Label runat="server" ID="Email_Label" AssociatedControlID="Email" CssClass="col-md-2 control-label">Email</asp:Label>    
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="Email" CssClass="form-control" />     
                </div> 
                <asp:Label runat="server" ID="DisplayName_Label" AssociatedControlID="DisplayName" CssClass="col-md-2 control-label">Display Name</asp:Label>   
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="DisplayName" CssClass="form-control" />     
                </div> 
                <asp:Label runat="server" ID="Mobile_Label" AssociatedControlID="Mobile" CssClass="col-md-2 control-label">Mobile Phone</asp:Label>    
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="Mobile" CssClass="form-control" />     
                </div> 
                <asp:Label runat="server" ID="HomePhone_Label" AssociatedControlID="HomePhone" CssClass="col-md-2 control-label">Home Phone</asp:Label>    
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="HomePhone" CssClass="form-control" />     
                </div> 
                <asp:Label runat="server" ID="OfficePhone_Label" AssociatedControlID="OfficePhone" CssClass="col-md-2 control-label">Office Phone</asp:Label>    
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="OfficePhone" CssClass="form-control" />        
                </div> 
                <br />
                <div class="form-group">   
                    <asp:Button runat="server" OnClick="UpdateSecretQuestion" Text="UpdateSecretQuestion" ID="Update_Secret_Question_Button" CssClass="btn btn-default" />  
                    <asp:Button runat="server" OnClick="ResetPassword" Text="Reset Password" ID="ResetPassword_Button" CssClass="btn btn-default" />
                    <asp:Button runat="server" OnClick="Save" Text="Save" ID="Save_Button" CssClass="btn btn-default" />                          
                </div>
            </div>                                                
        </div>
        <div class="row" runat="server" id="ResetPassword_Div">
            <div class="form-group">
                <asp:Label runat="server" ID="NewPassword_Label" AssociatedControlID="NewPassword" CssClass="col-md-2 control-label">New Password</asp:Label>    
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="NewPassword" CssClass="form-control" />     
                </div> 
                <asp:Label runat="server" ID="ConfirmPassword_Label" AssociatedControlID="ConfirmPassword" CssClass="col-md-2 control-label">Confirm Password</asp:Label>   
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="ConfirmPassword" CssClass="form-control" />     
                </div> 
                <div class="form-group"> 
                    <asp:Button runat="server" OnClick="SubmitPassword" Text="Submit" ID="PassSubmit" CssClass="btn btn-default" />   
                </div>
            </div>
        </div>
        <div class="row" runat="server" id="SecretQuestion_Div">
            <div class="form-group">
                <asp:Label runat="server" ID="SecretQuestion_Label" AssociatedControlID="SecretQuestion" CssClass="col-md-2 control-label">Secret Question</asp:Label>    
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="SecretQuestion" CssClass="form-control" />     
                </div> 
                <asp:Label runat="server" ID="SecretAnswer_Label" AssociatedControlID="SecretAnswer" CssClass="col-md-2 control-label">Secret Answer</asp:Label>   
                <div class="form-group">                         
                    <asp:TextBox runat="server" ID="SecretAnswer" CssClass="form-control" />     
                </div> 
                <div class="form-group"> 
                    <asp:Button runat="server" OnClick="SubmitSecretQuestion" Text="Submit" ID="QuestionSubmit" CssClass="btn btn-default" />   
                </div>
            </div>
        </div>
    </div>                           
</asp:Content>
