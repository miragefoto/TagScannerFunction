<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cTags.aspx.cs" Inherits="cTagInventoryDotNet.cTags" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <title>Tag Dashboard</title>

    <link href="Includes/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/v/bs/jszip-2.5.0/dt-1.10.16/b-1.4.2/b-html5-1.4.2/b-print-1.4.2/r-2.2.0/sc-1.4.3/datatables.min.css" />
    <link href="Includes/font-awesome/css/font-awesome.min.css" rel="stylesheet" />

    <style>
        .form-control {
            overflow-wrap: normal;
        }

        .MargRt20 {
            margin-right: 20px;
        }
    </style>
    <link href="Includes/LocalStyle.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">

        <asp:Panel runat="server" ID="pnlMessage" Visible="false" CssClass="modal fade" role="dialog">
            <div class="modal-dialog modal-lg">
                <!-- Modal content-->
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">ERROR</h4>
                </div>
                <div class="modal-body alert-danger">
                    <asp:Label runat="server" ID="lblMessage" Text="Please Choose"></asp:Label>
                </div>
                <div class="modal-footer">
                    <button data-dismiss="modal" onclick="return false;" class="btn btn-warning pull-right">Cancel</button>
                </div>
            </div>
        </asp:Panel>

        <%--Main Page--%>
        <asp:Panel runat="server" ID="pnlMain">

            <%---------------------------------ALL THE MODALS-----------------------------%>

            <!-- TagModal Modal  -->
            <div id="TagModal" class="modal fade" role="dialog">
                <div class="modal-dialog modal-lg">
                    <!-- Modal content-->
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title">Modify Tag</h4>
                        </div>
                        <div class="modal-body">
                            <asp:HiddenField runat="server" ID="hidPkey" />
                            <asp:HiddenField runat="server" ID="hidEdit"/>
                            <asp:HiddenField runat="server" ID="hidLocation" />
                            <div class="container-fluid">
                                <%--ROW 1--%>


                                <div class="col-md-6 col-xs-12">
                                    <div class="form-group">
                                        <label>Tag</label>
                                        <asp:Label runat="server" ID="lblTag" Text="" CssClass="form-control"></asp:Label>
                                    </div>
                                </div>

                                <div class="col-md-6 col-xs-12">
                                    <div class="form-group">
                                        <label>Location</label>
                                        <asp:Label runat="server" ID="lblLocation" CssClass="form-control"></asp:Label>
                                    </div>
                                </div>

                                <%--ROW 2--%>

                                <div class="col-xs-12">
                                    <div class="form-group">
                                        <label>Status</label>
                                        <asp:DropDownList runat="server" ID="ddlStatus" CssClass="form-control" DataTextField="GlobalValue" DataValueField="GlobalPkey"  >
                                        <%--    <asp:ListItem Text="Please Select" Value="" ></asp:ListItem>
                                            <asp:ListItem Text="Pending" Value="2"></asp:ListItem>
                                            <asp:ListItem Text="Reconciled" Value="3"></asp:ListItem>
                                            <asp:ListItem Text="Not Found" Value="4"></asp:ListItem>--%>
                                        </asp:DropDownList>
                                    </div>
                                </div>

                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:Button runat="server" ID="btnMarkCompleteProxy" OnClientClick="Waiting(); MarkComplete();" data-dismiss="modal" Text="Save" CssClass="btn pull-left btn-success" />
                            <asp:Button runat="server" ID="btnCancelContact" data-dismiss="modal" Text="Cancel" OnClientClick="return false;" CssClass="btn btn-warning pull-right" />
                        </div>
                    </div>
                </div>
            </div>
            <asp:Button runat="server" ID="btnMarkComplete" CssClass="btn btn-success hidden markComplete" Text="Save" OnClick="btnMarkComplete_Click" />
            <!-- End Contact Modal -->




            <%--------------------------------Main Page Content-----------------------------%>
            <div class="continer-fluid">
                <asp:Literal ID="lit" runat="server"></asp:Literal>
                <div class="panel panel-info panel-responsive ">
                    <div class="panel-heading" style="min-height: 60px;">
                        <div class="panel-title">
                            <h4 class="col-sm-2">Tag Data</h4>
                            <div id="btns"></div>
                        </div>
                    </div>
                    <div class="panel-body">
                        <table id="tagTable" class="table table-responsive responsive table-striped" width="100%">
                            <thead>
                                <tr>
                                    <th>Tag No</th>
                                    <th>Location</th>
                                    <th>Status</th>
                                    <th class="">Scan Date</th>
                                    <th>User</th>
                                    <th class="hidden">Status</th>
                                    <th data-priority="10" class="noSort"></th>
                                </tr>
                            </thead>
                            <tbody>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>


        </asp:Panel>


    </form>

    <script src="Includes/jquery/jquery.min.js"></script>
    <script src="Includes/bootstrap/js/bootstrap.min.js"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.1.32/pdfmake.min.js"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.1.32/vfs_fonts.js"></script>
    <script type="text/javascript" src="https://cdn.datatables.net/v/bs/jszip-2.5.0/dt-1.10.16/b-1.4.2/b-html5-1.4.2/b-print-1.4.2/r-2.2.0/sc-1.4.3/datatables.min.js"></script>
    <script src="Includes/moment-with-locales.js"></script>
    <script type="text/javascript">

        var tagTable;
        var dateFormat = 'MM/DD/YYYY';

        $(function () {
            pageLoad();
        });

        function pageLoad(sender, args) {
            $("body").css("cursor", "default");

            tagTable = $('#tagTable').DataTable({
                "autoWidth": false,
                responsive: true,

                "columnDefs": [{
                    "targets": "noSort",
                    "orderable": false,
                    "searchable": false,
                    select: "single"
                },
                {
                    "targets": "noShow",
                    "orderable": false,
                    "visible": false,

                    select: "single"
                }
                ],
                "ajax": {
                    url: "https://cc-tagscanner-functionapp20181003103414.azurewebsites.net/api/GetScannedTags",
                    "type": "GET",
                    "dataType": "JSON",
                    "error": function (error) {  // error handling
                        $(".dataTable-error").html("");
                        $("#dataTable").append('<tbody class="dataTable-error"><tr><th colspan="3">No data found in the server</th></tr></tbody>');
                        $("#dataTable_processing").css("display", "none");
                        console.log("error: " + error.error);
                    },
                    "dataSrc": function (json) {
                        var return_data = json;
                            $("body").css("cursor", "default");
                            return return_data;
                    }
                },
                columns: [
                    { data: "TagNo" },
                    { data: "Location" },
                    { data: "GlobalValue" },
                    {
                        data: "ValidFrom",
						//"className":"hidden noShow",
                        render: function (data, type, row) {
                        if (data !== null || type === "display" || type === "filter") {
                            return moment(data, "'YYYY-MM-DD HH:mm:ss'").isValid() ?
                                moment(data, "'YYYY-MM-DD HH:mm:ss'").format("MM/DD/YYYY")
                                :data;
                        }
                        else {
                            return "";
                        }
                    }

                    },
                    {
                        data:"EditBy"
                    },
                    {
                        data: "Status",
                        "className": "noShow hidden"
                    },
                    {
                        data: null,
                        defaultContent: '<button class="btn btn-sm btn-success btnEdit" data-toggle="model" data-target="#TagModal" onclick="return false;">Edit</button>'
                    }

                ],
                stateSave: true,
                buttons: {
                    buttons: [
                        {
                            extend: 'print',
                            title: 'Tags List',
                            className: 'btnHidePrint btn btn-sm btn-info pull-right',
                            autoPrint: true,
                            exportOptions: { columns: [0, 1, 2, 3] },
                            message: "Print Date: " + moment().format('LL'),
                            customize: function (win) {
                                $(win.document.body)
                                    .css('font-size', '10pt')
                                    .css('margin', '10pt');

                                $(win.document.body).find('table')
                                    .addClass("nowrap")
                                    .addClass('table-striped')
                                    .css('font-size', '10pt')
                                    .css('width', '100%');
                                $(win.document.body).find('table tr')
                                    .addClass("underline")
                                    .css("margin-bottom", "15px");
                            }
                        },
                        {
                            extend: 'csv',
                            title: 'Tag List',
                            className: 'btnHidePrint btn btn-sm btn-warning pull-right MargRt20',
                            exportOptions: { columns: [0, 1, 2, 3] }

                        }

                    ]
                },
                "fnDrawCallback": function () {
                    $('#tagTable tbody tr button').click(function () {
                        var data = tagTable.row($(this).parents('tr')).data();
                        FillTag(data);

                    });
                }

            });

        tagTable.buttons().container().appendTo($("#btns"));

        }

        function FillTag(data) {
            //Clear existing data
            ClearTag();
            console.log(data);

            var tag = data["TagNo"];
            var location = data["Location"];
            var status = data["Status"];
            var statusDesc = data["GlobalValue"];
            var scanDate = data["ValidFrom"];
            var editBy = data["EditBy"]
            // fill modal fields

            //drop down lists
            $('#<%= ddlStatus.ClientID%> option').filter(function () {
                return $(this).text() === statusDesc;
            }).prop('selected', true);

            console.log("Tag" + tag);
            //Text Boxes
            $('#<%=hidPkey.ClientID%>').val(tag);
            $('#<%=hidEdit.ClientID%>').val(editBy);
            $('#<%=hidLocation.ClientID%>').val(location);
            $('#<%=lblTag.ClientID%>').html(tag);
            $('#<%=lblLocation.ClientID%>').html(location);

            $('#TagModal').modal("show");

        }


        function ClearTag() {
            //drop down lists
            $('#<%= ddlStatus.ClientID%> option').filter(function () {
                return $(this).text() === "Please Select";
            }).prop('selected', true);

            //Text Boxes
            $('#<%=hidPkey.ClientID%>').val("");
            $('#<%=lblTag.ClientID%>').text('');
            $('#<%=lblLocation.ClientID%>').text('');
        }



        function MarkComplete() {

            if ($('#<%= ddlStatus.ClientID%>').val() === "") {
                alert('Pick something slacker!');
                return false;
            }
            else {

            
            $('#<%= btnMarkComplete.ClientID%>').click();
            }
            

        }

        function Waiting() {
            $("body").css("cursor", "progress");
        }

    </script>


</body>
</html>
