<%@ Page Title="Manage" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Manage.aspx.cs" Inherits="Account_Manage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <asp:UpdateProgress runat="server" ID="UpdateProgress1" AssociatedUpdatePanelID="UpdatePanel1" DisplayAfter="1000">
        <ProgressTemplate>
            <div class="loadingFade">    
                <div class="loadingProgressAligned">                   
	                <asp:Label ID="lblWait" runat="server" Text="" />
	                <asp:Image ID="imgWait" runat="server" ImageAlign="Middle" Width="150px" Height="150px" ImageUrl="~/Images/loadingMFA.gif" />  
                </div>  
            </div>              
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel runat="server" ID="UpdatePanel1">
        <ContentTemplate>
            <div class="form-horizontal">
                <h4>This page can be used to run various administrative tasks..</h4>
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
                    <div class="row" runat="server" id="Manage">
                        <div class="form-group">
                            <h2>Create User</h2>
                            <h4>Create a user by entering a username and, password, and clicking Create User.</h4>
                            <asp:Label runat="server" ID="UserName_Label" AssociatedControlID="UserName" CssClass="col-md-2 control-label">User name</asp:Label>
                            <div class="form-group">                               
                                <asp:TextBox runat="server" ID="UserName" CssClass="form-control" />                                          
                            </div>
                            <asp:Label runat="server" ID="Password_Label" AssociatedControlID="Password" CssClass="col-md-2 control-label">Password</asp:Label>    
                            <div class="form-group">                         
                                <asp:TextBox runat="server" ID="Password" TextMode="Password" CssClass="form-control" />     
                            </div> 
                            <div class="col-md-offset-5" runat="server" ID="Create_User_Button_Div" >
                                <asp:Button runat="server" OnClick="CreateUser" Text="Create User" ID="Create_User_Button" CssClass="btn btn-default" />                       
                            </div>
                        </div>
                        <div class="form-group">
                            <h2>Modify User</h2>
                            <h4>Modify a user by searching for the user, changing attributes to change, and clicking Modify User.</h4>
                            <asp:Label runat="server" ID="FindUser_UserName_Label" AssociatedControlID="FindUser_UserName" CssClass="col-md-2 control-label">User name</asp:Label>
                            <div class="form-group">                               
                                <asp:TextBox runat="server" ID="FindUser_UserName" CssClass="form-control" />                                          
                            </div>
                            <div class="col-md-offset-5" runat="server" ID="Find_User_Button_Div" >
                                <asp:Button runat="server" OnClick="FindUser" Text="Find User" ID="Find_User_Button" CssClass="btn btn-default" />                       
                            </div>
                        </div>
                        <div class="form-group">
                            <asp:Label runat="server" ID="SetPassword_Label" AssociatedControlID="SetPassword" CssClass="col-md-2 control-label">Set Password</asp:Label>
                            <div class="form-group">                               
                                <asp:TextBox runat="server" ID="SetPassword" CssClass="form-control" /> 
                                <div class="col-md-offset-1">
                                    <asp:CheckBox runat="server" ID="Account_Locked" />
                                    <asp:Label runat="server" ID="Account_Locked_Label" AssociatedControlID="Account_Locked">Account Locked</asp:Label>
                                </div>               
                                <div class="col-md-offset-1">
                                    <asp:CheckBox runat="server" ID="Account_Enabled" />
                                    <asp:Label runat="server" ID="Account_Enabled_Label" AssociatedControlID="Account_Enabled">Account Enabled</asp:Label>
                                </div>      
                                <div class="col-md-offset-5" runat="server" ID="ModifyUser_Div" >
                                    <asp:Button runat="server" OnClick="ModifyUser" Text="Modify User" ID="ModifyUser_Button" CssClass="btn btn-default" />                       
                                </div>                      
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="col-md-10"> 
                                <asp:Literal runat="server" ID="ResultMessage">JSON Result Message</asp:Literal> 
                            </div>
                        </div>                        
                        <div class="form-group">
                            <h2>Manage Roles</h2>
                            <h4>Create a role by typing in a name and pressing the create role button</h4>
                            <asp:Label runat="server" ID="CreateRole_Name_Label" AssociatedControlID="CreateRole_Name" CssClass="col-md-2 control-label">Role Name</asp:Label>
                            <div class="form-group">                               
                                <asp:TextBox runat="server" ID="CreateRole_Name" CssClass="form-control" />                                          
                            </div>
                            <div class="col-md-offset-5" runat="server" ID="CreateRole_Button_Div" >
                                <asp:Button runat="server" OnClick="CreateRole" Text="Create Role" ID="CreateRole_Button" CssClass="btn btn-default" />                       
                            </div>
                            <h4>Add a user to a role by typing in their user name and selecting a role from the drop down</h4>
                            <asp:Label runat="server" ID="UserToAdd_Label" AssociatedControlID="UserToAdd" CssClass="col-md-2 control-label">User To Add</asp:Label>
                            <div class="form-group">                               
                                <asp:TextBox runat="server" ID="UserToAdd" CssClass="form-control" />                                          
                            </div>
                            <asp:Label runat="server" ID="Roles_Label" AssociatedControlID="Roles_Dropdown" CssClass="col-md-2 control-label">Roles</asp:Label>
                            <asp:DropDownList runat="server" ID="Roles_Dropdown" AppendDataBoundItems="true" AutoPostBack="true" CssClass="form-control"/> 
                            <div class="col-md-offset-5" runat="server" ID="Div1" >
                                <asp:Button runat="server" OnClick="AddUserToRole" Text="Add User To Role" ID="AdUserToRole_Button" CssClass="btn btn-default" />                      
                            </div>
                        </div>                    
                    </div>                    
                </div>                
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
