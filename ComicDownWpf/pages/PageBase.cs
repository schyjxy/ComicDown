using ComicDownWpf.decoder;
using System.Collections.Generic;
using System.Windows.Controls;
using ComicPlugin;
using System.Windows.Input;

namespace ComicDownWpf.pages
{
    public class PageBase : Page
    {
        public List<ComicDetailInfo> ComicInfoList { get; set; }
        public ICommand LoadItemCommand { get; set; }

        public virtual void LoadItem()
        {

        }
    }
}
