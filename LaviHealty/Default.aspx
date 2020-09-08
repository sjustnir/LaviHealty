<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LaviHealty.Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="SitePanel" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <div class="jumbotron">
                <h1 class="text-center">
                    Lavi is Healthy? Good!
                </h1>
                <p class="lead text-center">
                    <%: DateTime.Now.ToString("dd/MM/yyyy") %>
                </p>
                <p class="lead text-center">
                    Send online approval to the kindergarten!
                </p>
                <p class="text-center">
                    As: 
                    <asp:DropDownList ID="SendAsDropDown" runat="server" CssClass="GeneralDropDown"/>
                </p>
                <p class="text-center">
                    <asp:LinkButton ID="SendConfirmationButton" runat="server" CssClass="btn btn-primary" Text="Send Confirmation" 
                                   OnClick="SendConfirmationButton_OnClick"
                                   OnClientClick="return confirm('are you sure you want to send confirmation?');"/>
                </p>
            </div>
            <div class="jumbotron" style="margin-left: auto; margin-right: auto; text-align: center;">
                 <asp:Label ID="ResultLabel" CssClass="lead text-center" Visible="False" runat="server"/>
                 <asp:Label ID="LinkToSiteLabel" CssClass="lead text-center" Visible="False" runat="server">
                     <br/><br/>You can 
                     <a href="https://govforms.gov.il/mw/forms/ChildHealthDeclaration@molsa.gov.il" target="_blank">enter the website</a>
                     and fill it manually
                 </asp:Label>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
