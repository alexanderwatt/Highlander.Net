<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DatePicker.ascx.cs" Inherits="Orion.WebViewer.Controls.DatePicker" %>

<script type="text/javascript" >

    function toggleCalendar() 
    {
        var calendarPopup = document.getElementById("<%=calendarPopup.ClientID %>");
        if (calendarPopup.style.display == 'none') 
        {
            calendarPopup.style.display = '';
        }
        else 
        {
            calendarPopup.style.display = 'none';
        }
    }

</script>

<asp:TextBox ID="dateTextBox" runat="server" ReadOnly="true" CssClass="datePickerControl" onclick="toggleCalendar();" />

<div id="calendarPopup" runat="server" style="float:left;position:absolute">
    <asp:Calendar ID="Calendar" runat="server" CssClass="calendar" DayHeaderStyle-CssClass="primaryNavigation" DayStyle-CssClass="calendarDays"
        onselectionchanged="CalendarSelectionChanged"  onvisiblemonthchanged="CalendarVisibleMonthChanged" />
</div>
    
