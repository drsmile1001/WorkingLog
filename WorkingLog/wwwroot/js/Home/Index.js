function Home() {
    this.data = [];
    this.addData = {};
    var page = this;
    var $page = $("[data-page='home']");
    this.initial = () => {
        page.loadData();
        $page.find("[data-panel='add'] input").change(function() {
            var $this = $(this);
            var field = $this.attr("data-field");
            page.addData[field] = $this.val();
        });
        
        $page.find("[data-action='add']").click(() => {
            $.ajax({
                url: window.Router.action("Home", "add"),
                data: page.addData,
                dataType: "JSON",
                success: response => {
                    if (!response.success)
                        alert("失敗");
                    else
                        page.loadData();
                }
            });
        });
    };
    this.loadData = () => {
        $.ajax({
            url: window.Router.action("Home","IndexModel"),
            dataType: "JSON",
            success: response => {
                page.data = response;
                page.showData();
            }
        });
    }
    this.showData = () => {
        var $tbody = $page.find("[data-table='result'] tbody").empty();
        page.data.forEach(item => {
            var $row = $page.find("[data-template='row']").children().clone();
            $row.find("[data-field]").each(function() {
                var $this = $(this);
                var field = $this.attr("data-field");
                $this.text(item[field]);
            });
            $tbody.append($row);
        });
        
    }

}

var homeInstance = new Home();
$(() => {
    homeInstance.initial();
})