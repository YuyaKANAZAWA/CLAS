using Microsoft.AspNetCore.Components;

namespace GritZ3.Components.Cards.GeneralDispCard
{
    public partial class GeneralDispCard
    {

        [Parameter]
        public DispData? Data { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await Task.Run(() => SetDisp());
        }
        private void SetDisp()
        {

        }
    }

    public class DispData
    {
        public string Title { get; set; } = "";
        public List<TagInfo> TagInfos { get; set; } = new List<TagInfo>();
        public List<Content> Contents { get; set; } = new List<Content>();

        public class TagInfo
        {
            public string Color { get; set; } = "";
            public string Title { get; set; } = "";
        }

        public class Content
        {
            public string Title { get; set; } = "";
            public string Value { get; set; } = "";
        }
    }

}
