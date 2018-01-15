function Home() {
    this.data = [];
    this.projects = [];
    this.addData = {
        hours:1
    };
    var page = this;
    var $page = $("[data-page='home']");
    var $projectSuggestions = $("#projectSuggestions");
    var $typeSuggestions = $("#typeSuggestions");
    var $workItemSuggestions = $("#workItemSuggestions");
    this.initial = () => {
        page.loadData();
        $page.find("[data-panel='add'] input").change(function () {
            var $this = $(this);
            var field = $this.attr("data-field");
            page.addData[field] = $this.val();
        });

        $page.find("[data-panel='add'] input[data-field='project']").change(function () {
            $workItemSuggestions.empty();
            page.projects = Lazy(page.data)
                .filter(item=>item.project === $(this).val())
                .map(item => item.workItem)
                .filter(item => item)
                .unique().toArray()
                .forEach(item => $workItemSuggestions.append($(`<option value='${item}'>`)));
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
            $row.find("[data-action='delete']").click(() => {
                page.delete(item.id);
            });
            $tbody.append($row);
        });

        $projectSuggestions.empty();
        page.projects = Lazy(page.data)
            .map(item => item.project)
            .filter(item => item)
            .unique().toArray()
            .forEach(item => $projectSuggestions.append($(`<option value='${item}'>`)));
        $typeSuggestions.empty();
        page.projects = Lazy(page.data)
            .map(item => item.type)
            .filter(item => item)
            .unique().toArray()
            .forEach(item => $typeSuggestions.append($(`<option value='${item}'>`)));
    };
    this.delete = id => {
        $.ajax({
            url: window.Router.action("Home", "Delete"),
            data: { id: id },
            dataType: "JSON",
            success: response => {
                if (!response.success)
                    alert("失敗");
                else
                    page.loadData();
            }
        });
    }

}

var homeInstance = new Home();
$(() => {
    homeInstance.initial();
})