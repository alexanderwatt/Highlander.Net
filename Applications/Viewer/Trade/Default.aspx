<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" 
    Inherits="Orion.WebViewer.Trade.Default" EnableEventValidation="false" %>
<%@ Import Namespace="Orion.WebViewer"%>
<%@ Register TagPrefix="uc" TagName="DatePicker" Src="~/Controls/DatePicker.ascx" %>
<%@ Register TagPrefix="uc" TagName="DatePicker2" Src="~/Controls/DatePicker2.ascx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <link href="../../Include/CrrStyleSheet.css" rel="stylesheet" type="text/css" />
    <link href="../../Include/QRStyleSheet.css" rel="stylesheet" type="text/css" />
    <link href="../Web.css" rel="stylesheet" type="text/css" />
    <title>QR Trades</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>

        <table class="headerControls" width="96%">
            <tr>
                <td>Trade Date From: </td>
                <td><uc:DatePicker ID="FromDatePicker" runat="server" LabelText="" /></td>
                <td>To:</td>
                <td><uc:DatePicker2 ID="ToDatePicker" runat="server" LabelText="" /></td>
                <td width="100%"></td>
            </tr>
            <tr>
                <td>Product Type:</td>
                <td><asp:DropDownList ID="ProductTypeDropDownList" runat="server" /></td>
            </tr>
            <tr>
                <td>Trade ID:</td>
                <td><asp:TextBox ID="TradeIdTextBox" runat="server"></asp:TextBox></td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="SubmitButton" runat="server" onclick="SubmitButtonClick" Text="Search" />
                </td>
            </tr>
        </table>
            
        <br />
        <br />
        
        <table>
        <tr>
            <td valign="top">
            <asp:gridview ID="TradesGridView" runat="server" AutoGenerateColumns="False" 
                    AllowPaging="True" 
                    DataKeyNames="TradeId" OnPageIndexChanged="TradesGridViewPageIndexChanged" 
                    CssClass="ListGrid" EnableViewState="False"                      
                    OnRowDataBound="TradesGridViewRowDataBound"                    
                    DataSourceID="TradesObjectDataSource" OnInit="TradesGridViewInit" >
                <PagerSettings Position="Bottom" />
                <RowStyle CssClass="RowStyle" />
                <SelectedRowStyle CssClass="RowSelected" />
                <AlternatingRowStyle CssClass="RowStyleAlternate" />
                <Columns>
                    <asp:TemplateField HeaderText="Trade Date" SortExpression="Date">
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" 
                                Text='<%# Common.FormatDate(Eval("Date")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="TradeId" HeaderText="Trade ID" SortExpression="TradeId" />
                    <asp:BoundField DataField="ProductType" HeaderText="Product Type" SortExpression="ProductType" />
                </Columns>
            </asp:gridview>    
            <asp:ObjectDataSource ID="TradesObjectDataSource" runat="server" 
                SelectMethod="GetTrades" 
                TypeName="Orion.WebViewer.Trade.Business.TradeProvider" EnablePaging="True" 
                SelectCountMethod="GetTradesCount">
                <SelectParameters>
                    <asp:ControlParameter ControlID="FromDateLabel" Name="startDate" 
                        PropertyName="Text" Type="DateTime" />
                    <asp:ControlParameter ControlID="ToDateLabel" Name="endDate" 
                        PropertyName="Text" Type="DateTime" />
                    <asp:ControlParameter ControlID="ProductTypeDropDownList" Name="productType" PropertyName="SelectedValue" 
                        Type="String" />
                    <asp:ControlParameter ControlID="TradeIdTextBox" Name="tradeId" 
                        PropertyName="Text" Type="String" />
                    <asp:Parameter Name="maximumRows" Type="Int32" />
                    <asp:Parameter Name="startRowIndex" Type="Int32" />
                </SelectParameters>
            </asp:ObjectDataSource>
            </td>
            <td>&nbsp;</td>
            <td valign="top">
                <asp:DetailsView ID="TradeDetailsView" runat="server" 
                    AutoGenerateRows="False" DataSourceID="TradeObjectDataSource" 
                    DataKeyNames="TradeId" HeaderText="Trade" >
                    <RowStyle CssClass="RowStyle" />
                    <FieldHeaderStyle CssClass="th" />
                <Fields>
                    <asp:TemplateField HeaderText="Trade Date" SortExpression="Date">
                        <ItemTemplate>
                            <asp:Label ID="Label4" runat="server" Text='<%# Common.FormatDate(Eval("Date")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="TradeId" HeaderText="Trade ID" ReadOnly="True" 
                        SortExpression="TradeId" />
                    <asp:BoundField DataField="ProductType" HeaderText="Product Type" 
                        ReadOnly="True" SortExpression="ProductType" />
                    <asp:BoundField DataField="Source" HeaderText="Source" ReadOnly="True" 
                        SortExpression="Source" />
                    <asp:BoundField DataField="OriginatingPartyId" HeaderText="Originating Party ID" 
                        ReadOnly="True" SortExpression="OriginatingPartyId" />
                    <asp:BoundField DataField="OriginatingPartyName" 
                        HeaderText="Originating Party Name" ReadOnly="True" 
                        SortExpression="OriginatingPartyName" />
                    <asp:BoundField DataField="CounterpartyId" HeaderText="Counterparty ID" 
                        ReadOnly="True" SortExpression="CounterpartyId" />
                    <asp:BoundField DataField="CounterpartyName" HeaderText="Counterparty Name" 
                        ReadOnly="True" SortExpression="CounterpartyName" />
                    <asp:BoundField DataField="TradingBookId" HeaderText="Trading Book ID" 
                        ReadOnly="True" SortExpression="TradingBookId" />
                    <asp:BoundField DataField="TradingBookName" HeaderText="Trading Book Name" 
                        ReadOnly="True" SortExpression="TradingBookName" />
                    <asp:TemplateField HeaderText="Pay Maturity Date" 
                        SortExpression="PayMaturityDate">
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" 
                                Text='<%# Common.FormatDate(Eval("PayMaturityDate")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="PayPV" HeaderText="PayPV" ReadOnly="True" 
                        SortExpression="PayPV" />
                    <asp:BoundField DataField="ReceiveStreamType" HeaderText="Rec Stream Type" 
                        ReadOnly="True" SortExpression="ReceiveStreamType" />
                    <asp:TemplateField HeaderText="Rec Maturity Date" 
                        SortExpression="ReceiveMaturityDate">
                        <ItemTemplate>
                            <asp:Label ID="Label2" runat="server" Text='<%# Common.FormatDate(Eval("ReceiveMaturityDate")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ReceivePV" HeaderText="ReceivePV" ReadOnly="True" 
                        SortExpression="ReceivePV" />
                    <asp:BoundField DataField="NetPV" HeaderText="Net PV" ReadOnly="True" 
                        SortExpression="NetPV" />
                    <asp:TemplateField HeaderText="As At Date" SortExpression="AsatDate">
                        <ItemTemplate>
                            <asp:Label ID="Label3" runat="server" Text='<%# Common.FormatDate(Eval("AsAtDate")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="TradeSvcRev" HeaderText="Trade Svc Rev" 
                        ReadOnly="True" SortExpression="TradeSvcRev" />
                    <asp:BoundField DataField="TradeId" HeaderText="Product"
                        ReadOnly="True" SortExpression="Fpml" 
                        DataFormatString="&lt;a target=_new href=&quot;javascript: void(0)&quot; onclick=&quot;window.open('ProductFpml.aspx?tradeid={0}','productFpml','status=0,toolbar=0,resizable=1,scrollbars=1');return false;&quot;&gt;FpML&lt;/a&gt;" 
                        HtmlEncodeFormatString="False" />
                </Fields>
                    <HeaderStyle CssClass="th" />
            </asp:DetailsView>
                <asp:ObjectDataSource ID="TradeObjectDataSource" runat="server" 
                    SelectMethod="GetTrade" TypeName="Orion.WebViewer.Trade.Business.TradeProvider">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="TradesGridView" Name="tradeId" 
                            PropertyName="SelectedValue" Type="String" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </td>
        </tr>
        </table>
    </div>
    <asp:Label ID="FromDateLabel" Visible="false" runat="server" />
    <asp:Label ID="ToDateLabel" Visible="false" runat="server" />
    </form>
</body>
</html>
