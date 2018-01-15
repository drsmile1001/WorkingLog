function Home() {
    this.data = [];
    var page = this;
    var $page = $("[data-page='home']");
    this.initial = () => {
        page.loadData();
    };
    this.loadData = () => {
        $.ajax({
            url: window.Router.action("Home","IndexModel"),
            dataType: "JSON",
            success: response => {
                console.log(response);
            }
        });
    }

}

var homeInstance = new Home();
$(() => {
    homeInstance.initial();
})