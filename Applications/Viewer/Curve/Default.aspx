<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" 
    Inherits="Orion.WebViewer.Curve.Default" EnableEventValidation="false" %>
<%@ Import Namespace="Orion.WebViewer"%>
<%@ Register assembly="Nevron.Chart.WebForm, Version=9.7.23.12, Culture=neutral, PublicKeyToken=346753153ef91008" namespace="Nevron.Chart.WebForm" tagprefix="ncwc" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <link href="../CrrStyleSheet.css" rel="stylesheet" type="text/css" />
    <link href="../QRStyleSheet.css" rel="stylesheet" type="text/css" />
    <link href="../web.css" rel="stylesheet" type="text/css" />
    <script language="JavaScript" type="text/javascript">

        function ClipBoard() {
            Copied = holdtext.createTextRange();
            Copied.execCommand("Copy");
        }

    </script> 
    <title>Curve Viewer</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <span>

        <table class="headerControls" width="96%">
            <tr>
                <td>Market Name:</td>
                <td><asp:TextBox ID="MarketNameTextBox" runat="server"></asp:TextBox></td>
                <td width="100%"></td>
            </tr>
            <tr>
                <td>Pricing Structure:</td>
                <td><asp:DropDownList ID="PricingStructureDropDownList" runat="server" /></td>
            </tr>
            <tr>
                <td>ID:</td>
                <td><asp:TextBox ID="IdTextBox" runat="server"></asp:TextBox></td>
                <td width="100%"></td>
            </tr>
            <tr>
                <td colspan=3>
                    <asp:Button ID="SubmitButton" runat="server" onclick="SubmitButtonClick" Text="Search" />
                </td>
            </tr>
        </table>
            
        </span>
        <br />
        <br />
        
        <table>
        <tr>
            <td valign="top">
            <asp:gridview ID="CurvesGridView" runat="server" AutoGenerateColumns="False" 
                    AllowPaging="True" 
                    DataKeyNames="Id" OnPageIndexChanged="CurvesGridViewPageIndexChanged" 
                    CssClass="ListGrid" EnableViewState="False"                      
                    OnRowDataBound="CurvesGridViewRowDataBound"
                    OnInit="CurvesGridViewInit"
                    DataSourceID="CurvesObjectDataSource"
                    OnDataBinding="CurvesGridViewDataBinding" >
                <PagerSettings Position="Bottom" />
                <RowStyle CssClass="RowStyle" />
                <SelectedRowStyle CssClass="RowSelected" />
                <AlternatingRowStyle CssClass="RowStyleAlternate" />
                <Columns>
                    <asp:TemplateField HeaderText="Base&lt;br&gt;Date">
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" 
                                Text='<%# Common.FormatDate(Eval("BaseDate")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="MarketName" HeaderText="Market<br>Name" HtmlEncode="false" />
                    <asp:BoundField DataField="PricingStructureType" HeaderText="Pricing<br>Structure" HtmlEncode=false />
                    <asp:BoundField DataField="CurveName" HeaderText="Curve<br>Name" HtmlEncode="false" />
                </Columns>
            </asp:gridview>    
            <asp:ObjectDataSource ID="CurvesObjectDataSource" runat="server" 
                SelectMethod="GetCurves" 
                TypeName="Orion.WebViewer.Curve.Business.CurveProvider" EnablePaging="True" 
                SelectCountMethod="GetCurvesCount">
                <SelectParameters>
                    <asp:ControlParameter ControlID="SearchEnabledRadioButton" Name="enabled" PropertyName="Checked" Type="Boolean" />
                    <asp:ControlParameter ControlID="MarketNameTextBox" Name="marketName" PropertyName="Text" Type="String" />
                    <asp:ControlParameter ControlID="PricingStructureDropDownList" Name="pricingStructureType" PropertyName="SelectedValue" Type="String" />
                    <asp:ControlParameter ControlID="IdTextBox" Name="id" PropertyName="Text" Type="String" />
                    <asp:Parameter Name="maximumRows" Type="Int32" />
                    <asp:Parameter Name="startRowIndex" Type="Int32" />
                </SelectParameters>
            </asp:ObjectDataSource>
            </td>
            <td>&nbsp;</td>
            <td valign="top">
                <asp:DetailsView ID="CurveDetailsView" runat="server" 
                    AutoGenerateRows="False" DataSourceID="CurveObjectDataSource" 
                    DataKeyNames="Id" HeaderText="Curve" >
                    <RowStyle CssClass="RowStyle" />
                    <AlternatingRowStyle CssClass="RowStyleAlternate" />
                    <FieldHeaderStyle CssClass="th" />
                    <HeaderStyle CssClass="th" />
                <Fields>
                    <asp:BoundField DataField="PricingStructureType" HeaderText="Pricing Structure" />
                    <asp:BoundField DataField="Id" HeaderText="ID" />
                    <asp:TemplateField HeaderText="Build DateTime">
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Common.FormatDateTime(Eval("BuildDateTime")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Base Date">
                        <ItemTemplate>
                            <asp:Label ID="Label2" runat="server" Text='<%# Common.FormatDate(Eval("BaseDate")) %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="MarketName" HeaderText="Market Name" />
                    <asp:BoundField DataField="CurveName" HeaderText="Curve Name" />
                    <asp:BoundField DataField="Algorithm" HeaderText="Algorithm" />
                    <asp:BoundField DataField="Currency" HeaderText="Currency" />
                    <asp:BoundField DataField="Domain" HeaderText="Domain" />
                    <asp:BoundField DataField="Id" HeaderText="Curve"
                        ReadOnly="True" SortExpression="Fpml" 
                        DataFormatString="&lt;a target=_new href=&quot;javascript: void(0)&quot; onclick=&quot;window.open('MarketFpml.aspx?id={0}','marketFpml','status=0,toolbar=0,resizable=1,scrollbars=1');return false;&quot;&gt;FpML&lt;/a&gt;" 
                        HtmlEncodeFormatString="False" />
                </Fields>
                </asp:DetailsView>
                <br />
                <ncwc:NChartControl ID="Chart" runat="server" Width="500px" Height="350px" 
                    Visible="False" BorderStyle="None" />                
                <br />
                <button onclick="ClipBoard();" style="<%=ClipboardButtonStyle%>">Copy to Clipboard</button>
                <asp:Table ID="DataTable" runat="server" style="border-collapse:collapse;" border="1"/>
                <asp:ObjectDataSource ID="CurveObjectDataSource" runat="server" 
                    SelectMethod="GetCurve" TypeName="Orion.WebViewer.Curve.Business.CurveProvider">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="CurvesGridView" Name="id" 
                            PropertyName="SelectedValue" Type="String" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </td>
        </tr>
        </table>
    </div>
    <asp:RadioButton ID="SearchEnabledRadioButton" Checked="false" runat="server" Visible="false" />
    </form>
    <textarea id="holdtext" style="display:none;" cols="2" rows="2"><asp:Literal runat="server" id="ClipboardStore"/></textarea>    
</body>
</html>
