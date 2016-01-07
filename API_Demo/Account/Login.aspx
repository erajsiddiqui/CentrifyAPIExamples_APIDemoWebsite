<%@ Page Title="Log in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Account_Login" Async="true" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">

<h2><%: Title %>.</h2>
    <div class="row">
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
        <div class="col-md-8">
            <div class="form-horizontal" runat="server" id="AlreadyLoggedIn">
                <asp:PlaceHolder runat="server" ID="ErrorMessage_LogOut" Visible="false">
                    <p class="text-danger">
                        <asp:Literal runat="server" ID="FailureText_LogOut" />
                     </p>
                </asp:PlaceHolder>
                <h4>You are already logged in.</h4>
                <asp:Button runat="server" OnClick="LogOut_Click" Text="Log Out" CssClass="btn btn-default" />
            </div>
            <asp:UpdatePanel runat="server" ID="UpdatePanel1">
                <ContentTemplate>
                    <div class="form-horizontal">
                        <h4>Use a Centrify account to log in.</h4>
                        <asp:PlaceHolder runat="server" ID="ErrorMessage" Visible="false">
                            <p class="text-danger">
                                <asp:Literal runat="server" ID="FailureText" />
                            </p>
                        </asp:PlaceHolder>

                        <asp:RequiredFieldValidator runat="server" ControlToValidate="UserName" CssClass="text-danger" ErrorMessage="The user name field is required." />
                        <asp:RequiredFieldValidator runat="server" ID="pwdValidator" ControlToValidate="Password" CssClass="text-danger" ErrorMessage="The password field is required." />
                        <asp:RequiredFieldValidator runat="server" ID="NewPassValidator" ControlToValidate="NewPass" CssClass="text-danger" ErrorMessage="The new password field is required." />
                        <asp:RequiredFieldValidator runat="server" ID="ConfirmNewPassValidator" ControlToValidate="ConfirmNewPass" CssClass="text-danger" ErrorMessage="The confirm password field is required." />                           
                        <div class="form-group">
                            <asp:Label runat="server" ID="UserName_Label" AssociatedControlID="UserName" CssClass="col-md-2 control-label">User name</asp:Label>
                            <div class="form-group">                               
                                <asp:TextBox runat="server" ID="UserName" CssClass="form-control" />                                          
                            </div>
                            <div class="col-md-offset-5" runat="server" ID="Next" >
                                <asp:Button runat="server" OnClick="Next_Login" Text="Next" CssClass="btn btn-default" />                       
                            </div>
                            <asp:Label runat="server" ID="Password_Label" AssociatedControlID="Password" CssClass="col-md-2 control-label">Password</asp:Label>    
                            <div class="form-group">                         
                                <asp:TextBox runat="server" ID="Password" TextMode="Password" CssClass="form-control" />     
                            </div>                      
                            <asp:Label runat="server" ID="AuthMethod_Label" AssociatedControlID="AuthMethod" CssClass="col-md-2 control-label">Authentication Method</asp:Label>
                            <asp:Label runat="server" ID="AuthMethod_Label_Second" AssociatedControlID="AuthMethod_Second" CssClass="col-md-2 control-label">Authentication Method</asp:Label>
                            <div class="form-group">
                                <asp:DropDownList runat="server" ID="AuthMethod" AppendDataBoundItems="true" AutoPostBack="true" OnSelectedIndexChanged="AuthMethod_SelectedIndexChanged" CssClass="form-control"/>     
                                <asp:DropDownList runat="server" ID="AuthMethod_Second" AppendDataBoundItems="true" AutoPostBack="true" OnSelectedIndexChanged="AuthMethod_Second_SelectedIndexChanged" CssClass="form-control"/>               
                            </div>
                            <asp:Label runat="server" ID="SecretQuestion_Label" AssociatedControlID="SecretQuestion" CssClass="col-md-2 control-label">Secret Question</asp:Label>
                            <div class="form-group">                                
                                <asp:TextBox runat="server" ID="SecretQuestion" CssClass="form-control" />                                
                            </div>
                            <asp:Label runat="server" ID="NewPass_Label" AssociatedControlID="NewPass" CssClass="col-md-2 control-label">New Password</asp:Label>
                            <div class="form-group">                                
                                <asp:TextBox runat="server" ID="NewPass" TextMode="Password" CssClass="form-control" />                                
                            </div>
                            <asp:Label runat="server" ID="ConfirmNewPass_Label" AssociatedControlID="ConfirmNewPass" CssClass="col-md-2 control-label">Confirm New Password</asp:Label>
                            <div class="form-group">                                
                                <asp:TextBox runat="server" ID="ConfirmNewPass" TextMode="Password" CssClass="form-control" />                                
                            </div>                           
                            <div class="col-md-offset-2 col-md-5" runat="server" ID="ForgotPass_button">   
                                <asp:Button runat="server" OnClick="ForgotPass" Text="Forgot Password" CausesValidation="False" CssClass="btn btn-default" />  
                            </div> 
                            <div class="col-md-offset-2 col-md-8" runat="server">
                                <div class="checkbox">
                                    <asp:CheckBox runat="server" ID="RememberMe" />
                                    <asp:Label runat="server" ID="RememberMe_Label" AssociatedControlID="RememberMe">Remember me?</asp:Label>
                                </div>          
                                <div class="col-md-offset-5 col-md-1" runat="server" ID="Login" >
                                    <asp:Button runat="server" OnClick="AdvanceAuth" Text="Login" CssClass="btn btn-default" />  
                                </div>          
                                <div class="col-md-offset-5 col-md-1" runat="server" ID="ForgotPass_Submit_button" >
                                    <asp:Button runat="server" OnClick="ForgotPass_Submit" Text="Submit" CssClass="btn btn-default" />  
                                </div>          
                            </div>                        
                        </div>
                        <div class="form-group">
                            <div class="col-md-10"> 
                                <asp:Literal runat="server" ID="MFAMessage">Please keep this window open until MFA challange has been completed..</asp:Literal> 
                            </div>
                            <div class="col-md-10">                               
                                <asp:Label runat="server" ID="MFAAnswer_Label" AssociatedControlID="MFAAnswer" CssClass="col-md-2 control-label">MFA Answer</asp:Label>
                                <div class="col-md-10">                                
                                    <asp:TextBox runat="server" ID="MFAAnswer" CssClass="form-control" />    
                                    <asp:RequiredFieldValidator runat="server" ID="MFAAnswer_Validator" ControlToValidate="MFAAnswer" CssClass="text-danger" ErrorMessage="The MFA Answer field is required." />                    
                                </div>
                            </div>
                            <div class="col-md-10"> 
                                <div class="col-md-offset-5 col-md-10" runat="server" ID="MFAAnswer_Submit" >
                                    <asp:Button runat="server" OnClick="Submit_MFA" Text="Submit" CssClass="btn btn-default" />
                                </div>                              
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>   
            <asp:UpdatePanel runat="server" ID="UpdatePanel2">
                <ContentTemplate>
                    <asp:Timer ID="Timer1" runat="server" OnTick="TimerTick" Interval="7000"></asp:Timer>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>
